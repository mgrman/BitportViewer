using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace BitportViewer.Common
{
    public class BitportAppSettings
    {
        public string AppId { get; }

        public string SecretKey { get; }

        public int LocalhostCallbackPort { get; }
        

        public BitportAppSettings(string appId, string secretKey, int port)
        {
            AppId = appId;
            SecretKey = secretKey;
            LocalhostCallbackPort = port;
        }
    }
    
}
