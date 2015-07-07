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
        private Lazy<PropertyDictionary> _properties = new Lazy<PropertyDictionary>(() => new PropertyDictionary());
        private IAstNode _parent;
        protected TextRangeCollection<IAstNode> _children = new TextRangeCollection<IAstNode>();

        #region IAstNode
        /// <summary>
        /// AST root node
        /// </summary>
        public virtual AstRoot Root
        {
            get { return Parent != null ? Parent.Root : null; }
        }

        /// <summary>
        /// This node's parent
        /// </summary>
        public IAstNode Parent
        {
            get { return _parent; }
            set
            {
                if (_parent != null && _parent != value && value != null)
                {
                    throw new InvalidOperationException("Node already has parent");
                }

                _parent = value;
                if (_parent != null)
                {
                    _parent.AppendChild(this);
                }
            }
        }

        public virtual IReadOnlyTextRangeCollection<IAstNode> Children
        {
            get { return _children; }
        }

        public void AppendChild(IAstNode child)
        {
            if (child.Parent == null)
            {
                child.Parent = this;
            }
            else if (child.Parent == this)
            {
                Debug.Assert(!_children.Contains(child.Start), "Children collection already contains this node");
                _children.Add(child);
                _children.Sort();
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
            get { return _properties.Value; }
        }
        #endregion

        #region IParseItem
        public virtual bool Parse(ParseContext context, IAstNode parent = null)
        {
            Parent = parent;
            return true;
        }
        #endregion

        #region ITextRange
        public virtual int Start
        {
            get { return Children.Count > 0 ? Children[0].Start : 0; }
        }

        public virtual int End
        {
            get { return Children.Count > 0 ? Children[Children.Count - 1].End : 0; }
        }

        public int Length
        {
            get { return End - Start; }
        }

        public virtual bool Contains(int position)
        {
            return Children.Contains(position);
        }

        public virtual void Shift(int offset)
        {
            Children.Shift(offset);
        }
        #endregion

        #region ICompositeTextRange
        public virtual void ShiftStartingFrom(int position, int offset)
        {
            Children.ShiftStartingFrom(position, offset);
        }
        #endregion

        #region IAstVisitorPattern
        public virtual bool Accept(IAstVisitor visitor, object parameter)
        {
            if (visitor != null && visitor.Visit(this, parameter))
            {
                for (int i = 0; i < Children.Count; i++)
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
