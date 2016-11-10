// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST.Comments;
using Microsoft.R.Core.AST.Evaluation.Definitions;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.Evaluation;
using Microsoft.R.Core.Parser;
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
            Errors = new TextRangeCollection<IParseError>();
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

        /// <summary>
        /// Finds deepest element node that contains given position
        /// </summary>
        /// <param name="position">Position</param>
        public override IAstNode NodeFromRange(ITextRange range, bool inclusiveEnd = false) {
            IAstNode node = base.NodeFromRange(range, inclusiveEnd);
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
        /// Updates positions of nodes in the tree reflecting multiple 
        /// changes made to the source text buffer.
        /// </summary>
        public void ReflectTextChanges(IReadOnlyCollection<TextChangeEventArgs> textChanges, ITextProvider newText) {
            foreach (TextChangeEventArgs curChange in textChanges) {
                ReflectTextChange(curChange.Start, curChange.OldLength, curChange.NewLength, newText);
            }
        }

        /// <summary>
        /// Updates positions of nodes in the tree reflecting changes 
        /// made to the source text buffer.
        /// </summary>
        /// <param name="start">Start position of the change</param>
        /// <param name="oldLength">Length of changed fragment before the change</param>
        /// <param name="newLength">Length of changed fragment after the change</param>
        /// <param name="newText">Complete new text snapshot</param>
        public void ReflectTextChange(int start, int oldLength, int newLength, ITextProvider newText) {
            TextProvider = newText;
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
