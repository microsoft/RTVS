using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.VisualStudio.R.Package.Utilities;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    class VariableNode : ITreeNode {
        EvaluationWrapper _evaluation;
        public VariableNode(EvaluationWrapper evaluation) {
            _evaluation = evaluation;
        }

        public object Content {
            get {
                return _evaluation;
            }
            set {
                _evaluation = (EvaluationWrapper)value;
            }
        }

        public async Task<IList<ITreeNode>> GetChildrenAsync(CancellationToken cancellationToken) {
            List<ITreeNode> result = null;
            var children = await _evaluation.GetChildrenAsync();
            if (children != null) {
                result = new List<ITreeNode>();
                var visibleChildren = children.Where(c => !c.IsHidden);
                foreach (var child in visibleChildren) {
                    result.Add(new VariableNode(child));
                }
            }
            return result;
        }

        public bool IsSame(ITreeNode node) {
            var value = node.Content as EvaluationWrapper;
            if (value != null) {
                return _evaluation.IsSameEvaluation(value);
            }

            return false;
        }
    }
}
