using System.ComponentModel.Composition;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.R.Package.Repl.Session;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Repl
{
    internal sealed class RInteractiveWindowProvider : IVsInteractiveWindowProvider
    {
        [Import]
        private IContentTypeRegistryService ContentTypeRegistryService { get; set; }

        [Import]
        private IVsInteractiveWindowFactory VsInteractiveWindowFactory { get; set; }

        private readonly IRSessionProvider _sessionProvider;

        public RInteractiveWindowProvider()
        {
            AppShell.Current.CompositionService.SatisfyImportsOnce(this);
            _sessionProvider = new RSessionProvider();
        }

        public IVsInteractiveWindow Create(int instanceId)
        {
            var session = _sessionProvider.Create(instanceId);
            var evaluator = new RInteractiveEvaluator(session);
            var vsWindow = VsInteractiveWindowFactory.Create(RGuidList.ReplInteractiveWindowProviderGuid, instanceId, Resources.ReplWindowName, evaluator);
            vsWindow.SetLanguage(RGuidList.RLanguageServiceGuid, ContentTypeRegistryService.GetContentType(RContentTypeDefinition.ContentType));

            vsWindow.InteractiveWindow.TextView.Closed += (_, __) =>
            {
                evaluator.Dispose();
                session.Dispose();
            };

            var window = vsWindow.InteractiveWindow;
            // fire and forget:
            window.InitializeAsync();

            return vsWindow;
        }

        public void Open(int instanceId, bool focus)
        {
            if (!ReplWindow.ReplWindowExists())
            {
                var window = Create(instanceId);
                window.Show(focus);
            }
            else
            {
                ReplWindow.Show();
            }
        }
    }
}
