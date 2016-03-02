﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Expressions;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.Parser.Definitions;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.Parser {
    [DebuggerDisplay("{Tokens.Position} = {Tokens.CurrentToken.TokenType} : Errors = {Errors.Count}")]
    public sealed class ParseContext {
        private List<IParseError> _errors = new List<IParseError>();

        public AstRoot AstRoot { get; private set; }

        /// <summary>
        /// Provider of the text to parse
        /// </summary>
        public ITextProvider TextProvider { get; private set; }

        /// <summary>
        /// Token stream
        /// </summary>
        public TokenStream<RToken> Tokens { get; private set; }

        /// <summary>
        /// The range that is being parsed
        /// </summary>
        public ITextRange TextRange { get; private set; }

        /// <summary>
        /// Nested scopes where last element is the innermost scope
        /// </summary>
        public Stack<IScope> Scopes { get; set; }

        /// <summary>
        /// Parent of the expression being parsed. Some expression
        /// parts may need to know the expression parent during
        /// parsing and at that time it is not set on them yet.
        /// </summary>
        public Stack<Expression> Expressions { get; set; }

        /// <summary>
        /// Collection of parsing errors encountered so far
        /// </summary>
        public IReadOnlyCollection<IParseError> Errors {
            get { return _errors; }
        }

        /// <summary>
        /// Collection of comments in the file
        /// </summary>
        public IReadOnlyCollection<RToken> Comments { get; private set; }

        public ParseContext(ITextProvider textProvider, ITextRange range, TokenStream<RToken> tokens, IReadOnlyList<RToken> comments) {
            this.AstRoot = new AstRoot(textProvider);
            this.TextProvider = textProvider;
            this.Tokens = tokens;
            this.TextRange = range;
            this.Scopes = new Stack<IScope>();
            this.Expressions = new Stack<Expression>();
            this.Comments = comments;
        }

        public void AddError(ParseError error) {
            bool found = false;

            foreach (IParseError e in _errors) {
                if (e.Start == error.Start && e.Length == error.Length && e.ErrorType == error.ErrorType) {
                    found = true;
                    break;
                }
            }

            if (!found) {
                _errors.Add(error);
            }
        }
    }
}
