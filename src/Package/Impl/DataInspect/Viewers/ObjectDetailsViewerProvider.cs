// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    [Export(typeof(IObjectViewer))]
    public sealed class ObjectDetailsViewerProvider : IObjectViewer {
        private readonly IObjectDetailsViewerAggregator _aggregator;

        [ImportingConstructor]
        public ObjectDetailsViewerProvider(IObjectDetailsViewerAggregator aggregator) {
            _aggregator = aggregator;
        }

        public async Task ViewObjectDetails(IRSession session, string environmentExpression, string expression, string title, CancellationToken cancellationToken) {
            var viewer = await _aggregator.GetViewer(session, environmentExpression, expression, cancellationToken);
            if (viewer != null) {
                await viewer.ViewAsync(expression, title, cancellationToken);
            }
        }

        public async Task ViewFile(string fileName, string tabName, bool deleteFile, CancellationToken cancellationToken) {
            await VsAppShell.Current.SwitchToMainThreadAsync(cancellationToken);

            FileViewer.ViewFile(fileName, tabName);
            try {
                if (deleteFile) {
                    File.Delete(fileName);
                }
            } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
    }
}
