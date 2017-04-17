// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Signatures;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Editor.Functions;

namespace Microsoft.R.Editor.Signatures {
    public sealed class RSignatureSource : IFunctionSignatureSource {
        private readonly ICoreShell _coreShell;
        private readonly DisposeToken _disposeToken;
        private string _packageName;

        public RSignatureSource(ICoreShell coreShell) {
            _coreShell = coreShell;
        }

        #region IFunctionSignatureSource
        public async Task<IEnumerable<IFunctionSignature>> GetSignaturesAsync(IRCompletionContext context) {
            if (!_coreShell.GetService<IREditorSettings>().SignatureHelpEnabled || context.Session.IsDismissed) {
                return Enumerable.Empty<IFunctionSignature>();
            }

            var snapshot = context.EditorBuffer.CurrentSnapshot;
            var position = context.Position;
            // Retrieve parameter positions from the current text buffer snapshot
            var parametersInfo = FunctionParameter.FromEditorBuffer(context.AstRoot, snapshot, context.Position);
            if (parametersInfo == null) {
                return Enumerable.Empty<IFunctionSignature>();
            }

            position = Math.Min(parametersInfo.SignatureEnd, position);
            var start = Math.Min(position, snapshot.Length);
            var end = Math.Min(parametersInfo.SignatureEnd, snapshot.Length);

            var applicableToSpan = snapshot.CreateTrackingRange(TextRange.FromBounds(start, end));
            IFunctionInfo functionInfo = null;
            string packageName = null;

            // First try user-defined function
            if (string.IsNullOrEmpty(parametersInfo.PackageName)) {
                functionInfo = context.AstRoot.GetUserFunctionInfo(parametersInfo.FunctionName, position);
            } else {
                packageName = parametersInfo.PackageName;
            }

            if (functionInfo == null) {
                var functionIndex = _coreShell.GetService<IFunctionIndex>();
                // Then try package functions
                packageName = packageName ?? _packageName;
                _packageName = null;
                // Get collection of function signatures from documentation (parsed RD file)
                functionInfo = await functionIndex.GetFunctionInfoAsync(parametersInfo.FunctionName, packageName);
            }

            var signatures = new List<IFunctionSignature>();
            if (functionInfo?.Signatures != null) {
                foreach (var signatureInfo in functionInfo.Signatures) {
                    var signature = FunctionSignature.Create(context, functionInfo, signatureInfo);
                    signatures.Add(signature);
                }
                context.Session.Properties["functionInfo"] = functionInfo;
            }
            return signatures;
        }
        #endregion
    }
}
