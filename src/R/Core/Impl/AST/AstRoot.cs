using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Evaluation.Definitions;
using Microsoft.R.Core.AST.Keys;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.Evaluation;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Parser.Definitions;

namespace Microsoft.R.Core.AST
{
    public sealed class AstRoot : AstNode
    {
        public ITextProvider TextProvider { get; internal set; }

        /// <summary>
        /// A collection of keys to all elements in the tree
        /// </summary>
        public NodeKeys Keys { get; private set; }

        public TextRangeCollection<TokenNode> Comments { get; internal set; }

        public ICodeEvaluator CodeEvaluator { get; private set; }

        public List<IParseError> Errors { get; internal set; }

        public AstRoot(ITextProvider textProvider) :
            this(textProvider, new CodeEvaluator())
        {
        }

        public AstRoot(ITextProvider textProvider, ICodeEvaluator codeEvaluator)
        {
            TextProvider = textProvider;
            Keys = new NodeKeys(this);
            Comments = new TextRangeCollection<TokenNode>();
            CodeEvaluator = codeEvaluator;
        }

        #region IAstNode
        public override AstRoot Root
        {
            get { return this; }
        }

        /// <summary>
        /// Finds deepest element node that contains given position
        /// </summary>
        /// <param name="position">Position</param>
        public override IAstNode NodeFromPosition(int position)
        {
            IAstNode node = base.NodeFromPosition(position);
            return node ?? this;
        }

        #endregion

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            // Remove comments from the token stream
            context.RemoveCommentTokens();

            GlobalScope globalScope = new GlobalScope();
            return globalScope.Parse(context, this);
        }


        public void ReflectTextChange(int start, int oldLength, int newLength)
        {
            // Note that shifting tree elements also shifts artifacts in 
            // element attributes. We need to track these changes in order
            // to avoid double shifts in artifacts.

            int offset = newLength - oldLength;
            ShiftStartingFrom(start, offset);

            Comments.ReflectTextChange(start, oldLength, newLength);
        }

        #region Key operations
        /// <summary>
        /// Checks if particular node is still in the tree. Method is thread-safe.
        /// However, if large amount of text has been deleted, key may still briefly
        /// be present in the collection. If you intend to use element ranges from 
        /// a background thread make sure to be working against appropriate text 
        /// buffer snapshot.
        /// </summary>
        /// <param name="key">Node key</param>
        /// <returns>True if node still exists.</returns>
        public bool ContainsNode(int key)
        {
            if (key == 0)
                return true; // Tree always contains root element

            return Keys[key] != null;
        }

        /// <summary>
        /// Retrieves node by its key. Must only be called if caller has tree read lock.
        /// Node may become invalid during next tree update unless caller is holding 
        /// a read lock or caller is on the application main thread.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IAstNode GetNode(int key)
        {
            return Keys[key];
        }
        #endregion
    }
}
