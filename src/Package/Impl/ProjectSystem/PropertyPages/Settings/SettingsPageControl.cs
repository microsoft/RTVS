// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Application.Configuration;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.PropertyPages.Settings {
    internal partial class SettingsPageControl : UserControl {
        private readonly SettingsPageViewModel _viewModel;
        private readonly ICoreShell _coreShell;
        private int _selectedIndex;
        private bool _isDirty;

        public SettingsPageControl() : this(
            VsAppShell.Current.ExportProvider.GetExportedValue<IProjectConfigurationSettingsProvider>(), 
            VsAppShell.Current, new FileSystem()) { }

        public SettingsPageControl(IProjectConfigurationSettingsProvider settingsProvider, ICoreShell coreShell, IFileSystem fs) {
            _coreShell = coreShell;
            _viewModel = new SettingsPageViewModel(settingsProvider, coreShell, fs);
            InitializeComponent();
        }

        public async Task SetProjectAsync(ConfiguredProject project) {
            await _viewModel.SetProjectAsync(project);
        }

        public event EventHandler<EventArgs> DirtyStateChanged;
        public bool IsDirty {
            get { return _isDirty; }
            set {
                if (_isDirty != value) {
                    _isDirty = value;
                    DirtyStateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public Task SaveSelectedSettingsFileNameAsync() {
            return _viewModel?.SaveSelectedSettingsFileNameAsync();
        }

        public async Task<bool> SaveSettingsAsync() {
            bool result = true;
            if (string.IsNullOrEmpty(_viewModel.CurrentFile)) {
                _viewModel.CreateNewSettingsFile();
                PopulateFilesCombo();
            }
            result = await _viewModel.SaveAsync();
            if (result) {
                IsDirty = false;
            }
            return result;
        }

        protected override void OnLoad(EventArgs e) {
            SetFont();
            SetTextToControls();

            PopulateFilesCombo();
            LoadPropertyGrid();
            SetButtonEnableState();

            filesList.SelectedIndexChanged += OnSelectedFileChanged;
            addSettingButton.Click += OnAddSettingClick;
            variableName.TextChanged += OnVariableNameTextChanged;
            variableValue.TextChanged += OnVariableValueTextChanged;
            propertyGrid.PropertyValueChanged += OnPropertyValueChanged;

            base.OnLoad(e);
        }

        private void SetTextToControls() {
            variableNameLabel.Text = Resources.SettingsPage_VariableNameLabel;
            variableValueLabel.Text = Resources.SettingsPage_VariableValueLabel;
            variableTypeLabel.Text = Resources.SettingsPage_VariableTypeLabel;

            variableTypeList.Items.Add(Resources.SettingsPage_VariableType_String);
            variableTypeList.Items.Add(Resources.SettingsPage_VariableType_Expression);
            variableTypeList.SelectedIndex = 0;

            explanationText.Text = string.Format(CultureInfo.InvariantCulture,
                                    Resources.SettingsPage_Explanation,
                                    Environment.NewLine + Environment.NewLine,
                                    Environment.NewLine + Environment.NewLine);
        }

        private void PopulateFilesCombo() {
            var files = _viewModel.Files.ToArray();
            filesList.Items.Clear();
            if (files.Length == 0) {
                filesList.Items.Add(Resources.NoSettingFiles);
                _selectedIndex = filesList.SelectedIndex = 0;
            } else {
                filesList.Items.AddRange(files);
                int index = Array.FindIndex(files, x => x.EqualsIgnoreCase(_viewModel.CurrentFile));
                _selectedIndex = filesList.SelectedIndex = index >= 0 ? index : 0;
            }
        }

        private void LoadPropertyGrid() {
            var item = filesList.Items[filesList.SelectedIndex] as string;
            if (item.EqualsOrdinal(Resources.NoSettingFiles)) {
                return;
            }
            _viewModel.CurrentFile = item;
            UpdatePropertyGrid();
        }

        private void UpdatePropertyGrid() {
            propertyGrid.SelectedObject = _viewModel.TypeDescriptor;
        }

        private void SetButtonEnableState() {
            addSettingButton.Enabled = !string.IsNullOrWhiteSpace(variableName.Text) && !string.IsNullOrWhiteSpace(variableValue.Text);
        }

        private void OnSelectedFileChanged(object sender, EventArgs e) {
            if (_selectedIndex != filesList.SelectedIndex) {
                if (IsDirty) {
                    var answer = _coreShell.ShowMessage(Resources.SettingsPage_SavePrompt, MessageButtons.YesNoCancel);
                    if (answer == MessageButtons.Cancel) {
                        filesList.SelectedIndex = _selectedIndex;
                        return;
                    } else if (answer == MessageButtons.Yes) {
                        _viewModel.SaveAsync().DoNotWait();
                    }
                }
                _selectedIndex = filesList.SelectedIndex;
                IsDirty = true;
                LoadPropertyGrid();
            }
        }

        private void OnAddSettingClick(object sender, EventArgs e) {
            _viewModel.AddSetting(variableName.Text, variableValue.Text,
                    ((string)variableTypeList.SelectedItem).EqualsOrdinal(Resources.SettingsPage_VariableType_String)
                        ? ConfigurationSettingValueType.String
                        : ConfigurationSettingValueType.Expression);
            UpdatePropertyGrid();
            IsDirty = true;
        }

        private void OnVariableNameTextChanged(object sender, EventArgs e) {
            SetButtonEnableState();
        }

        private void OnVariableValueTextChanged(object sender, EventArgs e) {
            SetButtonEnableState();
        }

        private void OnPropertyValueChanged(object s, PropertyValueChangedEventArgs e) {
            var setting = (e.ChangedItem.PropertyDescriptor as SettingPropertyDescriptor)?.Setting;
            if (setting != null && string.IsNullOrWhiteSpace(setting.Value)) {
                _viewModel.RemoveSetting(setting);
                UpdatePropertyGrid();
            }
            IsDirty = true;
        }

        private void SetFont() {
            // Set the correct font
            var fontSvc = VsAppShell.Current.GetGlobalService<IUIHostLocale2>(typeof(SUIHostLocale));
            if (fontSvc != null) {
                var logFont = new UIDLGLOGFONT[1];
                int hr = fontSvc.GetDialogFont(logFont);
                if (hr == VSConstants.S_OK) {
                    this.Font = IdeUtilities.FontFromUiDialogFont(logFont[0]);
                }
            }
        }

        #region Test support
        internal ComboBox FileListCombo => filesList;
        internal PropertyGrid PropertyGrid => propertyGrid;
        internal TextBox VariableName => variableName;
        internal TextBox VariableValue => variableValue;
        internal Button AddButton => addSettingButton;
        internal ComboBox VariableTypeCombo => variableTypeList;
        #endregion
    }
}
