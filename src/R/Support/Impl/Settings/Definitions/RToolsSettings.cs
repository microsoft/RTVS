using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Support.Settings.Definitions;
using Microsoft.Win32;

namespace Microsoft.R.Support.Settings
{
    public static class RToolsSettings
    {
        // Exported by the host package
        internal static IRToolsSettings ToolsSettings { get; set; }

        /// <summary>
        /// Retrieves path to the installed R engine root folder
        /// </summary>
        public static string GetRVersionPath()
        {
            string installPath = null;

            Init();

            // First try user-specified options
            installPath = ToolsSettings.GetRVersionPath();
            if (string.IsNullOrEmpty(installPath))
            {
                installPath = RToolsSettings.GetEnginePathFromRegistry();
            }

            return installPath;
        }

        /// <summary>
        /// Retrieves path to the installed R engine binaries folder
        /// </summary>
        public static string GetBinariesFolder()
        {
            string binFolder = null;

            Init();

            string installPath = RToolsSettings.GetRVersionPath();

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

        private static void Init()
        {
            if (ToolsSettings == null)
            {
                ToolsSettings = EditorShell.ExportProvider.GetExport<IRToolsSettings>().Value;
                Debug.Assert(ToolsSettings != null);
            }
        }

        public static string GetEnginePathFromRegistry()
        {
            string enginePath = null;

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

                    if (rKey != null)
                    {
                        enginePath = rKey.GetValue("InstallPath") as string;
                    }
                }
                finally
                {
                    if (rKey != null)
                    {
                        rKey.Dispose();
                    }
                }
            }

            return enginePath;
        }
    }
}
