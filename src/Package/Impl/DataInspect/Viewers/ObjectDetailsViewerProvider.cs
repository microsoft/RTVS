// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    [Export(typeof(IObjectViewer))]
    public sealed class ObjectDetailsViewerProvider : IObjectViewer {
        private readonly IObjectDetailsViewerAggregator _aggregator;
        private readonly IDataObjectEvaluator _evaluator;

        [ImportingConstructor]
        public ObjectDetailsViewerProvider(IObjectDetailsViewerAggregator aggregator, IDataObjectEvaluator evaluator) {
            _aggregator = aggregator;
            _evaluator = evaluator;
        }

        public async Task ViewObjectDetails(string expression, string title) {
            var viewer = await _aggregator.GetViewer(expression);
            if (viewer != null) {
                await viewer?.ViewAsync(expression, title);
            }
        }

        public async Task ViewFile(string fileName, string tabName, bool deleteFile) {
            await VsAppShell.Current.SwitchToMainThreadAsync();

            FileViewer.ViewFile(fileName, tabName);
            try {
                if (deleteFile) {
                    File.Delete(fileName);
                }
            } catch (IOException) { } catch (AccessViolationException) { }
        }
    }
}
