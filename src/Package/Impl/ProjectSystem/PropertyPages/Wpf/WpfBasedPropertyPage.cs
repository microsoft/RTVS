// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.PropertyPages {
    internal abstract partial class WpfBasedPropertyPage : PropertyPage {
        private PropertyPageElementHost _host;
        private PropertyPageControl _control;
        private PropertyPageViewModel _viewModel;

        public WpfBasedPropertyPage() {
            InitializeComponent();
        }

        protected abstract PropertyPageViewModel CreatePropertyPageViewModel();

        protected abstract PropertyPageControl CreatePropertyPageControl();

        protected override async Task OnSetObjects(bool isClosing) {
            if (isClosing) {
                _control.DetachViewModel();
                return;
            } else {
                //viewModel can be non-null when the configuration is changed. 
                if (_control == null) {
                    _control = CreatePropertyPageControl();
                }
            }

            _viewModel = CreatePropertyPageViewModel();

            await _viewModel.Initialize();

            _control.InitializePropertyPage(_viewModel);
        }

        protected async override Task<int> OnApply() {
            return await _control.Apply();
        }

        protected async override Task OnDeactivate() {
            if (IsDirty) {
                await OnApply();
            }
        }

        private void WpfPropertyPage_Load(object sender, EventArgs e) {
            SuspendLayout();

            _host = new PropertyPageElementHost();
            _host.AutoSize = false;
            _host.Dock = DockStyle.Fill;

            if (_control == null) {
                _control = CreatePropertyPageControl();
            }

            ScrollViewer viewer = new ScrollViewer {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            viewer.Content = _control;
            _host.Child = viewer;

            wpfHostPanel.Dock = DockStyle.Fill;
            wpfHostPanel.Controls.Add(_host);

            ResumeLayout(true);
            _control.StatusChanged += _control_OnControlStatusChanged;
        }

        private void _control_OnControlStatusChanged(object sender, EventArgs e) {
            if (IsDirty != _control.IsDirty) {
                IsDirty = _control.IsDirty;
            }
        }
    }
}
