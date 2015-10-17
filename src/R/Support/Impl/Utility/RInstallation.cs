using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Support.Settings;
using Microsoft.Win32;

namespace Microsoft.R.Support.Utility
{
    public static class RInstallation
    {
        public static bool VerifyRIsInstalled()
        {
            string rPath = RInstallation.GetRInstallPath();
            if (!string.IsNullOrEmpty(rPath))
            {
                bool rExeExists = File.Exists(Path.Combine(rPath, @"bin\R.exe"));
                bool rTermExists = File.Exists(Path.Combine(rPath, @"bin\i386\RTerm.exe")) || File.Exists(Path.Combine(rPath, @"bin\x64\RTerm.exe"));

                if (rExeExists && rTermExists)
                {
                    return true;
                }
            }
            else
            {
                rPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "R");
            }

            string message = string.Format(CultureInfo.InvariantCulture, Resources.Error_RNotInstalled, rPath);
            EditorShell.Current.ShowErrorMessage(message);

            Process.Start("https://cran.r-project.org");

            return false;
        }

        public static void GoToRInstallPage()
        {
            Process.Start("http://google.com");
        }

        /// <summary>
        /// Retrieves path to the installed R engine root folder
        /// </summary>
        public static string GetRInstallPath()
        {
            string rVersion = RToolsSettings.Current.RVersion;
            string installPath = null;

            if (rVersion[0] != '[') // [Latest] (localized)
            {
                // First try user-specified options
                installPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"\R\", rVersion);
            }

            if (string.IsNullOrEmpty(installPath) || !Directory.Exists(installPath))
            {
                installPath = RInstallation.GetLatestEnginePathFromRegistry();
            }

            return installPath;
        }

        /// <summary>
        /// Retrieves path to the installed R engine binaries folder
        /// </summary>
        public static string GetBinariesFolder()
        {
            string binFolder = null;
            string installPath = RInstallation.GetRInstallPath();

            if (!string.IsNullOrEmpty(installPath))
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    // Try x64 R engine
                    binFolder = Path.Combine(installPath, @"bin\x64");
                }

                if (!Directory.Exists(binFolder))
                {
                    binFolder = Path.Combine(installPath, @"bin\i386");
                    if (!Directory.Exists(binFolder))
                    {
                        binFolder = null;
                    }
                }
            }

            return binFolder;
        }

        public static string GetLatestEnginePathFromRegistry()
        {
            string[] installedEngines = GetInstalledEnginesFromRegistry();
            string highestVersionName = string.Empty;
            Version highest = null;

            foreach (string name in installedEngines)
            {
                Version v = new Version(name);
                if (highest != null)
                {
                    if (v > highest)
                    {
                        highest = v;
                        highestVersionName = name;
                    }
                }
                else
                {
                    highest = v;
                    highestVersionName = name;
                }
            }

            return highestVersionName;
        }

        public static string[] GetInstalledEnginesFromRegistry()
        {
            List<string> enginePaths = new List<string>();

            // HKEY_LOCAL_MACHINE\SOFTWARE\R-core
            // HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\R-core
            using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default))
            {
                RegistryKey rKey = null;

                try
                {
                    rKey = hklm.OpenSubKey(@"SOFTWARE\R-core\R");
                    if (rKey == null)
                    {
                        // Possibly 64-bit machine with only 32-bit R installed
                        rKey = hklm.OpenSubKey(@"SOFTWARE\Wow6432Node\R-core\R");
                    }

                    if (rKey == null)
                    {
                        // Possibly 64-bit machine with only 32-bit R installed
                        rKey = hklm.OpenSubKey(@"SOFTWARE\Wow6432Node\R-core\R64");
                    }

                    if (rKey != null)
                    {
                        return rKey.GetSubKeyNames();
                    }
                }
                catch (Exception)
                {
                    return null;
                }
                finally
                {
                    if (rKey != null)
                    {
                        rKey.Dispose();
                    }
                }
            }

            return new string[0];
        }
    }
}
