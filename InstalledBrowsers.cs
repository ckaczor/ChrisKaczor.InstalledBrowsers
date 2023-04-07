using Microsoft.Win32;
using System.Diagnostics;

namespace ChrisKaczor.InstalledBrowsers
{
    public class InstalledBrowser
    {
        public string Key { get; private init; } = string.Empty;
        public string Name { get; private init; } = string.Empty;
        public string Command { get; private init; } = string.Empty;
        public bool IsWindowsDefault { get; private init; }

        public void OpenLink(string url)
        {
            if (string.IsNullOrEmpty(url))
                return;

            // Add quotes around the URL for safety
            url = $"\"{url}\"";

            // Start the browser
            if (IsWindowsDefault)
            {
                var ps = new ProcessStartInfo(url)
                {
                    UseShellExecute = true,
                    Verb = "open"
                };

                Process.Start(ps);
            }
            else
                Process.Start(Command, url);
        }

        public static Dictionary<string, InstalledBrowser> GetInstalledBrowsers(bool includeSystemDefault)
        {
            var browsers = new Dictionary<string, InstalledBrowser>();

            if (includeSystemDefault)
            {
                var browser = new InstalledBrowser
                {
                    Key = string.Empty,
                    Name = "< Windows Default >",
                    IsWindowsDefault = true
                };

                browsers.Add(browser.Key, browser);
            }

            var browserKeys = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Clients\StartMenuInternet") ?? Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Clients\StartMenuInternet");

            if (browserKeys == null)
                return browsers;

            var browserNames = browserKeys.GetSubKeyNames();

            foreach (var browserName in browserNames)
            {
                var browserKey = browserKeys.OpenSubKey(browserName);

                var commandPath = browserKey?.OpenSubKey(@"shell\open\command");

                if (commandPath == null)
                    continue;

                var browser = new InstalledBrowser
                {
                    Key = browserName,
                    Name = (string) browserKey!.GetValue(null, string.Empty),
                    Command = (string) commandPath.GetValue(null, string.Empty)
                };

                if (browser.Name.Length == 0 || browser.Command.Length == 0)
                    continue;

                browsers.Add(browserName, browser);
            }

            return browsers;
        }

        public static InstalledBrowser? FindBrowser(string browserKey)
        {
            var browsers = GetInstalledBrowsers(true);

            browsers.TryGetValue(browserKey, out var installedBrowser);

            return installedBrowser;
        }

        public static bool OpenLink(string browserKey, string url)
        {
            try
            {
                var installedBrowser = FindBrowser(browserKey);

                installedBrowser?.OpenLink(url);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}