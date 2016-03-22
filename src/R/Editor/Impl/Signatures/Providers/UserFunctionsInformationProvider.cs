// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Arguments;
using Microsoft.R.Core.AST.Expressions.Definitions;
using Microsoft.R.Core.AST.Functions.Definitions;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Editor.Signatures.Definitions;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Functions;

namespace Microsoft.R.Editor.Signatures.Providers {
    internal sealed class UserFunctionsInformationProvider : IFunctionInformationProvider {
        public IFunctionInfo GetFunctionInfo(RSignatureHelpContext context, string functionName,
                                  Action<object> infoReadyCallback = null, object parameter = null) {
            var scope = context.AstRoot.GetNodeOfTypeFromPosition<IScope>(context.Position);
            if (scope == null) {
                return null;
            }
            var fd = scope.FindFunctionByName(functionName)?.Value as IFunctionDefinition;
            if (fd != null) {
                var fi = new FunctionInfo(functionName);
                fi.Signatures = new ISignatureInfo[] { MakeSignature(functionName, fd) };
                return fi;
            }
            return null;
        }

        private ISignatureInfo MakeSignature(string functionName, IFunctionDefinition fd) {
            var si = new SignatureInfo(functionName);
            si.Arguments = new List<IArgumentInfo>();
            foreach (var arg in fd.Arguments) {
                var na = arg as NamedArgument;
                if (na != null) {
                    si.Arguments.Add(new ArgumentInfo(na.Name));
                } else {
                    var ea = arg as ExpressionArgument;
                    if (ea != null && ea.Children.Count > 0) {
                        var exp = ea.Children[0] as IExpression;
                        if (ecp != null && exp.Children.Count > 0) {
                            var v = exp.Children[0] as Variable;
                            if (v != null) {
                                si.Arguments.Add(new ArgumentInfo(v.Name));
                            }
                        }
                    }
                }
            }
            return si;
        }
    }
}
