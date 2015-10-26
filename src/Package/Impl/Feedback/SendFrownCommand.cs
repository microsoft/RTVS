using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.R.Support.Utility;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Feedback {
    internal sealed class SendFrownCommand : SendMailCommand {
        private const string _rtvsGeneralDataFile = "RTVSGeneralData.log";
        private const string _rtvsSystemEventsFile = "RTVSSystemEvents.log";
        private const string _zipFile = "RTVSLogs.zip";
        private const int _daysToCollect = 7;

        private static LongAction[] _actions = {
            new LongAction() { Name = Resources.CollectingRTVSLogs, Action = CollectRTVSLogs },
            new LongAction() { Name = Resources.CollectingSystemEvents, Action = CollectSystemLogs },
            new LongAction() { Name = Resources.CollectingOSInformation, Action = CollectGeneralLogs },
            new LongAction() { Name = Resources.CreatingArchive, Action = CreateArchive },
        };

        private static List<string> _logFiles;

        public SendFrownCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSendFrown) {
        }

        protected override void SetStatus() {
            Enabled = true;
        }

        protected override void Handle() {
            string zipPath = Path.Combine(Path.GetTempPath(), _zipFile);
            _logFiles = new List<string>();

            try {
                File.Delete(zipPath);
            } catch (IOException) { }

            LongOperationNotification.ShowWaitingPopup(Resources.GatheringDiagnosticData, _actions);

            if (File.Exists(zipPath)) {
                SendMail("RTVS Frown", zipPath);
            }
        }

        private static void CollectRTVSLogs() {
            IEnumerable<string> logs;

            logs = GetRecentLogFiles("Microsoft.R.Host*.log");
            _logFiles.AddRange(logs);

            logs = GetRecentLogFiles("Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring*.log");
            _logFiles.AddRange(logs);

            string roamingFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string vsActivityLog = Path.Combine(roamingFolder, @"Microsoft\VisualStudio\14.0\ActivityLog.xml");
            if (File.Exists(vsActivityLog)) {
                _logFiles.Add(vsActivityLog);
            }
        }

        private static void CollectSystemLogs() {
            string systemEventsLog = CollectSystemEvents();
            _logFiles.Add(systemEventsLog);
        }

        private static void CollectGeneralLogs() {
            string generalDataLog = CollectGeneralData();
            _logFiles.Add(generalDataLog);
        }

        private static void CreateArchive() {
            ZipFiles(_logFiles);
        }

        private static string ZipFiles(IEnumerable<string> files) {
            string zipPath = Path.Combine(Path.GetTempPath(), _zipFile);

            using (FileStream zipStream = File.Create(zipPath)) {
                using (ZipArchive zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create)) {
                    foreach (string file in files) {
                        using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                            var entry = zipArchive.CreateEntry(Path.GetFileName(file));
                            using (var zipEntryStream = entry.Open()) {
                                fileStream.CopyTo(zipEntryStream);
                            }
                        }
                    }
                }
            }

            return zipPath;
        }

        private static IEnumerable<string> GetRecentLogFiles(string pattern) {
            string tempPath = Path.GetTempPath();

            var logs = Directory.EnumerateFiles(tempPath, pattern);
            return logs.Select((file) => {
                DateTime writeTime = File.GetLastWriteTimeUtc(file);
                TimeSpan difference = DateTime.Now.ToUniversalTime() - writeTime;
                if (difference.TotalDays < _daysToCollect) {
                    return file;
                }

                return null;
            });
        }

        private static string CollectSystemEvents() {
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

        private static string CollectGeneralData() {
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
                } catch (System.Exception ex) {
                    sw.WriteLine("  Failed to access system data.");
                    sw.WriteLine(ex.ToString());
                    sw.WriteLine();
                }
            }

            return generalDataFile;
        }
    }
}
