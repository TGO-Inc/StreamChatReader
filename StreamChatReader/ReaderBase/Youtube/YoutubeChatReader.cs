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
using System.Net.NetworkInformation;
using PuppeteerSharp;

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
        private readonly System.Timers.Timer MessageTimer;
        private static readonly string BaseUrl = $"https://www.youtube.com/live_chat?is_popout=1&v=";
        private readonly string JavaScript = string.Empty;
        private bool StreamLinked = false;
        private readonly IBrowser StreamBrowser;
        private readonly IPage StreamPage;
        #endregion

        #region ClassContext
        private void OnChatEvent(ChatEventArgs e) => Task.Run(() => ChatEvent?.Invoke(e));
        private void OnChatLoaded(ChatLoadedArgs e) => Task.Run(() => ChatLoaded?.Invoke(e));
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

            // Get the first available browser
            var (pathFound, (browserType, browserPath)) = BrowserLocator.GetFirstAvailableBrowser();

            // Browser launch options
            var launchOptions = new LaunchOptions
            {
                Headless = true,
                ExecutablePath = browserPath,
                Browser = browserType
            };

            // If no browser is found on the system, PuppeteerSharp will download the default (Chromium)
            this.StreamBrowser = Puppeteer.LaunchAsync(launchOptions).GetAwaiter().GetResult();
            this.StreamPage = this.StreamBrowser.PagesAsync().GetAwaiter().GetResult().First();
        }
        #endregion

        #region PageThread
        private void LoadStream()
        {
            Task.Run(LoadStreamAsync);
        }
        private async Task LoadStreamAsync()
        {
            string defaultChromeUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"; // Replace with your actual user agent
            await this.StreamPage.SetUserAgentAsync(defaultChromeUserAgent);

            await this.StreamPage.GoToAsync(BaseUrl + this.StreamId);

            string result = await this.StreamPage.EvaluateExpressionAsync<string>(this.JavaScript);

            // Check the result or handle any exceptions if needed
            // For example, if result contains some specific value or if there's an exception, handle it accordingly

            this.StreamLinked = true;
            this.OnChatLoaded(new ChatLoadedArgs());
        }
        private void ReadStream()
        {
            if (!this.MessageTimer.Enabled)
                this.MessageTimer.Start();
        }
        #endregion

        #region ChatControl
        public void StartReadingChat()
        {
            if (!this.StreamLinked)
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
            if (this.MessageTimer.Enabled)
                this.MessageTimer.Stop();
        }
        private async void GetMessages(object? sender, ElapsedEventArgs e)
        {
            try
            {
                string output = await this.StreamPage.EvaluateExpressionAsync<string>("window.GetMessageQueue();");
                Debug.WriteLine(output);
                if (output.Length > 2)
                {
                    foreach (JObject jobj in JArray.Parse(output).Cast<JObject>())
                    {
                        var obj = new ChatEventArgs(jobj);
                        this.OnChatEvent(obj);
                    }
                }
            }
            catch (PuppeteerException ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
        #endregion

        #region Dispose/Cleanup
        // To detect redundant calls
        private bool _disposedValue;
        // Instantiate a SafeHandle instance.
        private readonly SafeHandle _safeHandle = new SafeFileHandle(IntPtr.Zero, true);
        ~YoutubeChatReader()
        {
            this.Dispose(false);
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
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
