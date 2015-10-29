using System;
using System.ComponentModel.Composition;
using Microsoft.Common.Core;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Utility;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Repl {
    internal sealed class RInteractiveWindowProvider : IVsInteractiveWindowProvider {
        [Import]
        private IContentTypeRegistryService ContentTypeRegistryService { get; set; }

        [Import]
        private IVsInteractiveWindowFactory VsInteractiveWindowFactory { get; set; }

        [Import]
        private IRSessionProvider SessionProvider { get; set; }

        public RInteractiveWindowProvider() {
            AppShell.Current.CompositionService.SatisfyImportsOnce(this);
        }

        public IVsInteractiveWindow Create(int instanceId) {

            IInteractiveEvaluator evaluator;
            EventHandler textViewOnClosed;

            if (RInstallation.VerifyRIsInstalled()) {
                var session = SessionProvider.Create(instanceId);
                evaluator = new RInteractiveEvaluator(session);

                EventHandler<EventArgs> clearPendingInputsHandler = (sender, args) => ReplWindow.Current.ClearPendingInputs();
                session.Disconnected += clearPendingInputsHandler;

                textViewOnClosed = (_, __) => {
                    session.Disconnected -= clearPendingInputsHandler;
                    evaluator.Dispose();
                    session.Dispose();
                };
            } else {
                evaluator = new NullInteractiveEvaluator();
                textViewOnClosed = (_, __) => { evaluator.Dispose(); };
            }

            var vsWindow = VsInteractiveWindowFactory.Create(RGuidList.ReplInteractiveWindowProviderGuid, instanceId, Resources.ReplWindowName, evaluator);
            vsWindow.SetLanguage(RGuidList.RLanguageServiceGuid, ContentTypeRegistryService.GetContentType(RContentTypeDefinition.ContentType));
            vsWindow.InteractiveWindow.TextView.Closed += textViewOnClosed;

            var window = vsWindow.InteractiveWindow;
            window.InitializeAsync().DoNotWait();

            return vsWindow;
        }

        public void Open(int instanceId, bool focus) {
            if (!ReplWindow.ReplWindowExists()) {
                var window = Create(instanceId);
                window.Show(focus);
            } else {
                ReplWindow.Show();
            }
        }
    }
}
