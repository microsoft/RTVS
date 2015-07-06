using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.ContentType;
using Microsoft.R.Editor.Completion;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Html.Editor.Completion.Provider
{
    /// <summary>
    /// Completion controller provider for Visual Studio core editor
    /// </summary>
    [Export(typeof(IIntellisenseControllerProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [Name("R Completion Controller")]
    [Order(Before = "Default Completion Controller")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class CompletionControllerProvider : IIntellisenseControllerProvider
    {
        [Import]
        public ICompletionBroker CompletionBroker { get; set; }

        [Import]
        public IQuickInfoBroker QuickInfoBroker { get; set; }

        [Import]
        public ISignatureHelpBroker SignatureHelpBroker { get; set; }

        public IIntellisenseController TryCreateIntellisenseController(ITextView view, IList<ITextBuffer> subjectBuffers)
        {
            return RCompletionController.Create(view, subjectBuffers, CompletionBroker, QuickInfoBroker, SignatureHelpBroker);
        }
    }
}
