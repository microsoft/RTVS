using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core.Enums;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.R.Actions.Utility;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings;
using Microsoft.R.Support.Settings.Definitions;
using Microsoft.VisualStudio.R.Package.Repl.Session;
using Microsoft.VisualStudio.R.Package.RPackages.Mirrors;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Options.R {
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
                _workingDirectory = value;
                UpdateWorkingDirectoryList(_workingDirectory);

                if (EditorShell.HasShell) {
                    EditorShell.DispatchOnUIThread(() => {
                        IVsUIShell shell = AppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
                        shell.UpdateCommandUI(1);
                    });
                }
            }
        }

        public string[] WorkingDirectoryList { get; set; } = new string[0];

        public string RCommandLineArguments { get; set; }

        public RToolsSettingsImplementation() {
            // Default settings. Will be overwritten with actual
            // settings (if any) when settings are loaded from storage
            _cranMirror = "0-Cloud [https]";
            RBasePath = RInstallation.GetLatestEnginePathFromRegistry();
            WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private async Task SetMirrorToSession() {
            IRSessionProvider sessionProvider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            var sessions = sessionProvider.GetSessions();

            foreach (var s in sessions) {
                using (IRSessionEvaluation eval = await s.Value.BeginEvaluationAsync()) {
                    string mirrorName = RToolsSettings.Current.CranMirror;
                    string mirrorUrl = CranMirrorList.UrlFromName(mirrorName);
                    await eval.SetVsCranSelection(mirrorUrl);
                }
            }
        }

        public void LoadFromStorage() {
            // This causes IDE to load settings from storage.
            // Page retrieval from package sets site which yields
            // settings loaded. Just calling LoadSettingsFromStorage()
            // does not work.
            using (var p = RPackage.Current.GetDialogPage(typeof(RToolsOptionsPage))) { }
        }

        private void UpdateWorkingDirectoryList(string newDirectory) {
            List<string> list = new List<string>(WorkingDirectoryList);
            if (!list.Contains(newDirectory)) {
                list.Insert(0, newDirectory);
                if (list.Count > MaxDirectoryEntries) {
                    list.RemoveAt(list.Count - 1);
                }
                WorkingDirectoryList = list.ToArray();
            }
        }
    }
}
