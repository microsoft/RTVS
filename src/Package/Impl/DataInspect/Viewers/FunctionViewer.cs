// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.Common.Core;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Debugger;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Editor.Signatures;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    [Export(typeof(IObjectDetailsViewer))]
    internal sealed class FunctionViewer : IObjectDetailsViewer {
        private readonly IRSessionProvider _sessionProvider;

        [ImportingConstructor]
        public FunctionViewer(IRSessionProvider sessionProvider) {
            _sessionProvider = sessionProvider;
        }

        #region IObjectDetailsViewer
        public bool IsTable => false;

        public bool CanView(DebugValueEvaluationResult evaluation) {
            return evaluation != null && evaluation.Classes.Count == 1 && evaluation.Classes[0].EqualsOrdinal("function");
        }

        public async Task ViewAsync(DebugValueEvaluationResult evaluation) {
            string functionName = evaluation.Name as string;
            if (string.IsNullOrEmpty(functionName)) {
                return;
            }

            var session = _sessionProvider.GetInteractiveWindowRSession();
            string functionCode = null;
            using (var e = await session.BeginEvaluationAsync()) {
                var result = await e.EvaluateAsync(functionName, REvaluationKind.Json);
                if (result.ParseStatus == RParseStatus.OK && result.Error == null && result.JsonResult != null) {
                    try {
                        functionCode = (string)result.JsonResult;
                    } catch (InvalidCastException) { }
                }
            }

            if (!string.IsNullOrEmpty(functionCode)) {
                var formatter = new RFormatter(REditorSettings.FormatOptions);
                functionCode = formatter.Format(functionCode);

                string fileName = "~" + functionName;
                string tempFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(fileName, ".r"));
                try {
                    if (File.Exists(tempFile)) {
                        File.Delete(tempFile);
                    }

                    using (var sw = new StreamWriter(tempFile)) {
                        sw.Write(functionCode);
                    }
                    var dte = VsAppShell.Current.GetGlobalService<DTE>();
                    dte.ItemOperations.OpenFile(tempFile);
                    File.Delete(tempFile);
                } catch (IOException) { } catch (AccessViolationException) { }
            }
        }

        public async Task<object> GetTooltipAsync(DebugValueEvaluationResult evaluation) {
            string functionName = evaluation.Name as string;
            if (string.IsNullOrEmpty(functionName)) {
                return Task.FromResult<object>(null);
            }

            var presenter = new FunctionInfoPresenter();
            presenter.DataContext = await FunctionSignatureSource.GetSignatureAsync(functionName);

            return Task.FromResult<object>(presenter);
        }
        #endregion
    }
}
