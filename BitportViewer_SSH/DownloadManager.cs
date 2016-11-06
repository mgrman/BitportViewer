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

    public class DeviceSetup
    {

        public string DeviceHostName { get; }
        public string DeviceUserName { get; }
        public string DevicePassword { get; }

        public string DownloadPath { get; }

        public DeviceSetup(string deviceHostName, string deviceUserName, string devicePassword, string downloadPath)
        {
            DeviceHostName = deviceHostName;
            DeviceUserName = deviceUserName;
            DevicePassword = devicePassword;

            DownloadPath = downloadPath;
        }
    }

    public class DownloadInfo
    {
        public string Url { get; }
        public string Name { get; }

        public DownloadInfo(string url, string name)
        {
            Url = url;
            Name = name;
        }

        public DownloadInfo(string url)
        {
            Url = url;

            Uri uri = new Uri(url);
            Name = Path.GetFileName(uri.LocalPath);
        }
    }

    public class DownloadState : IEquatable<DownloadState>
    {
        public enum Statuses
        {
            Unknown = 0,
            Active,
            Finished
        }

        public Statuses Status { get; }

        public int Completion { get; }
        
        public string Path { get; }
        public int? Pid { get; }

        private DownloadState(Statuses status, string path, int? pid, int completion)
        {
            Status = status;
            Path = path;
            Pid = pid;
            Completion = completion;
        }

        public static DownloadState CreateActive(string path, int pid, int completion)
        {
            return new DownloadState(Statuses.Active, path, pid, completion);
        }

        public static DownloadState CreateFinished(string path)
        {
            return new DownloadState(Statuses.Finished, path, null,100);
        }

        public override string ToString()
        {
            return $"Path:\"{Path}\", Status:\"{Status}\", Pid:\"{Pid}\", {Completion}%";
        }

        public bool Equals(DownloadState other)
        {
            if (other == null)
                return false;

            return this.Path == other.Path &&
                this.Status == other.Status &&
                this.Pid == other.Pid &&
                this.Completion == other.Completion;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DownloadState);
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }
    }

    public class DownloadManager : IDisposable, IDownloadManager
    {
        public DeviceSetup Setup { get; }

        private SshClient sshClient { get; }

        private object _communicationLock = new object();

        public DownloadManager(DeviceSetup setup)
        {
            Setup = setup;

            sshClient = new SshClient(setup.DeviceHostName, setup.DeviceUserName, setup.DevicePassword);
            sshClient.ConnectionInfo.Timeout = new TimeSpan(0, 0, 10);

            try
            {
                sshClient.Connect();
            }
            catch (Exception)
            {

            }
        }

        public bool IsConnected
        {
            get
            {
                return sshClient.IsConnected;
            }
        }

        public void Reconect()
        {
            if (sshClient.IsConnected)
                sshClient.Disconnect();
            sshClient.Connect();
        }

        private const string DownloadCommand = "wget -c \"{0}\" -O \"{1}\" 2>&1";
        //private const string DownloadCommand = "curl \"{0}\" --progress-bar -o \"{1}\" 2>&1";
        //private static readonly string DownloadCommandRegex = "^" + Regex.Replace(DownloadCommand, "\"{\\d}\"", "(.*?)") + "$";
        private static readonly string DownloadCommandRegex = "^wget -c (.*?) -O (.*?)$";

        public void StartDownloadingFile(DownloadInfo info)
        {
            ValidateIsConnected();

            string scriptPath = $"{Setup.DownloadPath}/{info.Name}.download.sh";
            string downloadPath = $"{Setup.DownloadPath}/{info.Name}";
            string logPath = $"{Setup.DownloadPath}/{info.Name}.log";

            string script = string.Format("date; " + DownloadCommand + "; echo $?; date;", info.Url.Escape(), downloadPath.Escape());

            string createScriptCommand = $"echo \"{script.Escape()}\" > \"{scriptPath.Escape()}\"";
            RunCommand(createScriptCommand);

            string startDownloadCmd = $"nohup bash \"{scriptPath.Escape()}\" > \"{logPath.Escape()}\" &";
            RunCommand(startDownloadCmd);
        }

        public IReadOnlyCollection<DownloadState> Downloads
        {
            get
            {
                ValidateIsConnected();


                string result = RunCommand("ps");
                var activeDownloads = result.SplitLines()
                    .Select(o => Regex.Match(o, "^ *([^ ]+) *([^ ]+) *([^ ]+) (.*)$"))
                    .Where(o => o.Success)
                    .Select(o => new { pid = o.Groups[1].Value.TryParseInt(), nameMatch = Regex.Match(o.Groups[4].Value, DownloadCommandRegex) })
                    .Where(o => o.pid.HasValue && o.nameMatch.Success)
                    .Select(o => new { pid = o.pid.Value, url = o.nameMatch.Groups[1].Value.TrimQuotes(), path = o.nameMatch.Groups[2].Value.TrimQuotes() })
                    .ToArray();

                result = RunCommand($"cd \"{Setup.DownloadPath}\" && ls");
                var files = result.SplitLines();
                var finishedDownloads = files
                    .Where(file => files.Contains(file + ".log"))
                    .Select(o => $"{Setup.DownloadPath}/{o}")
                    .Except(activeDownloads.Select(o => o.path))
                    .ToArray();


                return Enumerable.Empty<DownloadState>()
                    .Concat(activeDownloads.Select(o => DownloadState.CreateActive(o.path, o.pid, GetCompletion(o.path+".log"))))
                    .Concat(finishedDownloads.Select(o => DownloadState.CreateFinished(o)))
                    .ToArray();
            }
        }

        private int GetCompletion(string logPath)
        {
            string cmd = $"cat \"{logPath}\" | tail -n 5";
            string result = RunCommand(cmd);
            return result
                 .SplitLines()
                 .Select(o => Regex.Match(o, @".* (\d+)% \|\** *\| *(\d*[a-zA-Z]) *(\d+):(\d+):(\d+) ETA"))
                 .Where(o => o.Success)
                 .Select(o => o.Groups[1].Value.TryParseInt())
                 .Where(o => o.HasValue)
                 .Select(o => o.Value)
                 .LastOrDefault();
        }

        public void StopDownloadingFile(DownloadState state)
        {
            if (state.Pid.HasValue)
                StopDownloadingFile(state.Pid.Value);

        }
        public void StopDownloadingFile(int pid)
        {
            ValidateIsConnected();

            RunCommand($"kill {pid}");
        }


        private string RunCommand(string cmd)
        {
            lock (_communicationLock)
            {
                var command = sshClient.CreateCommand(cmd);
                command.CommandTimeout = TimeSpan.FromSeconds(10);
                var asyncRes = command.BeginExecute();
                while (!asyncRes.IsCompleted)
                {
                    Thread.Sleep(100);
                }
                return command.Result;
            }
        }

        private void ValidateIsConnected()
        {
            if (!IsConnected)
                throw new InvalidOperationException("Must be Connected!");
        }

        public void Dispose()
        {
            if (sshClient != null)
            {
                if (sshClient.IsConnected)
                    sshClient.Disconnect();
                sshClient.Dispose();
            }
        }
    }



    public static class Extensions
    {
        public static int? TryParseInt(this string text)
        {
            int result;
            if (Int32.TryParse(text, out result))
                return result;

            return null;
        }

        public static string TrimQuotes(this string text)
        {
            return text.Trim().Trim('"').Trim();
        }

        public static string[] SplitLines(this string text)
        {
            return text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }


        public static string Escape(this string text)
        {
            return text
                .Replace("\"", "\\\"");
        }
    }
}
