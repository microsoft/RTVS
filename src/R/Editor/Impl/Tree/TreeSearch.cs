using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Definitions;

namespace Microsoft.R.Editor.Tree
{
    public static class TreeSearch
    {
        public static IAstNode FindFirstElement(this AstRoot tree, Func<IAstNode, bool> filter)
        {
            var finder = new SingleElementFinder(filter);
            tree.Accept(finder, null);

            return finder.Result;
        }

        public static IReadOnlyCollection<IAstNode> FindElements(this AstRoot tree, Func<IAstNode, bool> filter)
        {
            MultipleElementFinder finder = new MultipleElementFinder(filter);
            tree.Accept(finder, null);

            if (finder.Result.Count == 0)
            {
                return ReadOnlyTextRangeCollection< IAstNode>.EmptyCollection;
            }

            return new ReadOnlyCollection<IAstNode>(finder.Result);
        }

        class SingleElementFinder : IAstVisitor
        {
            public IAstNode Result { get; private set; }
            private Func<IAstNode, bool> _match;

            public SingleElementFinder(Func<IAstNode, bool> filter)
            {
                _match = filter;
            }

            public bool Visit(IAstNode element, object parameter)
            {
                if (!_match(element))
                    return true;

                Result = element;
                return false;
            }
        }

        class MultipleElementFinder : IAstVisitor
        {
            public List<IAstNode> Result { get; private set; }
            private Func<IAstNode, bool> _match;

            public MultipleElementFinder(Func<IAstNode, bool> filter)
            {
                _match = filter;
                Result = new List<IAstNode>();
            }

            public bool Visit(IAstNode element, object parameter)
            {
                if (_match(element))
                    Result.Add(element);

                return true;
            }
        }
    }
}
