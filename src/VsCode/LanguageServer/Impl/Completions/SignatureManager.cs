// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageServer.VsCode.Contracts;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Editor.QuickInfo;
using Microsoft.R.Editor.Signatures;
using Microsoft.R.LanguageServer.Extensions;

namespace Microsoft.R.LanguageServer.Completions {
    internal sealed class SignatureManager {
        private readonly RFunctionSignatureEngine _signatureEngine;
        private readonly IMainThread _mainThread;

        public SignatureManager(IServiceContainer services) {
            _signatureEngine = new RFunctionSignatureEngine(services);
            _mainThread = services.MainThread();
        }

        public async Task<IList<SignatureInformation>> GetSignaturesAsync(IRIntellisenseContext context) {
            var tcs = new TaskCompletionSource<IList<SignatureInformation>>();
            var sigs = _signatureEngine.GetSignaturesAsync(context, e => tcs.TrySetResult(ToSignatureInformation(e)));
            if(sigs != null) {
                return ToSignatureInformation(sigs);
            }
            return await tcs.Task;
        }

        public async Task<Hover> GetHoverAsync(IRIntellisenseContext context, CancellationToken ct) {
            var tcs = new TaskCompletionSource<Hover>();
            using (context.AstReadLock()) {
                var infos = _signatureEngine.GetQuickInfosAsync(context, e => {
                    if (!ct.IsCancellationRequested) {
                        tcs.TrySetResult(ToHover(e.ToList(), context.EditorBuffer));
                    } else {
                        tcs.TrySetCanceled();
                    }
                });

                if (infos != null) {
                    return ToHover(infos.ToList(), context.EditorBuffer);
                }
                return await tcs.Task;
            }
        }

        private static IList<SignatureInformation> ToSignatureInformation(IEnumerable<IRFunctionSignatureHelp> signatures)
            => signatures.Select(s => new SignatureInformation {
                Label = s.Content,
                Documentation = s.Documentation,
                Parameters = s.Parameters.Select(p => new ParameterInformation {
                    Label = p.Name,
                    Documentation = p.Documentation
                }).ToList()
            }).ToList();

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
            return null;
        }
    }
}
