// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Common.Core;
using Microsoft.R.Debugger;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings.Definitions;
using Microsoft.VisualStudio.R.Package.DataInspect.Definitions;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public partial class VariableView : UserControl, IDisposable {
        private readonly IRToolsSettings _settings;
        ObservableTreeNode _rootNode;
        IREnvironmentProvider _environmentProvider;
        IRSession _rSession;
        DebugSession _debugSession;

        private static List<REnvironment> _defaultEnvironments = new List<REnvironment>() { new REnvironment(Package.Resources.VariableExplorer_EnvironmentName) };

        public VariableView() : this(null) { }

        public VariableView(IRToolsSettings settings) {
            _settings = settings;

            InitializeComponent();

            SetRootNode(EvaluationWrapper.Ellipsis);
            EnvironmentComboBox.ItemsSource = _defaultEnvironments;
            EnvironmentComboBox.SelectedIndex = 0;

            SortDirection = ListSortDirection.Ascending;
            RootTreeGrid.Sorting += RootTreeGrid_Sorting;

            var sessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            _rSession = sessionProvider.GetInteractiveWindowRSession();

            _environmentProvider = new REnvironmentProvider(_rSession);
            _environmentProvider.EnvironmentChanged += EnvironmentProvider_EnvironmentChanged;
        }

        public void Dispose() {
            RootTreeGrid.Sorting -= RootTreeGrid_Sorting;

            if (_environmentProvider != null) {
                _environmentProvider.EnvironmentChanged -= EnvironmentProvider_EnvironmentChanged;
                _environmentProvider = null;
            }
        }

        private void RootTreeGrid_Sorting(object sender, DataGridSortingEventArgs e) {
            // SortDirection
            if (SortDirection == ListSortDirection.Ascending) {
                SortDirection = ListSortDirection.Descending;
            } else {
                SortDirection = ListSortDirection.Ascending;
            }

            _rootNode.Sort();
            e.Handled = true;
        }

        private void EnvironmentProvider_EnvironmentChanged(object sender, REnvironmentChangedEventArgs e) {
            int selectedIndex = 0;

            var currentItem = EnvironmentComboBox.SelectedItem as REnvironment;
            if (currentItem != null && !e.Environments[0].FrameIndex.HasValue) {
                for (int i = 1; i < e.Environments.Count; i++) {
                    if (e.Environments[i].Name == currentItem.Name) {
                        selectedIndex = i;
                        break;
                    }
                }
            }

            EnvironmentComboBox.ItemsSource = e.Environments;
            EnvironmentComboBox.SelectedIndex = selectedIndex;
        }

        private void EnvironmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if ((EnvironmentComboBox.ItemsSource != _defaultEnvironments) && (e.AddedItems.Count > 0)) {
                var env = e.AddedItems[0] as REnvironment;
                if (env != null) {
                    SetRootModelAsync(env).DoNotWait();
                }
            }
        }

        private async Task<DebugSession> GetDebugSessionAsync() {
            if (_debugSession != null) {
                return _debugSession;
            }

            var debugSessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IDebugSessionProvider>();
            if (_debugSession == null) {
                Debug.Assert(_rSession != null);
                _debugSession = await debugSessionProvider.GetDebugSessionAsync(_rSession);
            }

            return _debugSession;
        }

        private async Task SetRootModelAsync(REnvironment env) {
            await TaskUtilities.SwitchToBackgroundThread();

            var debugSession = await GetDebugSessionAsync();
            var frames = await debugSession.GetStackFramesAsync();
            var frame = frames.FirstOrDefault(f => f.Index == 0);

            if (frame != null) {
                const DebugEvaluationResultFields fields = DebugEvaluationResultFields.Classes
                        | DebugEvaluationResultFields.Expression
                        | DebugEvaluationResultFields.TypeName
                        | (DebugEvaluationResultFields.Repr | DebugEvaluationResultFields.ReprStr)
                        | DebugEvaluationResultFields.Dim
                        | DebugEvaluationResultFields.Length;
                DebugEvaluationResult result = await frame.EvaluateAsync(
                    GetExpression(env),
                    fields);

                var wrapper = new EvaluationWrapper(result);
                var rootNodeModel = new VariableNode(_settings, wrapper);

                VsAppShell.Current.DispatchOnUIThread(
                    () => {
                        _rootNode.Model = rootNodeModel;
                    });
            }
        }

        private string GetExpression(REnvironment env) {
            if (env.FrameIndex.HasValue) {
                return Invariant($"sys.frame({env.FrameIndex.Value})");
            } else {
                return Invariant($"as.environment({env.Name.ToRStringLiteral()})");
            }
        }

        private void SetRootNode(EvaluationWrapper evaluation) {
            _rootNode = new ObservableTreeNode(
                new VariableNode(_settings, evaluation),
                Comparer<ITreeNode>.Create(Comparison));

            RootTreeGrid.ItemsSource = new TreeNodeCollection(_rootNode).ItemList;
        }

        private ListSortDirection SortDirection { get; set; }

        private int Comparison(ITreeNode left, ITreeNode right) {
            return VariableNode.Comparison((VariableNode)left, (VariableNode)right, SortDirection);
        }
    }
}
