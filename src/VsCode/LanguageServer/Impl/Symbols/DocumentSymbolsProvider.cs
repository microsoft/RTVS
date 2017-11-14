// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using LanguageServer.VsCode.Contracts;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Functions;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Document;
using Microsoft.R.LanguageServer.Extensions;

namespace Microsoft.R.LanguageServer.Symbols {
    internal sealed class DocumentSymbolsProvider : IAstVisitor {
        private static readonly Guid _treeUserId = new Guid("5A8CE561-DC03-4CDA-8568-947DDB84F5FA");

        private sealed class SearchParams {
            public Uri Uri { get; }
            public List<SymbolInformation> Symbols { get; }
            public IEditorBufferSnapshot Snapshot { get; }
            public AstRoot Ast { get; }

            public SearchParams(Uri uri, IEditorBufferSnapshot snapshot, AstRoot ast) {
                Symbols = new List<SymbolInformation>();
                Uri = uri;
                Snapshot = snapshot;
                Ast = ast;
            }
        }

        public SymbolInformation[] GetSymbols(IREditorDocument document, Uri uri) {
            var ast = document.EditorTree.AcquireReadLock(_treeUserId);
            try {
                var p = new SearchParams(uri, document.EditorBuffer.CurrentSnapshot, ast);
                ast.Accept(this, p);
                return p.Symbols.ToArray();
            } finally {
                document.EditorTree.ReleaseReadLock(_treeUserId);
            }
        }

        public bool Visit(IAstNode node, object parameter) {
            var p = (SearchParams)parameter;
            SymbolKind kind = SymbolKind.Field; // never happens in R

            if (node is Variable) {
                kind = string.IsNullOrEmpty(p.Ast.IsInLibraryStatement(node.Start)) ? SymbolKind.Variable : SymbolKind.Package;
            } else if (node is IFunction) {
                kind = SymbolKind.Function;
            } else if (node is TokenNode t) {
                switch (t.Token.TokenType) {
                    case RTokenType.String:
                        kind = SymbolKind.String;
                        break;
                    case RTokenType.Null:
                    case RTokenType.NaN:
                        kind = SymbolKind.Constant;
                        break;
                    case RTokenType.Number:
                        kind = SymbolKind.Number;
                        break;
                    case RTokenType.Logical:
                        kind = SymbolKind.Boolean;
                        break;
                }
            }

            if (kind != SymbolKind.Field) {
                p.Symbols.Add(new SymbolInformation {
                    Kind = kind,
                    Location = new Location {
                        Uri = p.Uri,
                        Range = node.ToLineRange(p.Snapshot)
                    }
                });
            }
            return true;
        }

        private string ContainerName(IAstNode node) {
            while (node != null && !(node is AstRoot)) {
                if (node is IFunctionDefinition fd) {
                    return ((fd.Parent as IOperator)?.RightOperand as Variable)?.Name;
                }
                node = node.Parent;
            }
            return null;
        }
    }
}
