using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace Microsoft.R.Actions.Utility
{
    public static class RUtility
    {
        public static string GetRBinariesFolder()
        {
            string binFolder = null;

            string installPath = GetRPathFromRegistry();

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

        public static string GetRPathFromRegistry()
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

                    if (string.IsNullOrEmpty(enginePath))
                    {
                        Version highest = null;
                        string highestVersionSubkeyName = null;

                        string[] subkeyNames = rKey.GetSubKeyNames();
                        foreach (string name in subkeyNames)
                        {
                            try
                            {
                                Version v = new Version(name);
                                if (highest != null)
                                {
                                    if (v > highest)
                                    {
                                        highest = v;
                                        highestVersionSubkeyName = name;
                                    }
                                }
                                else
                                {
                                    highest = v;
                                    highestVersionSubkeyName = name;
                                }
                            }
                            catch (Exception) { }
                        }

                        if (!string.IsNullOrEmpty(highestVersionSubkeyName))
                        {
                            RegistryKey subKey = rKey.OpenSubKey(highestVersionSubkeyName);
                            if (rKey != null)
                            {
                                enginePath = subKey.GetValue("InstallPath") as string;
                            }
                        }
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

            Debug.Assert(enginePath != null);
            return enginePath;
        }
    }
}
