// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Application.Configuration;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.PropertyPages.Settings {
    internal partial class SettingsPageControl : UserControl {
        private readonly IConfigurationSettingCollection _settings = new ConfigurationSettingCollection();
        private readonly ProjectProperties[] _properties;
        private readonly SettingsPageViewModel _viewModel;
        private ICoreShell _coreShell;
        private IProjectSystemServices _pss;
        private int _selectedIndex;
        private bool _isDirty;

        public SettingsPageControl(ProjectProperties[] properties) {
            _properties = properties;
            _viewModel = new SettingsPageViewModel(_settings, CoreShell, FileSystem, ProjectServices);
            InitializeComponent();
        }

        public event EventHandler<EventArgs> DirtyStateChanged;
        public bool IsDirty {
            get { return _isDirty; }
            set {
                if (_isDirty != value) {
                    _isDirty = value;
                    DirtyStateChanged.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public async Task<bool> SaveAsync() {
            var result = _viewModel != null ? await _viewModel.SaveAsync(_properties) : true;
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

            explanationText.Text = Resources.SettingsPage_Explanation1
                                    + Environment.NewLine + Environment.NewLine
                                    + Resources.SettingsPage_Explanation2
                                    + Environment.NewLine + Environment.NewLine
                                    + Resources.SettingsPage_Explanation3;
        }

        private void PopulateFilesCombo() {
            filesList.Items.AddRange(_viewModel.Files.ToArray());
            if (filesList.Items.Count == 0) {
                filesList.Items.Add(Resources.NoSettingFiles);
            }
            _selectedIndex = filesList.SelectedIndex = 0;
        }

        private void LoadPropertyGrid() {
            IsDirty = false;
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
                        _viewModel.SaveAsync(_properties).DoNotWait();
                        IsDirty = false;
                    }
                }
                _selectedIndex = filesList.SelectedIndex;
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
                if (hr == VSConstants.S_OK)
                    this.Font = IdeUtilities.FontFromUiDialogFont(logFont[0]);
            }
        }

        #region Test support
        internal IFileSystem FileSystem { get; set; } = new FileSystem();
        internal ICoreShell CoreShell {
            get {
                if (_coreShell == null) {
                    _coreShell = VsAppShell.Current;
                }
                return _coreShell;
            }
            set {
                Debug.Assert(_coreShell == null);
                _coreShell = value;
            }
        }

        internal IProjectSystemServices ProjectServices {
            get {
                if (_pss == null) {
                    _pss = CoreShell.ExportProvider.GetExportedValue<IProjectSystemServices>();
                }
                return _pss;
            }
            set {
                Debug.Assert(_pss == null);
                _pss = value;
            }
        }
        #endregion
    }
}
