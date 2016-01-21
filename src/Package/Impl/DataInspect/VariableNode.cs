using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Editor.Data;

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
                        result.Add(new VariableNode(child as EvaluationWrapper));
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


        public bool HasChildren {
            get {
                return _evaluation.HasChildren;
            }
        }

        public static int Comparison(VariableNode left, VariableNode right, ListSortDirection sortDirection) {
            int leftIndex = left._evaluation.Index;
            int rightIndex = right._evaluation.Index;

            Debug.Assert(leftIndex >= -1 && rightIndex >= -1);

            // Regardless to sortDirection, special index -1 is larger than anything. So, put at the end always
            if (leftIndex == -1 || rightIndex == -1) {
                return rightIndex - leftIndex;
            }

            if (sortDirection == ListSortDirection.Ascending) {
                return leftIndex - rightIndex;
            } else {
                return rightIndex - leftIndex;
            }
        }

        #endregion ITreeNode support
    }
}
