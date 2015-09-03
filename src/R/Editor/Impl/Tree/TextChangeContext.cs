using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Editor.Tree
{
    /// <summary>
    /// Describes complete context of the text change
    /// including text ranges, affected editor tree
    /// and changed AST node.
    /// </summary>
    internal class TextChangeContext
    {
        public EditorTree EditorTree { get; private set; }

        public int Start { get; private set; }
        public int OldStart { get; private set; }
        public int OldLength { get; private set; }
        public int NewLength { get; private set; }

        public ITextProvider OldTextProvider { get; private set; }
        public ITextProvider NewTextProvider { get; private set; }

        public TextChange TextChange { get; private set; }
        public IAstNode ChangedNode { get; set; }
        public RToken ChangedComment { get; set; }

        private TextRange _oldRange;
        private TextRange _newRange;
        private string _oldText;
        private string _newText;

        public TextChangeContext(EditorTree editorTree, TextChangeEventArgs args, TextChange textChange)
        {
            EditorTree = editorTree;
            Start = args.Start;
            OldStart = args.OldStart;
            OldLength = args.OldLength;
            NewLength = args.NewLength;

            OldTextProvider = args.OldText != null ? args.OldText : editorTree.AstRoot.TextProvider;
            NewTextProvider = args.NewText != null ? args.NewText : new TextProvider(editorTree.TextBuffer.CurrentSnapshot, partial: true);

            TextChange = textChange;
        }

        public TextRange OldRange
        {
            get
            {
                if (_oldRange == null)
                    _oldRange = new TextRange(OldStart, OldLength);

                return _oldRange;
            }
        }

        public TextRange NewRange
        {
            get
            {
                if (_newRange == null)
                    _newRange = new TextRange(Start, NewLength);

                return _newRange;
            }
        }

        public string OldText
        {
            get
            {
                if (_oldText == null)
                    _oldText = OldTextProvider.GetText(OldRange);

                return _oldText;
            }
        }

        public string NewText
        {
            get
            {
                if (_newText == null)
                    _newText = NewTextProvider.GetText(NewRange);

                return _newText;
            }
        }
    }
}
