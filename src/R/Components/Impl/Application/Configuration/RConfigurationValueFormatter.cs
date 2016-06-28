// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Runtime.Serialization;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Expressions.Definitions;
using Microsoft.R.Core.AST.Expressions;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.AST.Statements.Definitions;

namespace Microsoft.R.Components.Application.Configuration {
    public sealed class RConfigurationValueFormatter : IFormatter {
        public SerializationBinder Binder { get; set; }
        public StreamingContext Context { get; set; }
        public ISurrogateSelector SurrogateSelector { get; set; }

        public object Deserialize(Stream serializationStream) {
            using (var sr = new StreamReader(serializationStream)) {
                while (!sr.EndOfStream) {
                    var line = sr.ReadLine();
                    if(ast.)
                }
            }
        }

        public void Serialize(Stream serializationStream, object graph) {
            throw new NotImplementedException();
        }

        private bool GetNameAndValue(string line, out string name, out string value) {
            var ast = RParser.Parse(line);
            if(ast.Errors.Count == 0 && ast.Children.Count > 0) {
                var scope = ast.Children[0] as GlobalScope;
                if(scope?.Children.Count > 0) {
                    var es = scope.Children[0] as IExpressionStatement;
                    var exp = es?.Expression;
                    if (exp?.Children.Count == 1) {
                        var op = exp.Children[0] as IOperator;
                    }
                }
            }
        }
    }
}
