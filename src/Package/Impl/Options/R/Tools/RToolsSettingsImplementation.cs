using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core.Enums;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.R.Actions.Utility;
using Microsoft.R.Components.Settings;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Support.Settings;
using Microsoft.R.Support.Settings.Definitions;
using Microsoft.VisualStudio.R.Package.RPackages.Mirrors;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Options.R {
    [Export(typeof(IRSettings))]
    [Export(typeof(IRToolsSettings))]
    internal sealed class RToolsSettingsImplementation : IRToolsSettings {
        private const int MaxDirectoryEntries = 8;
        private string _cranMirror;
        private string _workingDirectory;

        /// <summary>
        /// Path to 64-bit R installation such as 
        /// 'C:\Program Files\R\R-3.2.2' without bin\x64
        /// </summary>
        public string RBasePath { get; set; }

        public YesNoAsk LoadRDataOnProjectLoad { get; set; } = YesNoAsk.No;

        public YesNoAsk SaveRDataOnProjectUnload { get; set; } = YesNoAsk.Ask;

        public bool AlwaysSaveHistory { get; set; } = true;

        public bool ClearFilterOnAddHistory { get; set; } = true;

        public bool MultilineHistorySelection { get; set; } = true;

        public string CranMirror {
            get { return _cranMirror; }
            set {
                _cranMirror = value;
                // Setting mirror reques running code in R host
                // which async and cannot be done correctly here.
                IdleTimeAction.Create(async () => await SetMirrorToSession(), 20, typeof(RToolsSettingsImplementation));
            }
        }

        public string WorkingDirectory {
            get { return _workingDirectory; }
            set {
                var newDirectory = value;
                // Trim trailing slash except if it is root
                if (newDirectory.EndsWith("\\") && !(newDirectory.Length > 1 && newDirectory[newDirectory.Length-2] == ':')) {
                    newDirectory = newDirectory.Substring(0, newDirectory.Length - 1);
                }

                _workingDirectory = newDirectory;
                UpdateWorkingDirectoryList(newDirectory);

                if (EditorShell.HasShell) {
                    EditorShell.DispatchOnUIThread(() => {
                        IVsUIShell shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
                        shell.UpdateCommandUI(1);
                    });
                }
            }
        }

        public string[] WorkingDirectoryList { get; set; } = new string[0];

        public string RCommandLineArguments { get; set; }

        public HelpBrowserType HelpBrowser { get; set; }

        public bool ShowDotPrefixedVariables { get; set; }

        public RToolsSettingsImplementation() {
            // Default settings. Will be overwritten with actual
            // settings (if any) when settings are loaded from storage
            _cranMirror = "0-Cloud [https]";
            RBasePath = RInstallation.GetCompatibleEnginePathFromRegistry();
            WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private async Task SetMirrorToSession() {
            IRSessionProvider sessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            var sessions = sessionProvider.GetSessions();

            foreach (var s in sessions.Where(s => s.IsHostRunning)) {
                try {
                    using (IRSessionEvaluation eval = await s.BeginEvaluationAsync()) {
                        string mirrorName = RToolsSettings.Current.CranMirror;
                        string mirrorUrl = CranMirrorList.UrlFromName(mirrorName);
                        await eval.SetVsCranSelection(mirrorUrl);
                    }
                } catch(OperationCanceledException) { }
            }
        }

        private void UpdateWorkingDirectoryList(string newDirectory) {
            List<string> list = new List<string>(WorkingDirectoryList);
            if (!list.Contains(newDirectory, StringComparer.OrdinalIgnoreCase)) {
                list.Insert(0, newDirectory);
                if (list.Count > MaxDirectoryEntries) {
                    list.RemoveAt(list.Count - 1);
                }

                WorkingDirectoryList = list.ToArray();
            }
        }
    }
}
