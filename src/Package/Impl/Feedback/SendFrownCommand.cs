using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Office.Interop.Outlook;
using Microsoft.R.Support.Utility;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Feedback {
    internal sealed class SendFrownCommand : SendMailCommand {
        private const string _rtvsGeneralDataFile = "RTVSGeneralData.log";
        private const string _rtvsSystemEventsFile = "RTVSSystemEvents.log";
        private const int _daysToCollect = 7;

        public SendFrownCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSendFrown) {
        }

        protected override void SetStatus() {
            Enabled = true;
        }

        protected override void Handle() {
            string zipName = CollectAndZipLogs();
            SendMail("RTVS Frown", zipName);
        }

        private string CollectAndZipLogs() {
            try {
                List<string> logFiles = new List<string>();

                IEnumerable<string> logs = GetRecentLogFiles("Microsoft.R.Host*.log");
                logFiles.AddRange(logs);

                logs = GetRecentLogFiles("Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring*.log");
                logFiles.AddRange(logs);

                string roamingFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string vsActivityLog = Path.Combine(roamingFolder, @"Microsoft\VisualStudio\14.0\ActivityLog.xml");
                if (File.Exists(vsActivityLog)) {
                    logFiles.Add(vsActivityLog);
                }

                string systemEventsLog = CollectSystemEvents();
                logFiles.Add(systemEventsLog);

                string generatDataLog = CollectGeneralData();
                logFiles.Add(generatDataLog);

                return ZipFiles(logFiles);
            } catch (System.Exception ex) {
                EditorShell.Current.ShowErrorMessage(
                    string.Format(CultureInfo.InvariantCulture, Resources.Error_CannotCollectLogs, ex.Message));
            }

            return string.Empty;
        }

        private string ZipFiles(IEnumerable<string> files) {
            string zipPath = Path.Combine(Path.GetTempPath(), "RTVSLogs.zip");

            using (FileStream fs = File.Create(zipPath)) {
                using (ZipArchive zipArchive = new ZipArchive(fs, ZipArchiveMode.Create)) {
                    foreach (string file in files) {
                        zipArchive.CreateEntryFromFile(file, Path.GetFileName(file));
                    }
                }
            }

            return zipPath;
        }

        private IEnumerable<string> GetRecentLogFiles(string pattern) {
            string tempPath = Path.GetTempPath();

            var logs = Directory.EnumerateFiles(tempPath, pattern);
            return logs.Select((file) =>
            {
                DateTime writeTime = File.GetLastWriteTimeUtc(file);
                TimeSpan difference = DateTime.Now.ToUniversalTime() - writeTime;
                if (difference.TotalDays < _daysToCollect) {
                    return file;
                }

                return null;
            });
        }

        private string CollectSystemEvents() {
            string systemEventsFile = Path.Combine(Path.GetTempPath(), _rtvsSystemEventsFile);
            using (var sw = new StreamWriter(systemEventsFile)) {
                try {
                    sw.WriteLine("System events:");

                    var application = new EventLog("Application");
                    var lastWeek = DateTime.Now.Subtract(TimeSpan.FromDays(7));
                    foreach (var entry in application.Entries.Cast<EventLogEntry>()
                        .Where(e => e.InstanceId == 1026L)  // .NET Runtime
                        .Where(e => e.TimeGenerated >= lastWeek)
                        .Where(e => InterestingApplicationLogEntries.IsMatch(e.Message))
                        .OrderByDescending(e => e.TimeGenerated)
                    ) {
                        sw.WriteLine(string.Format("Time: {0:s}", entry.TimeGenerated));
                        using (var reader = new StringReader(entry.Message.TrimEnd())) {
                            for (var line = reader.ReadLine(); line != null; line = reader.ReadLine()) {
                                sw.WriteLine(line);
                            }
                        }
                        sw.WriteLine();
                    }

                } catch (System.Exception ex) {
                    sw.WriteLine("  Failed to access event log.");
                    sw.WriteLine(ex.ToString());
                    sw.WriteLine();
                }
            }

            return systemEventsFile;
        }

        private static readonly Regex InterestingApplicationLogEntries = new Regex(
            @"^Application: (devenv\.exe|.+?Microsoft\.R\.Host\.exe)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
        );

        private string CollectGeneralData() {
            string generalDataFile = Path.Combine(Path.GetTempPath(), _rtvsGeneralDataFile);
            using (var sw = new StreamWriter(generalDataFile)) {
                try {
                    sw.WriteLine("Operating System Information");
                    sw.WriteLine("    Version:       " + Environment.OSVersion.ToString());
                    sw.WriteLine("    CPU Count:     " + Environment.ProcessorCount);
                    sw.WriteLine("    64 bit:        " + Environment.Is64BitOperatingSystem);
                    sw.WriteLine("    System Folder: " + Environment.SystemDirectory);
                    sw.WriteLine("    Working set:   " + Environment.WorkingSet);
                    sw.WriteLine();

                    Assembly thisAssembly = Assembly.GetExecutingAssembly();
                    sw.WriteLine("RTVS Information:");
                    sw.WriteLine("    Assembly: " + thisAssembly.FullName);
                    sw.WriteLine("    Codebase: " + thisAssembly.CodeBase);
                    sw.WriteLine();

                    IEnumerable<string> rEngines = RInstallation.GetInstalledEngineVersionsFromRegistry();
                    sw.WriteLine("Installed R Engines (from registry):");
                    foreach (string e in rEngines) {
                        sw.WriteLine("    " + e);
                    }
                    sw.WriteLine();

                    string latestEngine = RInstallation.GetLatestEnginePathFromRegistry();
                    sw.WriteLine("Latest R Engine (from registry):");
                    sw.WriteLine("    " + latestEngine);
                    sw.WriteLine();

                    string rInstallPath = RInstallation.GetRInstallPath();
                    sw.WriteLine("R Install path:");
                    sw.WriteLine("    " + rInstallPath);
                    sw.WriteLine();

                    sw.WriteLine("Loaded assemblies:");

                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().OrderBy(assem => assem.FullName)) {
                        var assemFileVersion = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false).OfType<AssemblyFileVersionAttribute>().FirstOrDefault();

                        sw.WriteLine(string.Format(CultureInfo.InvariantCulture, "    {0}, FileVersion={1}",
                            assembly.FullName,
                            assemFileVersion == null ? "(null)" : assemFileVersion.Version
                        ));
                    }
                }
                catch(System.Exception ex) {
                    sw.WriteLine("  Failed to access system data.");
                    sw.WriteLine(ex.ToString());
                    sw.WriteLine();
                }
            }

            return generalDataFile;
        }
    }
}
