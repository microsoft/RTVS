// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.R.Components.Extensions;
using Microsoft.R.DataInspection;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
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

        abstract public bool CanView(IRValueInfo evaluation);

        public async Task ViewAsync(string expression, string title) {
            var evaluation = await EvaluateAsync(expression, _properties, RValueRepresentations.Str()) as IRValueInfo;
            if (evaluation != null) {
                await VsAppShell.Current.SwitchToMainThreadAsync();

                var id = _toolWindowIdBase + evaluation.GetHashCode() % (Int32.MaxValue - _toolWindowIdBase);
                VariableGridWindowPane pane = ToolWindowUtilities.ShowWindowPane<VariableGridWindowPane>(id, true);
                title = !string.IsNullOrEmpty(title) ? title : evaluation.Expression;
                pane.SetEvaluation(new VariableViewModel(evaluation, _aggregator), title);
            }
        }
        #endregion
    }
}
