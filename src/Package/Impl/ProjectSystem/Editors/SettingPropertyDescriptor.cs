// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing.Design;
using Microsoft.Common.Core.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Application.Configuration;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    /// <summary>
    /// Represents a single entry in the property grid in the Project | Properties | Settings page
    /// </summary>
    [Browsable(true)]
    [DesignTimeVisible(true)]
    public sealed class SettingPropertyDescriptor : PropertyDescriptor {
        private readonly ICoreShell _coreShell;

        public IConfigurationSetting Setting { get; }

        public SettingPropertyDescriptor(ICoreShell coreShell, IConfigurationSetting setting) :
                base(setting.Name, null) {
            _coreShell = coreShell;
            Setting = setting;
        }

        public override Type ComponentType => this.GetType();
        public override bool IsReadOnly => false;
        public override Type PropertyType => typeof(string);
        public override bool CanResetValue(object component) => false;
        public override object GetValue(object component) => Setting.Value;
        public override void ResetValue(object component) { }
        public override void SetValue(object component, object value) {
            Setting.Value = value as string;
        }
        public override bool ShouldSerializeValue(object component) => false;

        protected override void FillAttributes(IList attributeList) {
            if (!string.IsNullOrEmpty(Setting.Category)) {
                attributeList.Add(new CategoryAttribute(Setting.Category));
            }
            if (!string.IsNullOrEmpty(Setting.Description)) {
                attributeList.Add(new DescriptionAttribute(Setting.Description));
            }
            if (!string.IsNullOrEmpty(Setting.EditorType)) {
                var expl = new NamedExportLocator<IConfigurationSettingUIEditorProvider>(_coreShell.GetService<ICompositionService>());
                var provider = expl.GetExport(Setting.EditorType);
                if (provider != null) {
                    attributeList.Add(new EditorAttribute(provider.EditorType, typeof(UITypeEditor)));
                }
            }
            base.FillAttributes(attributeList);
        }
    }
}
