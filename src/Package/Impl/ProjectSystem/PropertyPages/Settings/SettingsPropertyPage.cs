// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.PropertyPages.Settings {
    [Guid("EE42AA31-44FF-4A83-B098-45C4F98FE9C3")]
    internal class SettingsPropertyPage : PropertyPage {
        internal static readonly string PageName = Resources.ProjectProperties_SettingsPageTitle;
        private readonly SettingsPageControl _control;

        public SettingsPropertyPage() {
            _control = new SettingsPageControl();
            _control.DirtyStateChanged += OnDirtyStateChanged;
            Load += OnLoad;
        }

        private void OnDirtyStateChanged(object sender, EventArgs e) => IsDirty = _control.IsDirty;

        private void OnLoad(object sender, EventArgs e) {
            Controls.Add(_control);
            AutoScroll = true;
        }

        protected override string PropertyPageName => PageName;

        protected override Task OnDeactivate() => _control.SaveSelectedSettingsFileNameAsync();

        protected override async Task<int> OnApply()
            => await _control.SaveSettingsAsync() ? VSConstants.S_OK : VSConstants.E_FAIL;

        protected override async Task OnSetObjects(bool isClosing) {
            if(!isClosing) {
                Debug.Assert(!string.IsNullOrEmpty(UnconfiguredProject.FullPath));
                await _control.SetProjectAsync(UnconfiguredProject, ConfiguredProperties[0]);
            } else {
                _control.Close();
            }
        }
    }
}
