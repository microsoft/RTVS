using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Common.Core.Telemetry;
using Microsoft.R.Actions.Utility;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Telemetry.Data;
using Microsoft.VisualStudio.R.Package.Telemetry.Definitions;
using Microsoft.VisualStudio.R.Package.Telemetry.Windows;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Telemetry {
    /// <summary>
    /// Represents telemetry operations in RTVS
    /// </summary>
    internal sealed class RtvsTelemetry : IRtvsTelemetry {
        private ToolWindowTracker _toolWindowTracker = new ToolWindowTracker();
        private ITelemetryService _telemetryService;

        public static IRtvsTelemetry Current { get; set; }

        internal class ConfigurationEvents {
            public const string RtvsVersion = "RTVS Version";
            public const string RInstallPath = "R Install Path";
            public const string REngine = "R Engine";
            public const string RROEngine = "RRO Engine";
            public const string MROEngine = "MRO Engine";
            public const string RBasePackages = "R Base Package";
            public const string RUserPackages = "R User Package";
        }

        internal class SettingEvents {
            public const string Settings = "Settings";
        }

        internal class WindowEvents {
            public const string ToolWindow = "Tool Window";
        }

        public static void Initialize(ITelemetryService service = null) {
            if (Current == null) {
                Current = new RtvsTelemetry(service);
            }
        }

        public RtvsTelemetry(ITelemetryService service = null) {
            _telemetryService = service ?? VsTelemetryService.Current;
        }

        public void ReportConfiguration() {
            if (_telemetryService.IsEnabled) {
                try {
                    Assembly thisAssembly = Assembly.GetExecutingAssembly();
                    _telemetryService.ReportEvent(TelemetryArea.Configuration, ConfigurationEvents.RtvsVersion, thisAssembly.GetName().Version.ToString());

                    string rInstallPath = RInstallation.GetRInstallPath(RToolsSettings.Current.RBasePath);
                    _telemetryService.ReportEvent(TelemetryArea.Configuration, ConfigurationEvents.RInstallPath, rInstallPath);

                    var rEngines = GetRSubfolders("R");
                    foreach (var s in rEngines) {
                        _telemetryService.ReportEvent(TelemetryArea.Configuration, ConfigurationEvents.REngine, s);
                    }

                    var rroEngines = GetRSubfolders("RRO");
                    foreach (var s in rroEngines) {
                        _telemetryService.ReportEvent(TelemetryArea.Configuration, ConfigurationEvents.RROEngine, s);
                    }

                    var mroEngines = GetRSubfolders("MRO");
                    foreach (var s in mroEngines) {
                        _telemetryService.ReportEvent(TelemetryArea.Configuration, ConfigurationEvents.MROEngine, s);
                    }

                    var hashes = RPackageData.GetInstalledPackageHashes(RPackageType.Base);
                    foreach (var s in hashes) {
                        _telemetryService.ReportEvent(TelemetryArea.Configuration, ConfigurationEvents.RBasePackages, s);
                    }

                    hashes = RPackageData.GetInstalledPackageHashes(RPackageType.User);
                    foreach (var s in hashes) {
                        _telemetryService.ReportEvent(TelemetryArea.Configuration, ConfigurationEvents.RUserPackages, s);
                    }
                } catch (Exception ex) {
                    Trace.Fail("Telemetry exception: " + ex.Message);
                }
            }
        }

        public void ReportSettings() {
            if (_telemetryService.IsEnabled) {
                try {
                    _telemetryService.ReportEvent(TelemetryArea.Configuration, SettingEvents.Settings,
                            new {
                                Cran = RToolsSettings.Current.CranMirror,
                                LoadRData = RToolsSettings.Current.LoadRDataOnProjectLoad,
                                SaveRData = RToolsSettings.Current.SaveRDataOnProjectUnload,
                                RCommandLineArguments = RToolsSettings.Current.RCommandLineArguments,
                                MultilineHistorySelection = RToolsSettings.Current.MultilineHistorySelection,
                                AlwaysSaveHistory = RToolsSettings.Current.AlwaysSaveHistory,
                                AutoFormat = REditorSettings.AutoFormat,
                                CommitOnEnter = REditorSettings.CommitOnEnter,
                                CommitOnSpace = REditorSettings.CommitOnSpace,
                                FormatOnPaste = REditorSettings.FormatOnPaste,
                                SendToReplOnCtrlEnter = REditorSettings.SendToReplOnCtrlEnter,
                                ShowCompletionOnFirstChar = REditorSettings.ShowCompletionOnFirstChar,
                                SignatureHelpEnabled = REditorSettings.SignatureHelpEnabled,
                                CompletionEnabled = REditorSettings.CompletionEnabled,
                                SyntaxCheckInRepl = REditorSettings.SyntaxCheckInRepl,
                                PartialArgumentNameMatch = REditorSettings.PartialArgumentNameMatch,
                            });
                } catch (Exception ex) {
                    Trace.Fail("Telemetry exception: " + ex.Message);
                }
            }
        }

        public void ReportWindowLayout(IVsUIShell shell) {
            if (_telemetryService.IsEnabled) {
                try {
                    var windows = ToolWindowData.GetToolWindowData(shell);
                    foreach (var w in windows) {
                        _telemetryService.ReportEvent(TelemetryArea.Configuration, WindowEvents.ToolWindow,
                                new { Caption = w.Caption, Left = w.X, Top = w.Y, Width = w.Width, Height = w.Height });
                    }
                } catch (Exception ex) {
                    Trace.Fail("Telemetry exception: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Retrieves all subfolders under R, RRO or MRO
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        private static IEnumerable<string> GetRSubfolders(string directory) {
            string root = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            string baseRFolder = Path.Combine(root + @"Program Files\", directory);
            try {
                return FolderUtility.GetSubfolderRelativePaths(baseRFolder);
            } catch (IOException) {
                // Don't do anything if there is no RRO installed
            }
            return Enumerable.Empty<string>();
        }

        public void Dispose() {
            _toolWindowTracker?.Dispose();
            _toolWindowTracker = null;

            var disp = _telemetryService as IDisposable;
            disp?.Dispose();
        }
    }
}
