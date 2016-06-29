// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Common.Core;
using Microsoft.R.Components.Application.Configuration;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.PropertyPages.Settings {
    [Guid("EE42AA31-44FF-4A83-B098-45C4F98FE9C3")]
    internal class SettingsPropertyPage : PropertyPage {
        internal static readonly string PageName = Resources.ProjectProperties_SettingsPageTitle;
        protected override string PropertyPageName => PageName;

        private readonly PropertyGrid _grid;
        private IReadOnlyList<IConfigurationSetting> _settings;

        public SettingsPropertyPage() {
            _grid = new PropertyGrid();
            this.Load += OnLoad;
            this.SizeChanged += OnSizeChanged;
        }

        private void OnLoad(object sender, EventArgs e) {
            this.Controls.Add(_grid);
            _grid.ToolbarVisible = false;
            _grid.Enabled = true;
        }

        private void OnSizeChanged(object sender, EventArgs e) {
            _grid.Width = this.Width - 40;
            _grid.Height = this.Height - 40;
            _grid.Left = 20;
            _grid.Top = 20;
        }

        protected override Task OnDeactivate() {
            return Task.CompletedTask;
        }

        protected override Task<int> OnApply() {
            return Task.FromResult<int>(VSConstants.S_OK);
        }

        protected override Task OnSetObjects(bool isClosing) {
            if (!isClosing) {
                _grid.SelectedObject = new SettingsTypeDescriptor(_settings);
            }
            return Task.CompletedTask;
        }

        private IReadOnlyList<IConfigurationSetting> LoadSettings() {
            var pss = VsAppShell.Current.ExportProvider.GetExportedValue<IProjectSystemServices>();
            try {
                var proj = pss.GetActiveProject();
                var projectItems = proj?.ProjectItems;
                if (projectItems != null) {
                    foreach (var item in projectItems) {
                        var pi = item as EnvDTE.ProjectItem;
                        var name = pi?.Name;
                        if (!string.IsNullOrEmpty(name) && Path.GetFileName(name).EqualsIgnoreCase("settings.r")) {
                            var filePath = Path.Combine(Path.GetDirectoryName(proj.FullName), pi.Name);
                            using (var sr = new StreamReader(filePath)) {
                                using (var csr = new ConfigurationSettingsReader(sr)) {
                                    return csr.LoadSettings();
                                }
                            }
                        }
                    }
                }
            } catch (COMException) { } catch (IOException) { } catch (AccessViolationException) { }

            return new List<IConfigurationSetting>();
        }
    }
}
