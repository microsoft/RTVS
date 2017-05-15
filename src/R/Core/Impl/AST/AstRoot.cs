// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
        public ITextProvider TextProvider { get; internal set; }

        public CommentsCollection Comments { get; private set; }

        public ICodeEvaluator CodeEvaluator { get; }

        public TextRangeCollection<IParseError> Errors { get; internal set; }

        public AstRoot(ITextProvider textProvider) : this(textProvider, new CodeEvaluator()) { }

        public AstRoot(ITextProvider textProvider, ICodeEvaluator codeEvaluator) {
            TextProvider = textProvider;
            Comments = new CommentsCollection();
            Errors = new TextRangeCollection<IParseError>();
            CodeEvaluator = codeEvaluator;
        }

        #region IAstNode
        public override AstRoot Root => this;

        /// <summary>
        /// Finds deepest element node that contains given position
        /// </summary>
        /// <param name="position">Position</param>
        public override IAstNode NodeFromPosition(int position) 
            => base.NodeFromPosition(position) ?? this;

        /// <summary>
        /// Finds deepest element node that contains given position
        /// </summary>
        public override IAstNode NodeFromRange(ITextRange range, bool inclusiveEnd = false) 
            => base.NodeFromRange(range, inclusiveEnd) ?? this;

        #endregion

        #region ITextRange
        public override int Start => 0;
        public override int End => TextProvider.Length;
        public override bool Contains(int position) => position >= Start && position <= End;
        #endregion

        public override bool Parse(ParseContext context, IAstNode parent = null) {
            // Remove comments from the token stream
            Comments = new CommentsCollection(context.Comments);

            var globalScope = new GlobalScope();
            return globalScope.Parse(context, this);
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
            var offset = newLength - oldLength;
            ShiftStartingFrom(start, offset);
            Comments.ReflectTextChange(start, oldLength, newLength);
        }

        public string WriteTree() {
            var writer = new AstWriter();
            return writer.WriteTree(this);
        }
    }
}
