using BitportViewer_SSH;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitportViewer_DownloadManager
{
    [ImplementPropertyChanged]
    class DownloadInfoViewModel
    {
        public DownloadInfoViewModel()
        {
#if DEBUG

            Url = "https://fra2.bitport.io/246wccyvtud1pznnwr6dp8y32ujbkb4x";
            Name = "The.Exorcist.S01E06.HDTV.x264-FUM[ettv].mp4";
#endif
        }

        public string Url { get; set; }
        public string Name { get; set; }

        public DownloadInfo GetModel()
        {
       
                if (string.IsNullOrEmpty(Name))
                    return new DownloadInfo(Url);
                else
                    return new DownloadInfo(Url, Name);
            
        }
    }
}
