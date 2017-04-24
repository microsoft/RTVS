using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Undo;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Test.Mocks {
    [ExcludeFromCodeCoverage]
    public class ApplicationEditorSupportMock : IApplicationEditorSupport {
        public ICommandTarget TranslateCommandTarget(ITextView textView, object commandTarget) => commandTarget as ICommandTarget;
        public object TranslateToHostCommandTarget(ITextView textView, object commandTarget) => commandTarget;
        public ICompoundUndoAction CreateCompoundAction(ITextView textView, ITextBuffer textBuffer) => new CompoundUndoActionMock();
    }
}