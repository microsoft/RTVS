using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST.Comments;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Evaluation.Definitions;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.Evaluation;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Parser.Definitions;
using Microsoft.R.Core.Utility;

namespace Microsoft.R.Core.AST {
    [DebuggerDisplay("AstRoot, Comments: {Comments.Count}, Errors: {Errors.Count}")]
    public sealed class AstRoot : AstNode {
        private TextRangeCollection<IParseError> _errors = new TextRangeCollection<IParseError>();

        public ITextProvider TextProvider { get; internal set; }

        public CommentsCollection Comments { get; private set; }

        public ICodeEvaluator CodeEvaluator { get; private set; }

        public TextRangeCollection<IParseError> Errors { get; internal set; }

        public AstRoot(ITextProvider textProvider) :
            this(textProvider, new CodeEvaluator()) {
        }

        public AstRoot(ITextProvider textProvider, ICodeEvaluator codeEvaluator) {
            TextProvider = textProvider;
            Comments = new CommentsCollection();
            CodeEvaluator = codeEvaluator;
        }

        #region IAstNode
        public override AstRoot Root {
            get { return this; }
        }

        /// <summary>
        /// Finds deepest element node that contains given position
        /// </summary>
        /// <param name="position">Position</param>
        public override IAstNode NodeFromPosition(int position) {
            IAstNode node = base.NodeFromPosition(position);
            return node ?? this;
        }

        #endregion

        #region ITextRange
        public override int Start {
            get { return 0; }
        }

        public override int End {
            get { return TextProvider.Length; }
        }

        public override bool Contains(int position) {
            return position >= Start && position <= End;
        }
        #endregion

        public override bool Parse(ParseContext context, IAstNode parent) {
            // Remove comments from the token stream
            this.Comments = new CommentsCollection(context.Comments);

            GlobalScope globalScope = new GlobalScope();
            return globalScope.Parse(context, this);
        }


        /// <summary>
        /// Updates positions of all elements and attributes in the tree
        /// reflecting multiple changes made to the source text buffer.
        /// </summary>
        public void ReflectTextChanges(IReadOnlyCollection<TextChangeEventArgs> textChanges) {
            foreach (TextChangeEventArgs curChange in textChanges) {
                ReflectTextChange(curChange.Start, curChange.OldLength, curChange.NewLength);
            }
        }

        /// <summary>
        /// Updates positions of all elements and attributes in the tree
        /// reflecting change made to the source text buffer.
        /// </summary>
        /// <param name="start">Start position of the change</param>
        /// <param name="oldLength">Length of changed fragment before the change</param>
        /// <param name="newLength">Length of changed fragment after the change</param>
        public void ReflectTextChange(int start, int oldLength, int newLength) {
            // Note that shifting tree elements also shifts artifacts in 
            // element attributes. We need to track these changes in order
            // to avoid double shifts in artifacts.

            int offset = newLength - oldLength;
            ShiftStartingFrom(start, offset);

            Comments.ReflectTextChange(start, oldLength, newLength);
        }

        public string WriteTree() {
            AstWriter writer = new AstWriter();
            return writer.WriteTree(this);
        }
    }
}
