using System;
using System.Collections.Generic;

namespace BitportViewer_SSH
{
    public interface IDownloadManager
    {
        IReadOnlyCollection<DownloadState> Downloads { get; }
        bool IsConnected { get; }
        DeviceSetup Setup { get; }
        
        void Reconect();
        void StartDownloadingFile(DownloadInfo info);
        void StopDownloadingFile(int pid);
        void StopDownloadingFile(DownloadState state);
    }
}