// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.Common.Core.Shell;
using Microsoft.R.DataInspection;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Shell.Interop;
using static Microsoft.R.DataInspection.REvaluationResultProperties;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    internal abstract class GridViewerBase : ViewerBase, IObjectDetailsViewer {
        private const int _toolWindowIdBase = 100;
        private const REvaluationResultProperties _properties =
           ClassesProperty | ExpressionProperty | TypeNameProperty | DimProperty | LengthProperty;

        private readonly IObjectDetailsViewerAggregator _aggregator;

        public GridViewerBase(IObjectDetailsViewerAggregator aggregator, IDataObjectEvaluator evaluator) :
            base(evaluator) {
            _aggregator = aggregator;
        }

        #region IObjectDetailsViewer
        public ViewerCapabilities Capabilities => ViewerCapabilities.List | ViewerCapabilities.Table;

        public abstract bool CanView(IRValueInfo evaluation);

        public async Task ViewAsync(string expression, string title, CancellationToken cancellationToken = default(CancellationToken)) {
            var evaluation = await EvaluateAsync(expression, _properties, RValueRepresentations.Str(), cancellationToken);
            if (evaluation != null) {
                await VsAppShell.Current.SwitchToMainThreadAsync(cancellationToken);
                var id = Math.Abs(_toolWindowIdBase + expression.GetHashCode() % (Int32.MaxValue - _toolWindowIdBase));

                var pane = ToolWindowUtilities.FindWindowPane<VariableGridWindowPane>(id);
                if (pane == null) {
                    pane = ToolWindowUtilities.ShowWindowPane<VariableGridWindowPane>(id, true);
                } else {
                    var frame = pane.Frame as IVsWindowFrame;
                    Debug.Assert(frame != null);
                    frame?.Show();
                }

                title = !string.IsNullOrEmpty(title) ? title : evaluation.Expression;
                pane.SetEvaluation(new VariableViewModel(evaluation, _aggregator), title);
            }
        }
        #endregion
    }
}
