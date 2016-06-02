// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Xml.Serialization;

namespace Microsoft.VisualStudio.R.Interactive.Test.Utility {
    [ExcludeFromCodeCoverage]
    public class VisualTreeProperty {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Value { get; set; }

        public override bool Equals(object obj) {
            if (ReferenceEquals(this, obj)) {
                return true;
            }

            var other = obj as VisualTreeProperty;
            if (other == null) {
                return false;
            }

            return Name == other.Name && Value == other.Value;
        }

        public override int GetHashCode() {
            return (Name == null ? 0 : Name.GetHashCode())
                ^ (Value == null ? 0 : Value.GetHashCode());
        }

        public static List<VisualTreeProperty> GetProperties(DependencyObject o) {
            var props = GetAttachedProperties(o);

            List<VisualTreeProperty> visualTreeProps = new List<VisualTreeProperty>();
            foreach (var prop in props) {
                visualTreeProps.Add(Create(o, prop));
            }
            return visualTreeProps;
        }

        public static VisualTreeProperty Create(DependencyObject o, DependencyProperty prop) {
            object value = o.GetValue(prop);

            var visualTreeProp = new VisualTreeProperty();
            visualTreeProp.Name = prop.Name;
            visualTreeProp.Value = value == null ? "null" : value.ToString().Trim();

            return visualTreeProp;
        }

        private static IList<DependencyProperty> GetAttachedProperties(DependencyObject obj) {
            List<DependencyProperty> result = new List<DependencyProperty>();

            foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(obj.GetType())) {
                var dpd = DependencyPropertyDescriptor.FromProperty(pd);
                if (dpd != null) {
                    result.Add(dpd.DependencyProperty);
                }
            }
            return result;
        }
    }
}
