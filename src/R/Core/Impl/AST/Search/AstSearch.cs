// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.R.Core.AST.Arguments;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST {
    public static class AstSearch {
        public static IAstNode FindFirstElement(this AstNode tree, Func<IAstNode, bool> filter) {
            var finder = new SingleElementFinder(filter);
            tree.Accept(finder, null);

            return finder.Result;
        }

        public static IReadOnlyCollection<IAstNode> FindElements(this AstRoot tree, Func<IAstNode, bool> filter) {
            var finder = new MultipleElementFinder(filter);
            tree.Accept(finder, null);

            if (finder.Result.Count == 0) {
                return new IAstNode[0];
            }

            return new ReadOnlyCollection<IAstNode>(finder.Result);
        }

        class SingleElementFinder : IAstVisitor {
            public IAstNode Result { get; private set; }
            private Func<IAstNode, bool> _match;

            public SingleElementFinder(Func<IAstNode, bool> filter) {
                _match = filter;
            }

            public bool Visit(IAstNode element, object parameter) {
                if (!_match(element)) {
                    return true;
                }

                Result = element;
                return false;
            }
        }

        class MultipleElementFinder : IAstVisitor {
            public List<IAstNode> Result { get; private set; }
            private Func<IAstNode, bool> _match;

            public MultipleElementFinder(Func<IAstNode, bool> filter) {
                _match = filter;
                Result = new List<IAstNode>();
            }

            public bool Visit(IAstNode element, object parameter) {
                if (_match(element)) {
                    Result.Add(element);
                }

                return true;
            }
        }

        /// <summary>
        /// Locates deepest node of a particular type 
        /// </summary>
        public static T GetNodeOfTypeFromPosition<T>(this AstRoot ast, int position, bool includeEnd = false) where T : class {
            return GetSpecificNodeFromPosition<T>(ast, position, n => n is T, includeEnd);
        }

        /// <summary>
        /// Locates deepest node that matches partucular criteria 
        /// and contains given position in the text buffer
        /// </summary>
        public static T GetSpecificNodeFromPosition<T>(this AstRoot ast, int position, Func<IAstNode, bool> match, bool includeEnd = false) where T : class {
            IAstNode deepestNode = null;
            FindSpecificNode(ast, position, match, ref deepestNode, includeEnd);
            if(deepestNode == null && ast.Children.Count > 0) {
                deepestNode = ast.Children[0]; // Global scope if nothing was found
            }
            return deepestNode as T;
        }

        private static void FindSpecificNode(IAstNode node, int position, Func<IAstNode, bool> match, ref IAstNode deepestNode, bool includeEnd = false) {
            if (position == node.Start || (!node.Contains(position) && !(includeEnd && node.End == position))) {
                return; // not this element
            }

            if (match(node)) {
                deepestNode = node;
            }

            for (var i = 0; i < node.Children.Count && node.Children[i].Start <= position; i++) {
                FindSpecificNode(node.Children[i], position, match, ref deepestNode, includeEnd);
            }
        }

        /// <summary>
        /// Retrieves list of packages available to the current file.
        /// Consists of packages in the base library and packages
        /// added via 'library' statements.
        /// </summary>
        public static IEnumerable<string> GetFilePackageNames(this AstRoot ast) {
            // TODO: results can be cached until AST actually changes
            var search = new AstLibrarySearch();
            ast.Accept(search, null);

            return search.PackageNames;
        }

        private class AstLibrarySearch : IAstVisitor {
            public List<string> PackageNames { get; } = new List<string>();

            public bool Visit(IAstNode node, object parameter) {
                var fc = node as FunctionCall;
                if (fc?.Arguments != null && fc.Arguments.Count > 0) {

                    // Function name is a Variable and is a child of () operator
                    var functionNameVariable = fc.Children.Count > 0 ? fc.Children[0] as Variable : null;
                    if (functionNameVariable != null) {

                        if (functionNameVariable.Name == "library" || functionNameVariable.Name == "require") {
                            // Now get the argument list. first argument, if any, is the package name.
                            if (fc.Arguments[0] is ExpressionArgument arg && arg.ArgumentValue.Children.Count == 1) {
                                // Technically we need to evaluate the expression and calculate 
                                // the actual package name in case it is constructed or returned 
                                // from another function. However, for now we limit search to 
                                // single value arguments like library(abind).
                                var packageNameVariable = arg.ArgumentValue.Children[0] as Variable;
                                if (packageNameVariable != null) {
                                    PackageNames.Add(packageNameVariable.Name);
                                }
                            }
                        }
                    }
                }
                return true;
            }
        }

        public static bool IsPositionInsideString(this AstRoot ast, int position) {
            // We don't want to auto-format inside strings
            var node = ast.NodeFromPosition(position) as TokenNode;
            return node != null && node.Token.TokenType == RTokenType.String;
        }

        public static string IsInLibraryStatement(this AstRoot ast, int position) {
            var fc = ast.GetNodeOfTypeFromPosition<FunctionCall>(position);
            if (fc?.RightOperand != null) {
                var funcName = ast.TextProvider.GetText(fc.RightOperand);
                if (funcName.Equals("library", StringComparison.Ordinal)) {
                    if (fc.Arguments.Count == 1) {
                        return ast.TextProvider.GetText(fc.Arguments[0]);
                    }
                }
            }
            return string.Empty;
        }
    }
}
