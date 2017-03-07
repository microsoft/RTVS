// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Arguments;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Editor.Signatures;
using Microsoft.R.Support.Help;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Completion.Providers {
    /// <summary>
    /// Provides list of parameter names in function parameter completion
    /// in the form of 'name=' so when parameter name is the simiar
    /// to a function name, user can choose either subset() or subset=
    /// </summary>
    public class ParameterNameCompletionProvider : IRCompletionListProvider {
        private readonly IFunctionIndex _functionIndex;
        private readonly IGlyphService _glyphService;

        public ParameterNameCompletionProvider(IFunctionIndex functionIndex, IGlyphService glyphService) {
            _functionIndex = functionIndex;
            _glyphService = glyphService;
        }

        #region IRCompletionListProvider
        public bool AllowSorting { get; } = true;

        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context) {
            List<RCompletion> completions = new List<RCompletion>();
            FunctionCall funcCall;
            ImageSource functionGlyph = _glyphService.GetGlyphThreadSafe(StandardGlyphGroup.GlyphGroupValueType, StandardGlyphItem.GlyphItemPublic);

            // Safety checks
            if (!ShouldProvideCompletions(context, out funcCall)) {
                return completions;
            }

            // Get collection of function signatures from documentation (parsed RD file)
            IFunctionInfo functionInfo = GetFunctionInfo(context);
            if (functionInfo == null) {
                return completions;
            }

            // Collect parameter names from all signatures
            IEnumerable<KeyValuePair<string, IArgumentInfo>> arguments = new Dictionary<string, IArgumentInfo>();
            foreach (ISignatureInfo signature in functionInfo.Signatures) {
                var args = signature.Arguments.ToDictionary(x => x.Name);
                arguments = arguments.Union(args);
            }

            // Add names of arguments that  are not yet specified to the completion
            // list with '=' sign so user can tell them from function names.
            IEnumerable<string> declaredArguments = funcCall.Arguments.Where(x => x is NamedArgument).Select(x => ((NamedArgument)x).Name);
            var possibleArguments = arguments.Where(x => !x.Key.EqualsOrdinal("...") && !declaredArguments.Contains(x.Key, StringComparer.OrdinalIgnoreCase));

            foreach (var arg in possibleArguments) {
                string displayText = arg.Key + (REditorSettings.FormatOptions.SpacesAroundEquals ? " =" : "=");
                string insertionText = arg.Key + (REditorSettings.FormatOptions.SpacesAroundEquals ? " = " : "=");
                completions.Add(new RCompletion(displayText, insertionText, arg.Value.Description, functionGlyph));
            }

            return completions;
        }
        #endregion

        private static bool ShouldProvideCompletions(RCompletionContext context, out FunctionCall funcCall) {
            // Safety checks
            funcCall = context.AstRoot.GetNodeOfTypeFromPosition<FunctionCall>(context.Position);
            if (funcCall == null || funcCall.OpenBrace == null || funcCall.Arguments == null) {
                return false;
            }

            if (context.Position < funcCall.OpenBrace.End || context.Position >= funcCall.SignatureEnd) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Extracts information on the current function in the completion context, if any.
        /// </summary>
        /// <returns></returns>
        private IFunctionInfo GetFunctionInfo(RCompletionContext context) {
            // Retrieve parameter positions from the current text buffer snapshot
            IFunctionInfo functionInfo = null;

            ParameterInfo parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(context.AstRoot, context.TextBuffer.CurrentSnapshot, context.Position);
            if (parametersInfo != null) {
                // User-declared functions take priority
                functionInfo = context.AstRoot.GetUserFunctionInfo(parametersInfo.FunctionName, context.Position);
                if (functionInfo == null) {
                    // Get collection of function signatures from documentation (parsed RD file)
                    functionInfo = _functionIndex.GetFunctionInfo(parametersInfo.FunctionName, null, (o, p) => { }, context.Session.TextView);
                }
            }
            return functionInfo;
        }
    }
}
