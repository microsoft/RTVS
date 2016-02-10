using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Repl {
    internal sealed class RInteractiveWindowProvider : IVsInteractiveWindowProvider {
        private IVsInteractiveWindow _vsInteractiveWindow;

        [Import]
        private IContentTypeRegistryService ContentTypeRegistryService { get; set; }

        [Import]
        private IVsInteractiveWindowFactory VsInteractiveWindowFactory { get; set; }

        [Import]
        private IRInteractiveProvider RInteractiveProvider { get; set; }

        public RInteractiveWindowProvider() {
            VsAppShell.Current.CompositionService.SatisfyImportsOnce(this);
        }

        public IVsInteractiveWindow Create(int instanceId) {
            var interactive = RInteractiveProvider.GetOrCreate();
            var evaluator = interactive.GetOrCreateEvaluator(instanceId);

            _vsInteractiveWindow = VsInteractiveWindowFactory.Create(RGuidList.ReplInteractiveWindowProviderGuid, instanceId, Resources.ReplWindowName, evaluator);
            _vsInteractiveWindow.SetLanguage(RGuidList.RLanguageServiceGuid, ContentTypeRegistryService.GetContentType(RContentTypeDefinition.ContentType));

            EventHandler<EventArgs> clearPendingInputsHandler = (sender, args) => ReplWindow.Current.ClearPendingInputs();
            interactive.RSession.Disconnected += clearPendingInputsHandler;
            var window = _vsInteractiveWindow.InteractiveWindow;

            EventHandler textViewOnClosed = null;
            textViewOnClosed = (_, __) => {
                window.TextView.Closed -= textViewOnClosed;
                interactive.RSession.Disconnected -= clearPendingInputsHandler;
                evaluator.Dispose();
            };
            window.TextView.Closed += textViewOnClosed;

            InitializeWindowAsync(window).DoNotWait();
            return _vsInteractiveWindow;
        }

        private static async Task InitializeWindowAsync(IInteractiveWindow window) {
            await window.InitializeAsync();
            VsAppShell.Current.DispatchOnUIThread(() => {
                IVsUIShell shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
                shell.UpdateCommandUI(0);
            });
        }

        public void Open(int instanceId, bool focus) {
            if (!ReplWindow.ReplWindowExists) {
                var window = Create(instanceId);
                window.Show(focus);
            } else {
                ReplWindow.ShowWindow();
            }
        }
    }
}
