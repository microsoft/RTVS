using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.Common.Core;
using Microsoft.R.Debugger;
using Microsoft.R.Editor.Data;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.R.Package.Utilities;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Model for variable tree grid, that provides UI customization of <see cref="DebugEvaluationResult"/>
    /// </summary>
    internal sealed class EvaluationWrapper : RSessionDataObject, IIndexedItem {
        public EvaluationWrapper() { }

        public EvaluationWrapper(int index, DebugEvaluationResult evaluation) :
            this(index, evaluation, true) { }

        /// <summary>
        /// Create new instance of <see cref="EvaluationWrapper"/>
        /// </summary>
        /// <param name="evaluation">R session's evaluation result</param>
        /// <param name="truncateChildren">true to truncate children returned by GetChildrenAsync</param>
        public EvaluationWrapper(int index, DebugEvaluationResult evaluation, bool truncateChildren) :
            base(index, evaluation, truncateChildren) {
            if (CanShowDetail) {
                ShowDetailCommand = new DelegateCommand(ShowVariableGridWindowPane, (o) => CanShowDetail);
            }
        }

        private static Lazy<EvaluationWrapper> _ellipsis = Lazy.Create(() => {
            var instance = new EvaluationWrapper();
            instance.Name = string.Empty;
            instance.Value = "[truncated]";
            instance.HasChildren = false;
            return instance;
        });
        public static EvaluationWrapper Ellipsis {
            get { return _ellipsis.Value; }
        }

        protected override List<IRSessionDataObject> EvaluateChildren(IReadOnlyList<DebugEvaluationResult> children, bool truncateChildren) {
            var result = new List<IRSessionDataObject>();
            for (int i = 0; i < children.Count; i++) {
                result.Add(new EvaluationWrapper(i, children[i], truncateChildren));
            }
            return result;
        }

        #region Detail Grid
        public ICommand ShowDetailCommand { get; }

        private void ShowVariableGridWindowPane(object parameter) {
            VariableGridWindowPane pane = ToolWindowUtilities.ShowWindowPane<VariableGridWindowPane>(0, true);
            pane.SetEvaluation(this);
        }
        #endregion
    }
}
