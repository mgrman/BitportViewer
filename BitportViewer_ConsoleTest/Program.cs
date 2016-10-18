using BitportViewer.Common;
using Newtonsoft.Json;
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

            var token=auth.GetToken();

            var tokenPath = Path.Combine(Environment.CurrentDirectory, "bitportToken.json");
            File.WriteAllText(tokenPath,JsonConvert.SerializeObject(token));
        }
    }
    
}
