using System;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Utility;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Core.AST
{
    /// <summary>
    /// Base class of AST nodes that can have child nodes such as expressions, 
    /// loops, assignments and other statements. Complex nodes acts as composite 
    /// text ranges that encompass all tokens that constitute the node. Supports 
    /// visitor pattern that allows traversal of the subtree starting at this node. 
    /// It is a property owner and allows attaching arbitrary properties.
    /// </summary>
    [DebuggerDisplay("[Children = {Children.Count}  {Start}...{End}, Length = {Length}]")]
    public abstract class AstNode : IAstNode
    {
        private Lazy<PropertyDictionary> properties = new Lazy<PropertyDictionary>(() => new PropertyDictionary());
        private IAstNode parent;
        protected TextRangeCollection<IAstNode> children = new TextRangeCollection<IAstNode>();

        #region IAstNode
        /// <summary>
        /// AST root node
        /// </summary>
        public virtual AstRoot Root
        {
            get { return this.Parent != null ? this.Parent.Root : null; }
        }

        /// <summary>
        /// This node's parent
        /// </summary>
        public IAstNode Parent
        {
            get { return this.parent; }
            set
            {
                if (this.parent != null && this.parent != value && value != null)
                {
                    throw new InvalidOperationException("Node already has parent");
                }

                this.parent = value;
                if (this.parent != null)
                {
                    this.parent.AppendChild(this);
                }
            }
        }

        public virtual IReadOnlyTextRangeCollection<IAstNode> Children
        {
            get { return this.children; }
        }

        public void AppendChild(IAstNode child)
        {
            if (child.Parent == null)
            {
                child.Parent = this;
            }
            else if (child.Parent == this)
            {
                Debug.Assert(!this.children.Contains(child.Start), "Children collection already contains this node");
                this.children.Add(child);
                this.children.Sort();
            }
            else
            {
                throw new InvalidOperationException("Node already has parent");
            }
        }
        #endregion

        #region IPropertyOwner
        public PropertyDictionary Properties
        {
            get { return this.properties.Value; }
        }
        #endregion

        #region IParseItem
        public virtual bool Parse(ParseContext context, IAstNode parent = null)
        {
            this.Parent = parent;
            return true;
        }
        #endregion

        #region ITextRange
        public virtual int Start
        {
            get { return this.Children.Count > 0 ? this.Children[0].Start : 0; }
        }

        public virtual int End
        {
            get { return this.Children.Count > 0 ? this.Children[this.Children.Count - 1].End : 0; }
        }

        public int Length
        {
            get { return this.End - this.Start; }
        }

        public virtual bool Contains(int position)
        {
            return this.Children.Contains(position);
        }

        public virtual void Shift(int offset)
        {
            this.Children.Shift(offset);
        }
        #endregion

        #region ICompositeTextRange
        public virtual void ShiftStartingFrom(int position, int offset)
        {
            this.Children.ShiftStartingFrom(position, offset);
        }
        #endregion

        #region IAstVisitorPattern
        public virtual bool Accept(IAstVisitor visitor, object parameter)
        {
            if (visitor != null && visitor.Visit(this, parameter))
            {
                for (int i = 0; i < this.Children.Count; i++)
                {
                    var child = Children[i] as IAstNode;

                    if (child != null && !child.Accept(visitor, parameter))
                        return false;
                }

                return true;
            }

            return false;
        }

        public virtual bool Accept(Func<IAstNode, object, bool> visitor, object parameter)
        {
            if (visitor != null && visitor(this, parameter))
            {
                foreach (IAstNode child in Children)
                {
                    if (!child.Accept(visitor, parameter))
                        return false;
                }

                return true;
            }

            return false;
        }
        #endregion
    }
}
