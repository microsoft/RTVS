// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.R.Components.Application.Configuration;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.PropertyPages.Settings {
    /// <summary>
    /// Represents view model for the property grid in the Project | Properties | Settings
    /// </summary>
    public sealed class SettingsTypeDescriptor : ICustomTypeDescriptor {
        private const string _componentName = "R Settings";
        private readonly IConfigurationSettingCollection _settings;

        public SettingsTypeDescriptor(IConfigurationSettingCollection settings) {
            _settings = settings;
        }

        public AttributeCollection GetAttributes() {
            return new AttributeCollection(new Attribute[] { BrowsableAttribute.Yes, DesignTimeVisibleAttribute.Yes });
        }

        public string GetClassName() => _componentName;
        public string GetComponentName() => _componentName;

        public TypeConverter GetConverter() => null;
        public EventDescriptor GetDefaultEvent() => null;
        public PropertyDescriptor GetDefaultProperty() => null;

        public object GetEditor(Type editorBaseType) {
            return null;
        }

        public EventDescriptorCollection GetEvents() => new EventDescriptorCollection(null);
        public EventDescriptorCollection GetEvents(Attribute[] attributes) => new EventDescriptorCollection(null);

        public PropertyDescriptorCollection GetProperties() {
            return new PropertyDescriptorCollection(GetProps());
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes) {
            return new PropertyDescriptorCollection(GetProps());
        }

        public object GetPropertyOwner(PropertyDescriptor pd) => this;

        private PropertyDescriptor[] GetProps() {
            return _settings.Select(s => new SettingPropertyDescriptor(s)).ToArray();
        }
    }
}
