using System.ComponentModel.Composition;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Repl
{
    [Export(typeof(IVsInteractiveWindowProvider))]
    [AppliesTo("RTools")]
    internal sealed class RInteractiveWindowProvider : IVsInteractiveWindowProvider
    {
        private readonly IRSessionProvider _sessionProvider;
        private readonly IVsInteractiveWindowFactory _vsInteractiveWindowFactory;
        private readonly IContentTypeRegistryService _contentTypeRegistry;

        [ImportingConstructor]
        public RInteractiveWindowProvider(IRSessionProvider sessionProvider, IVsInteractiveWindowFactory vsInteractiveWindowFactory, IContentTypeRegistryService contentTypeRegistry)
        {
            _sessionProvider = sessionProvider;
            _vsInteractiveWindowFactory = vsInteractiveWindowFactory;
            _contentTypeRegistry = contentTypeRegistry;
        }

        public IVsInteractiveWindow Create(int instanceId)
        {
            var session = _sessionProvider.Create(instanceId);
            var evaluator = new RInteractiveEvaluator(session);
            var vsWindow = _vsInteractiveWindowFactory.Create(GuidList.ReplInteractiveWindowProviderGuid, instanceId, Resources.ReplWindowName, evaluator);
            vsWindow.SetLanguage(GuidList.LanguageServiceGuid, _contentTypeRegistry.GetContentType(RContentTypeDefinition.ContentType));

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
            var window = Create(instanceId);
            window.Show(focus);
        }
    }
}
