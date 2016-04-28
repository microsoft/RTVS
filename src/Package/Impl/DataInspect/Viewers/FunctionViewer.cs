// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.Common.Core;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Debugger;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    [Export(typeof(IObjectDetailsViewer))]
    internal sealed class FunctionViewer : ViewerBase, IObjectDetailsViewer {
        private readonly IRSessionProvider _sessionProvider;

        [ImportingConstructor]
        public FunctionViewer(IRSessionProvider sessionProvider, IDataObjectEvaluator evaluator) :
            base(evaluator) {
            _sessionProvider = sessionProvider;
        }

        #region IObjectDetailsViewer
        public ViewerCapabilities Capabilities => ViewerCapabilities.Function;

        public bool CanView(IDebugValueEvaluationResult evaluation) {
            return evaluation != null && evaluation.Classes.Count == 1 && evaluation.Classes[0].EqualsOrdinal("function");
        }

        public async Task ViewAsync(string expression, string title) {
            var evaluation = await EvaluateAsync(expression, DebugEvaluationResultFields.Expression, null);
            if (evaluation == null || string.IsNullOrEmpty(evaluation.Expression)) {
                return;
            }

            var functionName = evaluation.Expression;
            var session = _sessionProvider.GetInteractiveWindowRSession();

            string functionCode = await session.EvaluateAsync<string>(Invariant($"paste0(deparse({functionName}), collapse='\n')"), REvaluationKind.Normal);
            if (!string.IsNullOrEmpty(functionCode)) {
                var formatter = new RFormatter(REditorSettings.FormatOptions);
                functionCode = formatter.Format(functionCode);

                string name = (!string.IsNullOrEmpty(title) && title.IndexOfAny(Path.GetInvalidFileNameChars()) < 0) ? title : functionName;
                string fileName = "~" + name;
                string tempFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(fileName, ".r"));
                try {
                    if (File.Exists(tempFile)) {
                        File.Delete(tempFile);
                    }

                    using (var sw = new StreamWriter(tempFile)) {
                        sw.Write(functionCode);
                    }

                    await VsAppShell.Current.SwitchToMainThreadAsync();

                    var dte = VsAppShell.Current.GetGlobalService<DTE>();
                    dte.ItemOperations.OpenFile(tempFile);
                    try {
                        File.Delete(tempFile);
                    } catch (IOException) { } catch (AccessViolationException) { }

                } catch (IOException) { } catch (AccessViolationException) { }
            }
        }
        #endregion
    }
}
