using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.Common.Core.Enums;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings;
using Microsoft.R.Support.Settings.Definitions;
using Microsoft.VisualStudio.R.Package.Repl.Session;
using Microsoft.VisualStudio.R.Package.RPackages.Mirrors;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Options.R {
    [Export(typeof(IRToolsSettings))]
    internal sealed class RToolsSettingsImplementation : IRToolsSettings {
        private string _cranMirror;

        public string RVersion { get; set; } = Resources.Settings_RVersion_Latest;

        public YesNoAsk LoadRDataOnProjectLoad { get; set; } = YesNoAsk.No;

        public YesNoAsk SaveRDataOnProjectUnload { get; set; } = YesNoAsk.Ask;

        public bool EscInterruptsCalculation { get; set; } = true;

        public string CranMirror {
            get { return _cranMirror; }
            set {
                _cranMirror = value;
                // Setting mirror reques running code in R host
                // which async and cannot be done correctly here.
                IdleTimeAction.Create(async () => await SetMirrorToSession(), 20, typeof(RToolsSettingsImplementation));
            }
        }

        public RToolsSettingsImplementation() {
            _cranMirror = "0-Cloud [https]";
            RVersion = Resources.Settings_RVersion_Latest;
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
    }
}
