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
        [Import]
        private IContentTypeRegistryService ContentTypeRegistryService { get; set; }

        [Import]
        private IVsInteractiveWindowFactory VsInteractiveWindowFactory { get; set; }

        [Import]
        private IRInteractiveSessionProvider RInteractiveSessionProvider { get; set; }

        public RInteractiveWindowProvider() {
            VsAppShell.Current.CompositionService.SatisfyImportsOnce(this);
        }

        public IVsInteractiveWindow Create(int instanceId) {
            var interactive = RInteractiveSessionProvider.GetOrCreate();
            var evaluator = interactive.GetOrCreateEvaluator(instanceId);

            EventHandler<EventArgs> clearPendingInputsHandler = (sender, args) => interactive.ClearPendingInputs();
            interactive.RSession.Disconnected += clearPendingInputsHandler;
            EventHandler textViewOnClosed = (_, __) => {
                interactive.RSession.Disconnected -= clearPendingInputsHandler;
                evaluator.Dispose();
            };

            var vsWindow = VsInteractiveWindowFactory.Create(RGuidList.ReplInteractiveWindowProviderGuid, instanceId, Resources.ReplWindowName, evaluator);
            vsWindow.SetLanguage(RGuidList.RLanguageServiceGuid, ContentTypeRegistryService.GetContentType(RContentTypeDefinition.ContentType));
            vsWindow.InteractiveWindow.TextView.Closed += textViewOnClosed;

            var window = vsWindow.InteractiveWindow;
            InitializeWindowAsync(window).DoNotWait();

            return vsWindow;
        }

        private static async Task InitializeWindowAsync(IInteractiveWindow window) {
            await window.InitializeAsync();
            IVsUIShell shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            shell.UpdateCommandUI(1);
        }
    }
}
