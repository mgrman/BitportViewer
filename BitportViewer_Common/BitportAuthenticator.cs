using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace BitportViewer.Common
{
    public class BitportAuthenticator
    {
        public BitportAppSettings Settings { get; }

        public BitportAppToken Token { get; }

        public BitportAuthenticator(BitportAppSettings appSettings)
        {
            Settings = appSettings;
        }

        public BitportAppToken GetToken()
        {
            BitportAppToken token = new BitportAppToken();

            string code = "";

            while (code.IsNullOrEmpty())
            {
                PlatformService.OpenBrowser(string.Format("https://api.bitport.io/v2/oauth2/authorize?response_type=code&client_id={0}", Settings.AppId));
                
                var request = PlatformService.ListenOnLocalPort(Settings.LocalhostCallbackPort);
                
                code = request["code"];
            }

            while (token.IsInvalid())
            {
                using (var client = new HttpClient())
                {
                    UriBuilder builder = new UriBuilder();
                    builder.Scheme = "https";
                    builder.Host = "api.bitport.io";
                    builder.Path = "/v2/oauth2/access-token";
                    var body = new Dictionary<string, string>();
                    body["client_id"] = Settings.AppId;
                    body["grant_type"] = "authorization_code";
                    body["client_secret"] = Settings.SecretKey;
                    body["code"] = code;
                    string url = builder.ToString();

                    var resultText = client.PostAsync(url, new FormUrlEncodedContent(body)).Result?.Content?.ReadAsStringAsync().Result;
                    token = JsonConvert.DeserializeObject<BitportAppToken>(resultText);

                }
            }
            return token;
        }
    }
}
