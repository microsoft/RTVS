// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Host.Client;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    [Export(typeof(IObjectViewer))]
    public sealed class ObjectDetailsViewerProvider : IObjectViewer {
        private readonly IObjectDetailsViewerAggregator _aggregator;
        private readonly ICoreShell _coreShell;

        [ImportingConstructor]
        public ObjectDetailsViewerProvider(IObjectDetailsViewerAggregator aggregator, ICoreShell coreShell) {
            _aggregator = aggregator;
            _coreShell = coreShell;
        }

        public async Task ViewObjectDetails(IRSession session, string environmentExpression, string expression, string title, CancellationToken cancellationToken) {
            var viewer = await _aggregator.GetViewer(session, environmentExpression, expression, cancellationToken);
            if (viewer != null) {
                await viewer.ViewAsync(expression, title, cancellationToken);
            }
        }

        public async Task ViewFile(string fileName, string tabName, bool deleteFile, CancellationToken cancellationToken) {
            await _coreShell.SwitchToMainThreadAsync(cancellationToken);

            try {
                if (File.Exists(fileName)) {
                    FileViewer.ViewFile(fileName, tabName);
                    if (deleteFile) {
                        File.Delete(fileName);
                    }
                }
            } catch ( Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) {
                _coreShell.ShowErrorMessage(ex.Message);
            }
        }
    }
}
