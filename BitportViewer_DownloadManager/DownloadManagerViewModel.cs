using BitportViewer_SSH;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BitportViewer_DownloadManager
{
    [ImplementPropertyChanged]
    class DownloadManagerViewModel : IDisposable
    {
        private Task _pollConnectionTask;

        private CancellationTokenSource _pollConnectionCts = new CancellationTokenSource();

        public DownloadManagerViewModel()
        {
            DeviceSetup = new DeviceSetupViewModel();
            (DeviceSetup as INotifyPropertyChanged).PropertyChanged += (o, e) =>
            {
                UpdateModel();
            };

            _pollConnectionTask = Task.Run(async () =>
            {
                while (!_pollConnectionCts.IsCancellationRequested)
                {
                    try
                    {
                        UpdateFromDomain();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error updating info : {ex.GetType().Name} - {ex.Message}");
                    }
                    await Task.Delay(1000);
                }
            });

            Reconnect = new RelayCommand<object>(o =>
            {
                try
                {
                    Model?.Reconect();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error connecting to device : {ex.GetType().Name} - {ex.Message}");
                }
            }, o => Model != null && !IsConnected);
            StartDownloadingFile = new RelayCommand<DownloadInfoViewModel>(o => Model?.StartDownloadingFile(o.GetModel()), o => o != null && IsConnected);
            StopDownloadingFile = new RelayCommand<DownloadState>(o => Model?.StopDownloadingFile(o), o => o != null && o.Status == DownloadState.Statuses.Active && IsConnected);
            UpdateModel();
        }

        public DownloadManager Model { get; private set; }

        public DeviceSetupViewModel DeviceSetup { get; }

        public IReadOnlyCollection<DownloadState> Downloads { get; private set; }

        public bool IsConnected { get; private set; }

        public ICommand Reconnect { get; }
        public ICommand StartDownloadingFile { get; }
        public ICommand StopDownloadingFile { get; }

        private void UpdateModel()
        {
            if (Model != null)
            {
                var oldModel = Model;
                Model = null;
                oldModel.Dispose();
            }



            try
            {
                Model = new DownloadManager(DeviceSetup.GetModel());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to device : {ex.GetType().Name} - {ex.Message}");
            }

            UpdateFromDomain();
        }

        private void UpdateFromDomain()
        {
            IsConnected = Model?.IsConnected ?? false;

            IReadOnlyCollection<DownloadState> newDownloads;
            if (Model != null && IsConnected)
            {
                newDownloads = Model.Downloads ?? Array.Empty<DownloadState>();
            }
            else
            {
                newDownloads = Array.Empty<DownloadState>();
            }
            var oldDownloads = Downloads ?? Array.Empty<DownloadState>();

            if (!oldDownloads.SequenceEqual(newDownloads))
                Downloads = newDownloads;
        }

        public void Dispose()
        {
            _pollConnectionCts.Cancel();
        }
    }
}
