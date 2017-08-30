// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Core.Formatting;
using Microsoft.R.DataInspection;
using Microsoft.R.Editor;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    [Export(typeof(IObjectDetailsViewer))]
    internal sealed class CodeViewer : ViewerBase, IObjectDetailsViewer {
        private static readonly string[] _types = { "closure", "language" };
        private readonly IRInteractiveWorkflow _workflow;
        private readonly IFileSystem _fs;

        [ImportingConstructor]
        public CodeViewer(ICoreShell coreShell, IRInteractiveWorkflowProvider workflowProvider, IDataObjectEvaluator evaluator) :
            base(coreShell.Services, evaluator) {
            _workflow = workflowProvider.GetOrCreate();
            _fs = coreShell.Services.FileSystem();
        }

        #region IObjectDetailsViewer
        public ViewerCapabilities Capabilities => ViewerCapabilities.Function;

        public bool CanView(IRValueInfo evaluation) {
            return _types.Contains(evaluation?.TypeName);
        }

        public async Task ViewAsync(string expression, string title, CancellationToken cancellationToken = default(CancellationToken)) {
            var evaluation = await EvaluateAsync(expression, REvaluationResultProperties.ExpressionProperty, null, cancellationToken);
            if (string.IsNullOrEmpty(evaluation?.Expression)) {
                return;
            }

            var functionName = evaluation.Expression;
            var functionCode = await GetFunctionCode(functionName, cancellationToken);
            if (!string.IsNullOrEmpty(functionCode)) {

                var tempFile = Path.ChangeExtension(Path.GetTempFileName(), ".r");
                try {
                    if (_fs.FileExists(tempFile)) {
                        _fs.DeleteFile(tempFile);
                    }

                    using (var sw = new StreamWriter(tempFile)) {
                        sw.Write(functionCode);
                    }

                    await _workflow.Shell.SwitchToMainThreadAsync(cancellationToken);

                    FileViewer.ViewFile(tempFile, functionName);
                    try {
                        _fs.DeleteFile(tempFile);
                    } catch (IOException) { } catch (UnauthorizedAccessException) { }

                } catch (IOException) { } catch (UnauthorizedAccessException) { }
            }
        }
        #endregion

        internal async Task<string> GetFunctionCode(string functionName, CancellationToken cancellationToken = default(CancellationToken)) {
            var session = _workflow.RSession;
            string functionCode = null;
            try {
                functionCode = await session.GetFunctionCodeAsync(functionName, cancellationToken);
            } catch (RException) { } catch (OperationCanceledException) { }

            if (!string.IsNullOrEmpty(functionCode)) {
                var formatter = new RFormatter(_workflow.Shell.GetService<IREditorSettings>().FormatOptions);
                functionCode = formatter.Format(functionCode);
            }
            return functionCode;
        }
    }
}
