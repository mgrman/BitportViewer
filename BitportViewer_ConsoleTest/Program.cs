using BitportViewer.Common;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BitportViewer.ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Win32PlatformService.Initialize();


            var tokenPath = Path.Combine(Environment.CurrentDirectory, "bitportToken.json");
            var token = File.Exists(tokenPath) ? JsonConvert.DeserializeObject<BitportAppToken>(File.ReadAllText(tokenPath)) : null;
            if ((token?.access_token).IsNullOrEmpty())
            {

                var appSettingsPath = Path.Combine(Environment.CurrentDirectory, "appSettings.json");

                BitportAppSettings appSettings;
                if (File.Exists(appSettingsPath))
                {
                    appSettings = JsonConvert.DeserializeObject<BitportAppSettings>(File.ReadAllText(appSettingsPath));
                }
                else
                {
                    throw new FileNotFoundException($"Settings file at \"{appSettingsPath}\" is missing!");
                }

                var auth = new BitportAuthenticator(appSettings);

                token = auth.GetToken();

                File.WriteAllText(tokenPath, JsonConvert.SerializeObject(token));
            }


            var client = new RestClient("https://api.bitport.io/v2/");
            client.Authenticator = new RestSharp.Authenticators.OAuth2AuthorizationRequestHeaderAuthenticator(token.access_token, token.token_type);
            client.AddHandler("application/json", new JsonSerializer());

            //var result = client.Get<BP_CloudFolder>("/search/a" );


            var files = GetFiles(client, null).ToArray();

            var fileNames=string.Join("\r\n", files.Select(o=>o.name));

            var links = files.Select(o => GetLink(client, o, true))
                .ToArray();

        }

        private static IEnumerable<BP_File> GetFiles(RestClient client, string folderCode)
        {

            var result = client.Get<BP_CloudFolder>(folderCode.IsNullOrEmpty() ? "/cloud" : $"/cloud/{folderCode}");

            var folder = result.data[0];

            List<BP_File> files = new List<BP_File>();
            files.AddRange(folder.files);

            foreach (var file in folder.files)
            {
                yield return file;
            }

            foreach (var subFolder in folder.folders)
            {

                foreach (var file in GetFiles(client, subFolder.code))
                {
                    yield return file;
                }
            }
        }



        private static string GetLink(RestClient client, BP_File file, bool converted)
        {
            return GetLink(client, file.code, converted);
        }

        private static string GetLink(RestClient client, string fileCode, bool converted)
        {
            return client.Get_LinkOnly($"/files/{fileCode}/{(converted ? "stream" : "download")}");
        }



        public class BP_CloudFolder : BP_ItemData
        {
            public BP_SubFolder[] folders { get; set; }
            public BP_File[] files { get; set; }
        }

        public class BP_SubFolder : BP_ItemData
        {

        }

        public class BP_File : BP_ItemData
        {
            public string crc32 { get; set; }
            public BP_Created_At created_at { get; set; }
            public object parent_folder_code { get; set; }
            public bool? video { get; set; }
            public string conversion_status { get; set; }
        }

        public class BP_Created_At
        {
            public DateTime date { get; set; }
            public int? timezone_type { get; set; }
            public string timezone { get; set; }
        }

        public abstract class BP_ItemData
        {
            public string name { get; set; }
            public string code { get; set; }
            public string size { get; set; }
        }
    }


    public static class RestRequestExtensions
    {
        public static BitPortResponse<T> Get<T>(this RestClient client, string path)
            where T : class, new()
        {

            var response = client.Request<BitPortResponse<T>>(path, Method.GET);
            response.Validate();
            return response?.Data;
        }

        public static string Get_LinkOnly(this RestClient client, string path)
        {
            var response = client.Request(path, Method.GET, request =>
            {
                request.AddHeader("Range", "bytes=0-0");
            });

            return response.ResponseUri.ToString();
        }


        private static IRestResponse Request(this RestClient client, string path, Method method, Action<RestRequest> preprocess = null)
        {
            var request = PrepareRequest(path, method, preprocess);

            var response = client.Execute(request);

            return response;
        }



        private static IRestResponse<T> Request<T>(this RestClient client, string path, Method method, Action<RestRequest> preprocess = null)
            where T : class, new()
        {
            var request = PrepareRequest(path, method, preprocess);

            var response = client.Execute<T>(request);
            return response;
        }

        private static RestRequest PrepareRequest(string path, Method method, Action<RestRequest> preprocess = null)
        {

            var request = new RestRequest(path, method);
            request.RequestFormat = DataFormat.Json;

            preprocess?.Invoke(request);
            return request;
        }

        public static void Validate<T>(this IRestResponse<BitPortResponse<T>> response)
            where T : class, new()
        {
            if (response == null)
            {
                throw new InvalidOperationException();
            }

            switch (response.ContentType)
            {
                case "application/json":
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new InvalidOperationException();
                    }

                    if (response.Data == null || response.Data.status != Status.Success || (response.Data.errors != null && response.Data.errors.Any()))
                    {
                        throw new InvalidOperationException();
                    }
                    break;
                case "application/octet-stream":
                    if (response.StatusCode != HttpStatusCode.PartialContent)
                    {
                        throw new InvalidOperationException();
                    }
                    if (response.ContentLength < 0)
                    {
                        throw new InvalidOperationException();
                    }
                    break;
                default:
                    break;
            }

        }
    }



    public class JsonSerializer : ISerializer, IDeserializer
    {
        private readonly Newtonsoft.Json.JsonSerializer _serializer;

        /// <summary>
        /// Default serializer
        /// </summary>
        public JsonSerializer()
        {
            ContentType = "application/json";
            _serializer = new Newtonsoft.Json.JsonSerializer
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Include,
                DefaultValueHandling = DefaultValueHandling.Include
            };
        }

        /// <summary>
        /// Default serializer with overload for allowing custom Json.NET settings
        /// </summary>
        public JsonSerializer(Newtonsoft.Json.JsonSerializer serializer)
        {
            ContentType = "application/json";
            _serializer = serializer;
        }

        /// <summary>
        /// Serialize the object as JSON
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <returns>JSON as String</returns>
        public string Serialize(object obj)
        {
            using (var stringWriter = new StringWriter())
            {
                using (var jsonTextWriter = new JsonTextWriter(stringWriter))
                {
                    jsonTextWriter.Formatting = Formatting.Indented;
                    jsonTextWriter.QuoteChar = '"';

                    _serializer.Serialize(jsonTextWriter, obj);

                    var result = stringWriter.ToString();
                    return result;
                }
            }
        }

        public T Deserialize<T>(IRestResponse response)
        {
            return JsonConvert.DeserializeObject<T>(response.Content);
        }

        /// <summary>
        /// Unused for JSON Serialization
        /// </summary>
        public string DateFormat { get; set; }
        /// <summary>
        /// Unused for JSON Serialization
        /// </summary>
        public string RootElement { get; set; }
        /// <summary>
        /// Unused for JSON Serialization
        /// </summary>
        public string Namespace { get; set; }
        /// <summary>
        /// Content type for serialized content
        /// </summary>
        public string ContentType { get; set; }
    }

}
