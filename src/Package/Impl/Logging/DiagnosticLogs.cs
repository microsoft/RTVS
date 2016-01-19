using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.R.Actions.Utility;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;

namespace Microsoft.VisualStudio.R.Package.Logging {
    internal static class DiagnosticLogs {
        public const int DaysToRetain = 5;
        public const string RHostLogPattern = "Microsoft.R.Host*.log";
        public const string ProjectSystemLogPattern = "Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring*.log";
        public const string RtvsGeneralDataFile = "RTVSGeneralData.log";
        public const string RtvsSystemEventsFile = "RTVSSystemEvents.log";
        public const string RtvsLogZipFile = "RTVSLogs.zip";

        public static IEnumerable<string> RtvsLogFilePatterns {
            get {
                return new string[] {
                    RHostLogPattern,
                    ProjectSystemLogPattern,
                    RtvsGeneralDataFile,
                    RtvsSystemEventsFile,
                    RtvsLogZipFile
                };
            }
        }

        private static LongAction[] _actions = {
            new LongAction() { Name = Resources.CollectingRTVSLogs, Action = CollectRTVSLogs },
            new LongAction() { Name = Resources.CollectingSystemEvents, Action = CollectSystemLogs },
            new LongAction() { Name = Resources.CollectingOSInformation, Action = CollectGeneralLogs },
            new LongAction() { Name = Resources.CreatingArchive, Action = CreateArchive },
        };

        private static List<string> _logFiles = new List<string>();

        public static string Collect() {
            string zipPath = string.Empty;
            _logFiles.Clear();

            try {
                zipPath = Path.Combine(Path.GetTempPath(), RtvsLogZipFile);
                var rSessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
                var sessions = rSessionProvider.GetSessions();
                foreach (var s in sessions) {
                    s.FlushLog();
                }

                if (File.Exists(zipPath)) {
                    File.Delete(zipPath);
                }
            } catch (IOException) {
            } catch (UnauthorizedAccessException) {
            } catch (ArgumentException) {
            }

            LongOperationNotification.ShowWaitingPopup(Resources.GatheringDiagnosticData, _actions);
            return zipPath;
        }

        private static void CollectRTVSLogs() {
            IEnumerable<string> logs;

            logs = GetRecentLogFiles(RHostLogPattern);
            _logFiles.AddRange(logs);

            logs = GetRecentLogFiles(ProjectSystemLogPattern);
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
            string zipPath = Path.Combine(Path.GetTempPath(), RtvsLogZipFile);

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

            _logFiles.Clear();
            return zipPath;
        }

        private static IEnumerable<string> GetRecentLogFiles(string pattern) {
            string tempPath = Path.GetTempPath();

            var logs = Directory.EnumerateFiles(tempPath, pattern);
            return logs.Where((file) => {
                DateTime writeTime = File.GetLastWriteTimeUtc(file);
                TimeSpan difference = DateTime.Now.ToUniversalTime() - writeTime;
                if (difference.TotalDays < DaysToRetain) {
                    return true;
                }

                return false;
            });
        }

        private static string CollectSystemEvents() {
            string systemEventsFile = Path.Combine(Path.GetTempPath(), RtvsSystemEventsFile);
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
            string generalDataFile = Path.Combine(Path.GetTempPath(), RtvsGeneralDataFile);
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

                    string rInstallPath = RInstallation.GetRInstallPath(RToolsSettings.Current.RBasePath);
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
