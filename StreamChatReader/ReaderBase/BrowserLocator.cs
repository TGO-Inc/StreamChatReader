using Microsoft.Win32;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamingServices
{
    internal class BrowserLocator
    {
        public static (SupportedBrowser, string?) GetChromePath() =>
            (SupportedBrowser.Chrome, GetBrowserPath(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe"));

        public static (SupportedBrowser, string?) GetEdgePath() =>
            (SupportedBrowser.Chromium, GetBrowserPath(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\msedge.exe"));

        public static (SupportedBrowser, string?) GetFirefoxPath() =>
            (SupportedBrowser.Firefox, GetBrowserPath(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\firefox.exe"));


        private static string? GetBrowserPath(string keyPath)
        {
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(keyPath);
            return key?.GetValue("") as string;
        }

        public static List<(SupportedBrowser, string)?> GetAvailableBrowsers()
        {
            List<(SupportedBrowser, string)?> availableBrowsers = new();

            AddIfNotNull(availableBrowsers, GetChromePath());
            AddIfNotNull(availableBrowsers, GetFirefoxPath());
            AddIfNotNull(availableBrowsers, GetEdgePath());

            return availableBrowsers;

            static void AddIfNotNull(List<(SupportedBrowser, string)?> list, (SupportedBrowser, string? path) item)
            {
                if (!string.IsNullOrEmpty(item.path))
                    list.Add(item!);
            }
        }

        public static (bool, (SupportedBrowser, string)) GetFirstAvailableBrowser()
        {
            var browser = GetAvailableBrowsers().FirstOrDefault();
            return (browser.HasValue, browser ?? (SupportedBrowser.Chromium, string.Empty));
        }
    }
}
