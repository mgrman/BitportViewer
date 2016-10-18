using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace BitportViewer.Common
{
    public interface IPlatformService
    {
        void OpenBrowser(string url);

        IReadOnlyDictionary<string,string> ListenOnLocalPort(int port);
    }

    public static class PlatformService
    {
        public static IPlatformService Service { get; private set; }

        public static void Initialize(IPlatformService service)
        {
            Service = service;
        }


        public static void OpenBrowser(string url)
        {
            if (Service == null)
                throw new InvalidOperationException();

            Service.OpenBrowser(url);
        }

        public static IReadOnlyDictionary<string, string> ListenOnLocalPort(int port)
        {

            if (Service == null)
                throw new InvalidOperationException();

            return Service.ListenOnLocalPort(port);
        }
    }
}
