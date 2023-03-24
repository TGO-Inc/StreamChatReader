using System;
using System.IO;
using System.Linq;
using System.Timers;
using System.Reflection;
using System.Diagnostics;
using StreamingServices.Chat;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.Threading;
using UltralightSharp.Safe;
using System.Net.NetworkInformation;

namespace StreamingServices.Youtube
{
    internal class YoutubeChatReader : IReaderBase, IDisposable
    {
        #region InterfaceContext
        public StreamType StreamType { get; init; }
        public string StreamId { get; init; }
        public event ChatEventHandler? ChatEvent;
        public event ChatLoadedHandler? ChatLoaded;
        #endregion

        #region BrowserContext
        private readonly DirectoryInfo CachePath;
        private readonly string BaseDirectory = new Uri(Environment.CurrentDirectory).LocalPath;
        private readonly System.Timers.Timer MessageTimer;
        private static readonly string BaseUrl = $"https://www.youtube.com/live_chat?is_popout=1&v=";
        private readonly string JavaScript = string.Empty;
        private bool StreamLinked = false;
        private readonly ThreadSafeView Page;
        #endregion

        #region ClassContext
        private void OnChatEvent(ChatEventArgs e) => Task.Run(()=>ChatEvent?.Invoke(e));
        private void OnChatLoaded(ChatLoadedArgs e) => Task.Run(()=>ChatLoaded?.Invoke(e));
        #endregion

        #region Initialize
        public YoutubeChatReader(StreamType StreamType, string StreamId)
        {
            this.StreamType = StreamType;
            this.StreamId = StreamId;

            this.MessageTimer = new()
            {
                Interval = 250,
                AutoReset = true
            };
            this.MessageTimer.Elapsed += GetMessages;

            // Load JavaScript for injection
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("YoutubeChatEvent.js"));
            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream is not null)
                    using (StreamReader reader = new(stream))
                        this.JavaScript = reader.ReadToEnd();
            }

            // Find Cache Directory
            do this.CachePath = new(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            while (this.CachePath.Exists || File.Exists(this.CachePath.FullName));
            
            Path.Combine(BaseDirectory, "resources");
            // stuff
            this.Page = new(1, 1, false)
            {
                BaseDirectory= this.BaseDirectory,
                CachePath= CachePath.CreateSubdirectory("Cache").FullName,
                LogFileName="headless-log.txt",
                ResourcePath= Path.Combine(BaseDirectory, "resources"),
                SessionName="StreamSession"
            };
            this.Page.RunAsync();
        }
        #endregion

        #region PageThread
        private void LoadStream()
        {
            this.Page!.LoadUrl(BaseUrl + this.StreamId);
            this.Page?.EvaluateScript(this.JavaScript, (string ret, string exception) =>
            {
                this.StreamLinked = true;
                this.OnChatLoaded(new ChatLoadedArgs());
            });
        }
        private void ReadStream()
        {
            if(!this.MessageTimer.Enabled)
                this.MessageTimer.Start();
        }
        #endregion

        #region ChatControl
        public void StartReadingChat()
        {
            if(!this.StreamLinked)
                LoadStream();
            ReadStream();
        }
        public void PauseReadingChat()
        {
            if (this.MessageTimer.Enabled)
                this.MessageTimer.Stop();
        }
        public void ResumeReadingChat()
        {
            if (this.StreamLinked && !this.MessageTimer.Enabled)
                this.MessageTimer.Start();
        }
        public void StopReadingChat()
        {
            if(this.MessageTimer.Enabled)
                this.MessageTimer.Stop();
        }
        private void GetMessages(object? sender, ElapsedEventArgs e)
        {
            this.Page?.EvaluateScript("window.GetMessageQueue();", (string output, string exception) =>
            {
                if (output.Length > 2 && exception.Length == 0)
                {
                    foreach (JObject jobj in JArray.Parse(output).Cast<JObject>())
                    {
                        var obj = new ChatEventArgs(jobj);

                        this.OnChatEvent(obj);
                    }
                }
                else if (exception.Length > 2)
                {
                    Debug.WriteLine(exception);
                }
            });
        }
        #endregion

        #region Dispose/Cleanup
        // To detect redundant calls
        private bool _disposedValue;
        // Instantiate a SafeHandle instance.
        private readonly SafeHandle _safeHandle = new SafeFileHandle(IntPtr.Zero, true);
        ~YoutubeChatReader()
        {
            this.CachePath.Delete(true);
            this.Dispose(false);
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.CachePath.Delete(true);
            this.Dispose(true);
            GC.ReRegisterForFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _safeHandle.Dispose();
                }

                _disposedValue = true;
            }
        }
        #endregion
    }
}
