// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageServer.VsCode.Contracts;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Editor;
using Microsoft.R.Editor.Functions;
using Microsoft.R.Editor.QuickInfo;
using Microsoft.R.Editor.Signatures;
using Microsoft.R.LanguageServer.Extensions;

namespace Microsoft.R.LanguageServer.Completions {
    internal sealed class SignatureManager {
        private readonly IServiceContainer _services;
        private readonly RFunctionSignatureEngine _signatureEngine;

        public SignatureManager(IServiceContainer services) {
            _services = services;
            _signatureEngine = new RFunctionSignatureEngine(services);
        }

        public Task<SignatureHelp> GetSignatureHelpAsync(IRIntellisenseContext context) {
            using (context.AstReadLock()) {
                var tcs = new TaskCompletionSource<SignatureHelp>();
                var sigs = _signatureEngine.GetSignaturesAsync(context, e => tcs.TrySetResult(ToSignatureHelp(e, context)));
                if (sigs != null) {
                    return Task.FromResult(ToSignatureHelp(sigs, context));
                }
                return tcs.Task;
            }
        }

        public Task<Hover> GetHoverAsync(IRIntellisenseContext context, CancellationToken ct) {
            var tcs = new TaskCompletionSource<Hover>();
            using (context.AstReadLock()) {
                var infos = _signatureEngine.GetQuickInfosAsync(context, e => {
                    tcs.TrySetResult(!ct.IsCancellationRequested ? ToHover(e.ToList(), context.EditorBuffer) : null);
                });
                return infos != null ? Task.FromResult(ToHover(infos.ToList(), context.EditorBuffer)) : tcs.Task;
            }
        }

        private SignatureHelp ToSignatureHelp(IEnumerable<IRFunctionSignatureHelp> signatures, IRIntellisenseContext context) {
            var sigInfos = signatures.Select(s => new SignatureInformation {
                Label = s.Content,
                Documentation = s.Documentation,
                Parameters = s.Parameters.Select(p => new ParameterInformation {
                    Label = p.Name,
                    Documentation = p.Documentation
                }).ToList()
            }).ToList();

            return new SignatureHelp {
                Signatures = sigInfos,
                ActiveParameter = sigInfos.Count > 0 ? ComputeActiveParameter(context, signatures.First().SignatureInfo) : 0
            };
        }

        private static Hover ToHover(IReadOnlyList<IRFunctionQuickInfo> e, IEditorBuffer buffer) {
            if (e.Count > 0) {
                var info = e[0];
                var content = info.Content?.FirstOrDefault();
                if (!string.IsNullOrEmpty(content)) {
                    var snapshot = buffer.CurrentSnapshot;
                    var start = info.ApplicableToRange.GetStartPoint(snapshot);
                    var end = info.ApplicableToRange.GetEndPoint(snapshot);
                    return new Hover {
                        Contents = content,
                        Range = buffer.ToLineRange(start, end)
                    };
                }
            }
            return new Hover();
        }

        private int ComputeActiveParameter(IRIntellisenseContext context, ISignatureInfo signatureInfo) {
            var settings = _services.GetService<IREditorSettings>();
            var parameterIndex = signatureInfo.ComputeCurrentParameter(context.EditorBuffer.CurrentSnapshot, context.AstRoot, context.Position, settings);
            if (parameterIndex < signatureInfo.Arguments.Count) {
                return parameterIndex;
            }
            return signatureInfo.Arguments.Count > 0 ? signatureInfo.Arguments.Count - 1 : 0;
        }
    }
}
