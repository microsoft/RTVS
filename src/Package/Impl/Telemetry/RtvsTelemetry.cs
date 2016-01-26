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

        public static IRtvsTelemetry Current { get; set; }

        class ConfigurationEvents {
            public const string RtvsVersion = "RTVS Version";
            public const string RInstallPath = "R Install Path";
            public const string REngine = "R Engine";
            public const string RROEngine = "RRO Engine";
            public const string MROEngine = "MRO Engine";
            public const string RBasePackages = "R Base Package";
            public const string RUserPackages = "R User Package";
        }

        class SettingEvents {
            public const string Settings = "Settings";
        }

        class WindowEvents {
            public const string ToolWindow = "Tool Window";
        }

        public static void Initialize() {
            if (Current == null) {
                Current = new RtvsTelemetry();
            }
        }

        public void ReportConfiguration() {
            if (VsTelemetryService.Current.IsEnabled) {
                try {
                    Assembly thisAssembly = Assembly.GetExecutingAssembly();
                    VsTelemetryService.Current.ReportEvent(TelemetryArea.Configuration, ConfigurationEvents.RtvsVersion, thisAssembly.GetName().Version.ToString());

                    string rInstallPath = RInstallation.GetRInstallPath(RToolsSettings.Current.RBasePath);
                    VsTelemetryService.Current.ReportEvent(TelemetryArea.Configuration, ConfigurationEvents.RInstallPath, rInstallPath);

                    var rEngines = GetRSubfolders("R");
                    foreach (var s in rEngines) {
                        VsTelemetryService.Current.ReportEvent(TelemetryArea.Configuration, ConfigurationEvents.REngine, s);
                    }

                    var rroEngines = GetRSubfolders("RRO");
                    foreach (var s in rroEngines) {
                        VsTelemetryService.Current.ReportEvent(TelemetryArea.Configuration, ConfigurationEvents.RROEngine, s);
                    }

                    var mroEngines = GetRSubfolders("MRO");
                    foreach (var s in mroEngines)
                    {
                        VsTelemetryService.Current.ReportEvent(TelemetryArea.Configuration, ConfigurationEvents.MROEngine, s);
                    }

                    var hashes = RPackageData.GetInstalledPackageHashes(RPackageType.Base);
                    foreach (var s in hashes) {
                        VsTelemetryService.Current.ReportEvent(TelemetryArea.Configuration, ConfigurationEvents.RBasePackages, s);
                    }

                    hashes = RPackageData.GetInstalledPackageHashes(RPackageType.User);
                    foreach (var s in hashes) {
                        VsTelemetryService.Current.ReportEvent(TelemetryArea.Configuration, ConfigurationEvents.RUserPackages, s);
                    }
                } catch (Exception ex) {
                    Trace.Fail("Telemetry exception: " + ex.Message);
                }
            }
        }

        public void ReportSettings() {
            if (VsTelemetryService.Current.IsEnabled) {
                try {
                    VsTelemetryService.Current.ReportEvent(TelemetryArea.Configuration, SettingEvents.Settings,
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
            if (VsTelemetryService.Current.IsEnabled) {
                try {
                    var windows = ToolWindowData.GetToolWindowData(shell);
                    foreach (var w in windows) {
                        VsTelemetryService.Current.ReportEvent(TelemetryArea.Configuration, WindowEvents.ToolWindow,
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
            VsTelemetryService.Current.Dispose();
        }
    }
}
