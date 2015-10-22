using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Model adapter to <see cref="ObservableTreeNode"/>
    /// </summary>
    internal class VariableNode : ITreeNode {

        #region member/ctor

        private EvaluationWrapper _evaluation;
        public VariableNode(EvaluationWrapper evaluation) {
            _evaluation = evaluation;
        }

        #endregion

        #region ITreeNode support

        public object Content {
            get {
                return _evaluation;
            }
            set {
                _evaluation = (EvaluationWrapper)value;
            }
        }

        public async Task<IReadOnlyList<ITreeNode>> GetChildrenAsync(CancellationToken cancellationToken) {
            List<ITreeNode> result = null;
            var children = await _evaluation.GetChildrenAsync();
            if (children != null) {
                result = new List<ITreeNode>();

                foreach (var child in children) {
                    if (!child.IsHidden) {
                        result.Add(new VariableNode(child));
                    }
                }
            }
            return result;
        }

        public bool CanUpdateTo(ITreeNode node) {
            var value = node.Content as EvaluationWrapper;
            if (value != null) {
                return _evaluation.Name == value.Name;
            }

            return false;
        }

        #endregion ITreeNode support
    }
}
