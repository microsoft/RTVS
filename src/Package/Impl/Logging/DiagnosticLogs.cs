// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Interpreters;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Logging {
    internal static class DiagnosticLogs {
        public const int DaysToRetain = 5;
        public const int MaximumFileSize = 1024 * 1024;
        public const string GeneralLogPattern = "Microsoft.R.General*.log";
        public const string RHostLogPattern = "Microsoft.R.Host*.log";
        public const string ProjectSystemLogPattern = "Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring*.log";
        public const string RtvsGeneralDataFile = "RTVSGeneralData.log";
        public const string RtvsSystemEventsFile = "RTVSSystemEvents.log";
        public const string RtvsLogZipFile = "RTVSLogs.zip";

        public static IEnumerable<string> RtvsLogFilePatterns => new [] {
            RHostLogPattern,
            ProjectSystemLogPattern,
            RtvsGeneralDataFile,
            RtvsSystemEventsFile,
            RtvsLogZipFile
        };

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
                var workflowProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRInteractiveWorkflowProvider>();
                var rSessionProvider = workflowProvider.GetOrCreate().RSessions;
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

        private static void CollectRTVSLogs(object o, CancellationToken ct) {
            IEnumerable<string> logs;

            logs = GetRecentLogFiles(GeneralLogPattern);
            _logFiles.AddRange(logs);

            logs = GetRecentLogFiles(RHostLogPattern);
            _logFiles.AddRange(logs);

            logs = GetRecentLogFiles(ProjectSystemLogPattern);
            _logFiles.AddRange(logs);

            string roamingFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string vsActivityLog = Path.Combine(roamingFolder, Invariant($"Microsoft\\VisualStudio\\{Toolset.Version}\\ActivityLog.xml"));
            if (File.Exists(vsActivityLog)) {
                _logFiles.Add(vsActivityLog);
            }
        }

        private static void CollectSystemLogs(object o, CancellationToken ct) {
            string systemEventsLog = CollectSystemEvents();
            _logFiles.Add(systemEventsLog);
        }

        private static void CollectGeneralLogs(object o, CancellationToken ct) {
            string generalDataLog = CollectGeneralData();
            _logFiles.Add(generalDataLog);
        }

        private static void CreateArchive(object o, CancellationToken ct) {
            ZipFiles(_logFiles);
        }

        private static void ZipFiles(IEnumerable<string> files) {
            string zipPath = Path.Combine(Path.GetTempPath(), RtvsLogZipFile);

            using (FileStream zipStream = File.Create(zipPath)) {
                using (ZipArchive zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create)) {
                    foreach (string file in files) {
                        using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                            if (fileStream.Length > MaximumFileSize) {
                                fileStream.Seek(-MaximumFileSize, SeekOrigin.End);
                            }
                            var entry = zipArchive.CreateEntry(Path.GetFileName(file));
                            using (var zipEntryStream = entry.Open()) {
                                fileStream.CopyTo(zipEntryStream);
                            }
                        }
                    }
                }
            }

            _logFiles.Clear();
        }

        private static IEnumerable<string> GetRecentLogFiles(string pattern) {
            string tempPath = Path.GetTempPath();

            var logs = Directory.EnumerateFiles(tempPath, pattern);
            return logs.Where(file => {
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
                        sw.WriteLine(string.Format(CultureInfo.InvariantCulture, "Time: {0:s}", entry.TimeGenerated));
                        using (var reader = new StringReader(entry.Message.TrimEnd())) {
                            for (var line = reader.ReadLine(); line != null; line = reader.ReadLine()) {
                                sw.WriteLine(line);
                            }
                        }
                        sw.WriteLine();
                    }

                } catch (Exception ex) {
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

        public static void WriteGeneralData(TextWriter writer, bool detailed) {
            try {
                writer.WriteLine("OS Information");
                writer.WriteLine("    Version:       " + Environment.OSVersion);
                if (detailed) {
                    writer.WriteLine("    CPU Count:     " + Environment.ProcessorCount);
                    writer.WriteLine("    64 bit:        " + Environment.Is64BitOperatingSystem);
                    writer.WriteLine("    System Folder: " + Environment.SystemDirectory);
                    writer.WriteLine("    Working set:   " + Environment.WorkingSet);
                }
                writer.WriteLine();

                Assembly thisAssembly = Assembly.GetExecutingAssembly();
                writer.WriteLine("RTVS Information:");
                writer.WriteLine("    Assembly: " + thisAssembly.FullName);
                if (detailed) {
                    writer.WriteLine("    Codebase: " + thisAssembly.CodeBase);
                }
                writer.WriteLine();

                var ri = new RInstallation();
                var workflow = VsAppShell.Current.ExportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate();
                if (detailed) {
                    IEnumerable<string> rEngines = ri.GetInstalledEngineVersionsFromRegistry();
                    writer.WriteLine("Installed R Engines (from registry):");
                    foreach (string e in rEngines) {
                        writer.WriteLine("    " + e);
                    }
                    writer.WriteLine();

                    string latestEngine = ri.GetCompatibleEnginePathFromRegistry();
                    writer.WriteLine("Latest R Engine (from registry):");
                    writer.WriteLine("    " + latestEngine);
                    writer.WriteLine();

                    var connections = workflow.Connections.RecentConnections;
                    writer.WriteLine("Installed R Engines (from registry):");
                    foreach (var connection in connections) {
                        writer.WriteLine($"    {connection.Name}: {connection.Id}");
                    }
                    writer.WriteLine();
                }
                
                var activeConnection = workflow.Connections.ActiveConnection;
                if (activeConnection != null) {
                    writer.WriteLine("Active R URI:");
                    writer.WriteLine($"    {activeConnection.Name}: {activeConnection.Id}");
                    writer.WriteLine();
                }

                if (detailed) {
                    writer.WriteLine("Assemblies loaded by Visual Studio:");

                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().OrderBy(assem => assem.FullName)) {
                        var assemFileVersion = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false).OfType<AssemblyFileVersionAttribute>().FirstOrDefault();

                        writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "    {0}, FileVersion={1}",
                            assembly.FullName,
                            assemFileVersion == null ? "(null)" : assemFileVersion.Version
                        ));
                    }
                }
            } catch (Exception ex) {
                writer.WriteLine("  Failed to access system data.");
                writer.WriteLine(ex.ToString());
                writer.WriteLine();
            } finally {
                writer.Flush();
            }
        }

        private static string CollectGeneralData() {
            string generalDataFile = Path.Combine(Path.GetTempPath(), RtvsGeneralDataFile);
            using (var sw = new StreamWriter(generalDataFile)) {
                WriteGeneralData(sw, detailed: true);
            }
            return generalDataFile;
        }
    }
}
