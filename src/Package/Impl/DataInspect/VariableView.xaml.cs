// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Wpf.Extensions;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.DataInspection;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.StackTracing;
using Microsoft.R.Support.Settings;
using Microsoft.R.Wpf.Themes;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Commands.R;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using static System.FormattableString;
using static Microsoft.R.DataInspection.REvaluationResultProperties;
using Brushes = Microsoft.R.Wpf.Brushes;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public partial class VariableView : UserControl, ICommandTarget, IDisposable {
        private readonly IRToolsSettings _settings;
        private readonly ICoreShell _shell;
        private readonly IRSession _session;
        private readonly IREnvironmentProvider _environmentProvider;
        private readonly IObjectDetailsViewerAggregator _aggregator;

        private bool _keyDownSeen;
        private ObservableTreeNode _rootNode;

        public VariableView() : this(null, VsAppShell.Current) { }

        public VariableView(IRToolsSettings settings, ICoreShell shell) {
            _settings = settings;
            _shell = shell;

            InitializeComponent();

            _aggregator = _shell.ExportProvider.GetExportedValue<IObjectDetailsViewerAggregator>();

            SetRootNode(VariableViewModel.Ellipsis);

            SortDirection = ListSortDirection.Ascending;
            RootTreeGrid.Sorting += RootTreeGrid_Sorting;

            var theme = shell.ExportProvider.GetExportedValue<IThemeUtilities>();
            theme.SetImageBackgroundColor(RootTreeGrid, Brushes.ToolWindowBackgroundKey);
            theme.SetThemeScrollBars(RootTreeGrid);

            var workflow = VsAppShell.Current.ExportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate();
            _session = workflow.RSession;

            _environmentProvider = new REnvironmentProvider(_session);
            EnvironmentComboBox.DataContext = _environmentProvider;
            _environmentProvider.RefreshEnvironmentsAsync().DoNotWait();
        }

        public void Dispose() {
            RootTreeGrid.Sorting -= RootTreeGrid_Sorting;
            _environmentProvider?.Dispose();
        }

        public bool IsGlobalREnvironment() {
            var env = EnvironmentComboBox.SelectedValue as REnvironment;
            return env?.Kind == REnvironmentKind.Global;
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

        private void EnvironmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var env = e.AddedItems.OfType<REnvironment>().FirstOrDefault();
            if (env != null) {
                SetRootModelAsync(env).DoNotWait();
            }
        }

        private async Task SetRootModelAsync(REnvironment env) {
            _shell.AssertIsOnMainThread();

            if (env.Kind != REnvironmentKind.Error) {
                try {
                    var result = await EvaluateAndDescribeAsync(env);
                    var wrapper = new VariableViewModel(result, _aggregator);
                    _rootNode.Model = new VariableNode(_settings, wrapper);
                } catch (RException ex) {
                    SetRootNode(VariableViewModel.Error(ex.Message));
                } catch (RHostDisconnectedException ex) {
                    SetRootNode(VariableViewModel.Error(ex.Message));
                }
            } else {
                SetRootNode(VariableViewModel.Error(env.Name));
            }

            // Some of the Variable Explorer tool bar buttons are depend on the R Environment (e.g., Delete all Variables button).
            // This will give those UI elements a chance to update state.
            _shell.UpdateCommandStatus();
        }

        private async Task<IRValueInfo> EvaluateAndDescribeAsync(REnvironment env) {
            await TaskUtilities.SwitchToBackgroundThread();

            const REvaluationResultProperties properties = ClassesProperty | ExpressionProperty | TypeNameProperty | DimProperty | LengthProperty;
            return await _session.EvaluateAndDescribeAsync(env.EnvironmentExpression, properties, null);
        }

        private void SetRootNode(VariableViewModel evaluation) {
            _rootNode = new ObservableTreeNode(
                new VariableNode(_settings, evaluation),
                Comparer<ITreeNode>.Create(Comparison));

            RootTreeGrid.ItemsSource = new TreeNodeCollection(_rootNode).ItemList;
        }

        private ListSortDirection SortDirection { get; set; }

        private int Comparison(ITreeNode left, ITreeNode right) {
            return VariableNode.Comparison((VariableNode)left, (VariableNode)right, SortDirection);
        }

        private void GridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            HandleDefaultAction();
        }

        private void GridRow_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            var row = sender as DataGridRow;
            if (row != null) {
                SelectRow(row);
                var pt = PointToScreen(e.GetPosition(this));
                _shell.ShowContextMenu(new CommandID(RGuidList.RCmdSetGuid, (int)RContextMenuId.VariableExplorer), (int)pt.X, (int)pt.Y, this);
                e.Handled = true;
            }
        }

        private void SelectRow(DataGridRow row) {
            RootTreeGrid.SelectedItem = row;
            row.IsSelected = true;
            var presenter = VisualTreeExtensions.FindFirstVisualChildOfType<DataGridCellsPresenter>(row);
            var cell = presenter.ItemContainerGenerator.ContainerFromIndex(0) as DataGridCell;
            cell.Focus();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e) {
            switch (e.Key) {
                case Key.Enter:
                    // Track that we've seen key down here so when key up
                    // comes we can tell if it is a real one or a leftover
                    // notification from the just closed context menu when
                    // user hit Enter to execute command in the context menu.
                    _keyDownSeen = true;
                    e.Handled = true;
                    return;
                case Key.System:
                    if (e.SystemKey == Key.F10 && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {
                        ShowContextMenu();
                    }
                    break;
                case Key.C:
                    // Actual binding theorerically can be fetched from DTE.
                    // However, DTE gives binding string which needs to be parsed
                    // and since Copy rarely changes from Ctrl+C we'll leave it alone for now.
                    if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
                        CopyEntry(GetCurrentSelectedModel());
                        e.Handled = true;
                        return;
                    }
                    break;
            }
            base.OnPreviewKeyDown(e);
        }

        private void ShowContextMenu() {
            var focus = Keyboard.FocusedElement as FrameworkElement;
            if (focus != null) {
                var pt = focus.PointToScreen(new Point(1, 1));
                _shell.ShowContextMenu(
                    new CommandID(RGuidList.RCmdSetGuid, (int)RContextMenuId.VariableExplorer), (int)pt.X, (int)pt.Y, this);
            }
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e) {
            // Prevent Enter from being passed to WPF control
            // when user hits it in the context menu
            if (e.Key == Key.Enter && _keyDownSeen) {
                HandleDefaultAction();
                e.Handled = true;
                _keyDownSeen = false;
                return;
            } else if (e.Key == Key.Delete || e.Key == Key.Back) {
                DeleteCurrentVariableAsync().DoNotWait();
            } else if (e.Key == Key.Apps) {
                ShowContextMenu();
            }
            base.OnPreviewKeyUp(e);
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            // Suppress Enter navigation
            if (e.Key == Key.Enter) {
                return;
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                return;
            }
            base.OnKeyUp(e);
        }

        private void HandleDefaultAction() {
            var model = GetCurrentSelectedModel();
            if (model != null && model.CanShowDetail) {
                model.ShowDetailCommand.Execute(model);
            }
        }

        private Task DeleteCurrentVariableAsync() {
            var env = EnvironmentComboBox.SelectedItem as REnvironment;
            var model = GetCurrentSelectedModel();
            return model != null ? model.DeleteAsync(env?.EnvironmentExpression) : Task.CompletedTask;
        }

        #region Icons
        private ImageMoniker GetVariableIcon(IREvaluationResultInfo info) {
            if (info is IRActiveBindingInfo) {
                return KnownMonikers.Property;
            } else if (info is IRPromiseInfo) {
                return KnownMonikers.Delegate;
            } else if (info is IRErrorInfo) {
                return KnownMonikers.StatusInvalid;
            }

            var value = info as IRValueInfo;
            if (value != null) {
                // Order of checks here is important, as some categories are subsets of others, and hence have to be checked first.
                // For example, all dataframes are also lists, and so we need to check for class "data.frame", and supply an icon
                // for it, before we check for type "list".
                if (value.TypeName == "S4") {
                    return KnownMonikers.Class;
                } else if (value.Classes.Contains("refObjectGenerator")) {
                    return KnownMonikers.NewClass;
                } else if (value.TypeName == "closure" || value.TypeName == "builtin") {
                    return KnownMonikers.Procedure;
                } else if (value.Classes.Contains("formula")) {
                    return KnownMonikers.MemberFormula;
                } else if (value.TypeName == "symbol" || value.TypeName == "language" || value.TypeName == "expression") {
                    return KnownMonikers.Code;
                } else if (value.Classes.Contains("data.frame")) {
                    return KnownMonikers.Table;
                } else if (value.Classes.Contains("matrix")) {
                    return KnownMonikers.Matrix;
                } else if (value.TypeName == "environment") {
                    return KnownMonikers.BulletList;
                } else if (value.TypeName == "list" || (value.IsAtomic() && value.Length > 1)) {
                    return KnownMonikers.OrderedList;
                } else {
                    return KnownMonikers.BinaryRegistryValue;
                }
            }

            return KnownMonikers.UnknownMember;
        }

        private ImageMoniker GetEnvironmentIcon(REnvironmentKind kind) {
            switch (kind) {
                case REnvironmentKind.Global:
                    return KnownMonikers.GlobalVariable;
                case REnvironmentKind.Function:
                    return KnownMonikers.Procedure;
                case REnvironmentKind.Package:
                    return KnownMonikers.Package;
                case REnvironmentKind.Error:
                    return KnownMonikers.StatusInvalid;
                default:
                    return KnownMonikers.BulletList;
            }
        }
        #endregion

        #region ICommandTarget
        public CommandStatus Status(Guid group, int id) {
            if (group == VSConstants.GUID_VSStandardCommandSet97) {
                switch ((VSConstants.VSStd97CmdID)id) {
                    case VSConstants.VSStd97CmdID.Copy:
                    case VSConstants.VSStd97CmdID.Delete:
                        return CommandStatus.SupportedAndEnabled;
                }
            } else if (group == RGuidList.RCmdSetGuid) {
                var model = GetCurrentSelectedModel();
                if (model != null) {
                    switch (id) {
                        case (int)RContextMenuId.VariableExplorer:
                        case RPackageCommandId.icmdCopyValue:
                            return CommandStatus.SupportedAndEnabled;
                        case RPackageCommandId.icmdShowDetails:
                            return model.CanShowDetail ? CommandStatus.SupportedAndEnabled : CommandStatus.Invisible;
                        case RPackageCommandId.icmdOpenInCsvApp:
                            return model.CanShowOpenCsv ? CommandStatus.SupportedAndEnabled : CommandStatus.Invisible;
                    }
                }
            }
            return CommandStatus.Invisible;
        }

        public CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            var model = GetCurrentSelectedModel();
            if (model != null) {
                if (group == VSConstants.GUID_VSStandardCommandSet97) {
                    switch ((VSConstants.VSStd97CmdID)id) {
                        case VSConstants.VSStd97CmdID.Copy:
                            CopyEntry(model);
                            break;
                        case VSConstants.VSStd97CmdID.Delete:
                            DeleteCurrentVariableAsync().DoNotWait();
                            break;
                    }
                } else if (group == RGuidList.RCmdSetGuid) {
                    switch (id) {
                        case RPackageCommandId.icmdCopyValue:
                            CopyValue(model);
                            break;
                        case RPackageCommandId.icmdShowDetails:
                            model?.ShowDetailCommand.Execute(model);
                            break;
                        case RPackageCommandId.icmdOpenInCsvApp:
                            model?.OpenInCsvAppCommand.Execute(model);
                            break;
                    }
                }
            }
            return CommandResult.Executed;
        }

        public void PostProcessInvoke(CommandResult result, Guid group, int id, object inputArg, ref object outputArg) { }
        #endregion

        private VariableViewModel GetCurrentSelectedModel() {
            var selection = RootTreeGrid?.SelectedItem as ObservableTreeNode;
            return selection?.Model?.Content as VariableViewModel;
        }

        private void CopyEntry(VariableViewModel model) {
            if (model != null) {
                string data = Invariant($"{model.Name} {model.Value} {model.Class} {model.TypeName}");
                SetClipboardData(data);
            }
        }

        private void CopyValue(VariableViewModel model) {
            if (model != null) {
                SetClipboardData(model.Value);
            }
        }
        private void SetClipboardData(string text) {
            Clipboard.Clear();
            Clipboard.SetText(text);
        }
    }
}
