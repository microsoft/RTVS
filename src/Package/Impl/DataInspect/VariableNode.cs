// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Components.Settings;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Model adapter to <see cref="ObservableTreeNode"/>
    /// </summary>
    internal class VariableNode : ITreeNode {
        #region member/ctor

        private readonly IRSettings _settings;
        private VariableViewModel _evaluation;

        public VariableNode(IRSettings settings, VariableViewModel evaluation) {
            _settings = settings;
            _evaluation = evaluation;
        }

        #endregion

        #region ITreeNode support

        public object Content {
            get {
                return _evaluation;
            }
            set {
                _evaluation = (VariableViewModel)value;
            }
        }

        public async Task<IReadOnlyList<ITreeNode>> GetChildrenAsync(CancellationToken cancellationToken) {
            List<ITreeNode> result = null;
            var children = await _evaluation.GetChildrenAsync();
            if (children != null) {
                result = new List<ITreeNode>();

                foreach (var child in children) {
                    if ((_settings != null && _settings.ShowDotPrefixedVariables) || !child.IsHidden) {
                        result.Add(new VariableNode(_settings, child as VariableViewModel));
                    }
                }
            }
            return result;
        }

        public bool CanUpdateTo(ITreeNode node) {
            var value = node.Content as VariableViewModel;
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
