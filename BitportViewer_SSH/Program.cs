using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BitportViewer_SSH
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = "/var/media/My Book";
            string url = "https://prg1.bitport.io/246wccyvtud1pznnwr6dp8y32ujbkb4x";
            string name = "The.Exorcist.S01E06.HDTV.x264-FUM[ettv].mp4";

            string deviceHostName = "libreelec";
            string deviceUserName = "root";
            string devicePassword = "####";


            var setup = new DeviceSetup(deviceHostName, deviceUserName, devicePassword, path);


            using (var manager = new DownloadManager(setup))
            {
                var download = new DownloadInfo(url, name);

                manager.StartDownloadingFile(download);


                foreach(var status in manager.Downloads)
                {
                    Console.WriteLine(status);
                }
            }
           
        }
    }
    
}
