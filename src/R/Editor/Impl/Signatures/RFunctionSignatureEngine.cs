// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Completions;
using Microsoft.R.Editor.Functions;

namespace Microsoft.R.Editor.Signatures {
    /// <summary>
    /// Implements engine that provides information on function signature
    /// given intellisense context (text buffer, position, AST).
    /// </summary>
    public sealed class RFunctionSignatureEngine : IRFunctionSignatureEngine {
        private readonly IServiceContainer _services;
        private readonly IFunctionIndex _functionIndex;

        public RFunctionSignatureEngine(IServiceContainer services) {
            _services = services;
            _functionIndex = _services.GetService<IFunctionIndex>();
        }

        #region IFunctionSignatureEngine
        public async Task<IEnumerable<IRFunctionSignatureHelp>> GetSignaturesAsync(IRIntellisenseContext context) {
            if (!_services.GetService<IREditorSettings>().SignatureHelpEnabled || context.Session.IsDismissed) {
                return Enumerable.Empty<IRFunctionSignatureHelp>();
            }

            var snapshot = context.EditorBuffer.CurrentSnapshot;
            var position = context.Position;
            // Retrieve parameter positions from the current text buffer snapshot
            var signatureInfo = context.AstRoot.GetSignatureInfoFromBuffer(snapshot, context.Position);
            if (signatureInfo == null) {
                return Enumerable.Empty<IRFunctionSignatureHelp>();
            }

            position = Math.Min(signatureInfo.FunctionCall.SignatureEnd, position);
            var start = Math.Min(position, snapshot.Length);
            var end = Math.Min(signatureInfo.FunctionCall.SignatureEnd, snapshot.Length);

            var applicableToSpan = snapshot.CreateTrackingRange(TextRange.FromBounds(start, end));
            IFunctionInfo functionInfo = null;
            string packageName = null;

            // First try user-defined function
            if (string.IsNullOrEmpty(signatureInfo.PackageName)) {
                functionInfo = context.AstRoot.GetUserFunctionInfo(signatureInfo.FunctionName, position);
            } else {
                packageName = signatureInfo.PackageName;
            }

            if (functionInfo == null) {
                // Get collection of function signatures from documentation (parsed RD file)
                functionInfo = await _functionIndex.GetFunctionInfoAsync(signatureInfo.FunctionName, packageName);
            }

            var signatures = new List<IRFunctionSignatureHelp>();
            if (functionInfo?.Signatures != null) {
                foreach (var s in functionInfo.Signatures) {
                    var signature = RFunctionSignatureHelp.Create(context, functionInfo, s, applicableToSpan);
                    signatures.Add(signature);
                }
                context.Session.Properties["functionInfo"] = functionInfo;
            }
            return signatures;
        }
        #endregion
    }
}
