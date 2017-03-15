// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Telemetry;
using Microsoft.R.Components.Settings;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Interpreters;
using Microsoft.R.Support.Help;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Telemetry.Data;
using Microsoft.VisualStudio.R.Package.Telemetry.Definitions;
using Microsoft.VisualStudio.R.Package.Telemetry.Windows;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Telemetry;

namespace Microsoft.VisualStudio.R.Package.Telemetry {
    /// <summary>
    /// Represents telemetry operations in RTVS
    /// </summary>
    internal sealed class RtvsTelemetry : IRtvsTelemetry {
        private ToolWindowTracker _toolWindowTracker = new ToolWindowTracker();
        private readonly IPackageIndex _packageIndex;
        private static IRSettings _settings;

        public static IRtvsTelemetry Current { get; set; }

        internal class ConfigurationEvents {
            public const string RtvsVersion = "RTVS Version";
            public const string RInstallPath = "R Install Path";
            public const string REngine = "R Engine";
            public const string RROEngine = "RRO Engine";
            public const string MROEngine = "MRO Engine";
            public const string RPackages = "R Package";
            public const string RClientFound = "MRC Found";
            public const string RClientInstallYes = "MRC Install Yes";
            public const string RClientInstallCancel = "MRC Install Canceled";
            public const string RClientActive = "MRC Active";
            public const string RClientDownloadFailed = "MRC Download Failed";
            public const string LocalConnection = "Local Connection";
            public const string RemoteConnection = "Remote Connection";
        }

        internal class SettingEvents {
            public const string Settings = "Settings";
        }

        internal class WindowEvents {
            public const string ToolWindow = "Tool Window";
        }

        public static void Initialize(IPackageIndex packageIndex, IRSettings settings, ITelemetryService service = null) {
            if (Current == null) {
                Current = new RtvsTelemetry(packageIndex, settings, service);
            }
        }

        public RtvsTelemetry(IPackageIndex packageIndex, IRSettings settings, ITelemetryService service = null) {
            _packageIndex = packageIndex;
            _settings = settings;
            TelemetryService = service ?? Vsshell.Current.Services.GetService<ITelemetryService>();
        }

        public ITelemetryService TelemetryService { get; }

        public void ReportConfiguration() {
            if (TelemetryService.IsEnabled) {
                try {
                    Assembly thisAssembly = Assembly.GetExecutingAssembly();
                    TelemetryService.ReportEvent(TelemetryArea.Configuration, ConfigurationEvents.RtvsVersion, thisAssembly.GetName().Version.ToString());

                    ReportLocalRConfiguration();
                    ReportConnectionsConfiguration();

                } catch (Exception ex) {
                    Trace.Fail("Telemetry exception: " + ex.Message);
                }
            }
        }

        private void ReportLocalRConfiguration() {
            // Report local R installations
            var engines = new RInstallation().GetCompatibleEngines();
            foreach (var e in engines) {
                TelemetryService.ReportEvent(TelemetryArea.Configuration, ConfigurationEvents.RInstallPath, e.InstallPath);
            }

            string rClientPath = SqlRClientInstallation.GetRClientPath();
            if (rClientPath != null) {
                TelemetryService.ReportEvent(TelemetryArea.Configuration, ConfigurationEvents.RClientFound);
            }

            var rEngines = GetRSubfolders("R");
            foreach (var s in rEngines) {
                TelemetryService.ReportEvent(TelemetryArea.Configuration, ConfigurationEvents.REngine, s);
            }

            var rroEngines = GetRSubfolders("RRO");
            foreach (var s in rroEngines) {
                TelemetryService.ReportEvent(TelemetryArea.Configuration, ConfigurationEvents.RROEngine, s);
            }

            var mroEngines = GetRSubfolders("MRO");
            foreach (var s in mroEngines) {
                TelemetryService.ReportEvent(TelemetryArea.Configuration, ConfigurationEvents.MROEngine, s);
            }

            if (_packageIndex != null) {
                foreach (var p in _packageIndex.Packages) {
                    TelemetryService.ReportEvent(TelemetryArea.Configuration, ConfigurationEvents.RPackages, new TelemetryPiiProperty(p.Name));
                }
            }
        }

        private void ReportConnectionsConfiguration() {
            var connections = _settings.Connections;
            if (connections != null) {
                foreach (var c in connections) {
                    Uri uri;
                    if (Uri.TryCreate(c.Path, UriKind.Absolute, out uri)) {
                        if (uri.IsFile) {
                            TelemetryService.ReportEvent(TelemetryArea.Configuration, ConfigurationEvents.LocalConnection, uri.ToString());
                        } else {
                            TelemetryService.ReportEvent(TelemetryArea.Configuration, ConfigurationEvents.RemoteConnection, new TelemetryPiiProperty(uri.ToString()));
                        }
                    }
                }
            }
        }

        public void ReportSettings() {
            if (TelemetryService.IsEnabled) {
                try {
                    TelemetryService.ReportEvent(TelemetryArea.Configuration, SettingEvents.Settings,
                            new {
                                Cran = RToolsSettings.Current.CranMirror,
                                Locale = RToolsSettings.Current.RCodePage,
                                LoadRData = RToolsSettings.Current.LoadRDataOnProjectLoad,
                                SaveRData = RToolsSettings.Current.SaveRDataOnProjectUnload,
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
                                RCommandLineArguments = RToolsSettings.Current.LastActiveConnection.RCommandLineArguments
                            });
                } catch (Exception ex) {
                    Trace.Fail("Telemetry exception: " + ex.Message);
                }
            }
        }

        public void ReportWindowLayout(IVsUIShell shell) {
            if (TelemetryService.IsEnabled) {
                try {
                    var windows = ToolWindowData.GetToolWindowData(shell);
                    foreach (var w in windows) {
                        TelemetryService.ReportEvent(TelemetryArea.Configuration, WindowEvents.ToolWindow,
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

            var disp = TelemetryService as IDisposable;
            disp?.Dispose();
        }
    }
}
