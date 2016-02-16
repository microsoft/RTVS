using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Signatures
{
    [Export(typeof(ISignatureHelpSourceProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [Name("R Signature Source Provider")]
    [Order(Before = "default")]
    sealed class SignatureHelpProvider : ISignatureHelpSourceProvider
    {
        public ISignatureHelpSource TryCreateSignatureHelpSource(ITextBuffer textBuffer)
        {
            var helpSource = ServiceManager.GetService<SignatureHelpSource>(textBuffer);
            
            if (helpSource == null)
                helpSource = new SignatureHelpSource(textBuffer);

            return helpSource;
        }
    }
}
