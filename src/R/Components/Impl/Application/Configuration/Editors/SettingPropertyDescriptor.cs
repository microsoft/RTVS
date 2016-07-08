// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;

namespace Microsoft.R.Components.Application.Configuration {
    /// <summary>
    /// Represents a single entry in the property grid in the Project | Properties | Settings page
    /// </summary>
    [Browsable(true)]
    [DesignTimeVisible(true)]
    public sealed class SettingPropertyDescriptor : PropertyDescriptor {
        public IConfigurationSetting Setting { get; }

        public SettingPropertyDescriptor(IConfigurationSetting setting) :
                base(setting.Name, null) {
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
            if(!string.IsNullOrEmpty(Setting.Category)) {
                attributeList.Add(new CategoryAttribute(Setting.Category));
            }
            if (!string.IsNullOrEmpty(Setting.Description)) {
                attributeList.Add(new DescriptionAttribute(Setting.Description));
            }
            if (!string.IsNullOrEmpty(Setting.EditorType)) {
                attributeList.Add(new EditorAttribute(Setting.EditorType, typeof(UITypeEditor)));
            }
            base.FillAttributes(attributeList);
        }
    }
}
