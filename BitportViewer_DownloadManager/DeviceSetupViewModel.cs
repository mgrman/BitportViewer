using BitportViewer_SSH;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace BitportViewer_DownloadManager
{
    [ImplementPropertyChanged]
    class DeviceSetupViewModel
    {
        public DeviceSetupViewModel()
        {
#if DEBUG

            DownloadPath = "/var/media/My Book";

            DeviceHostName = "libreelec";
            DeviceUserName = "root";
            DevicePassword = "libreelec";
#endif
        }
        
        
        public string DeviceHostName { get; set; }
        
        public string DeviceUserName { get; set; }
        
        public string DevicePassword { get; set; }
        
        public string DownloadPath { get; set; }
        

        public DeviceSetup GetModel()
        {
            return new DeviceSetup(DeviceHostName, DeviceUserName, DevicePassword, DownloadPath);
        }
        
    }
}
