// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Microsoft.Common.Core.Test.Utility {
    [ExcludeFromCodeCoverage]
    public sealed class VisualTreeWriter {
        private int _indent = 0;
        private StringBuilder _sb;
        private Control _control;
        private bool _writeProperties;

        public string WriteTree(Control control, bool writeProperties) {
            _sb = new StringBuilder();
            _indent = 0;
            _control = control;
            _writeProperties = writeProperties;

            WriteObject(control);

            string text = _sb.ToString();
            _sb = null;
            _control = null;

            return text;
        }

        private void WriteObject(DependencyObject o) {
            WriteObjectName(o);

            if (_writeProperties) {
                WriteObjectProperties(o);
            }

            int childrenCount = VisualTreeHelper.GetChildrenCount(o);
            if (childrenCount > 0) {
                WriteSeparator();
                _indent++;

                for (int i = 0; i < childrenCount; i++) {
                    var child = VisualTreeHelper.GetChild(o, i);
                    WriteObject(child);
                }

                _indent--;
                WriteSeparator();
            }
        }

        private void WriteObjectName(DependencyObject o) {
            Indent();
            _sb.Append(o.GetType().Name);
            _sb.Append("\r\n");
        }

        private void WriteSeparator() {
            Indent();
            _sb.Append("--------------------------------------------------------------\r\n");
        }

        private void WriteObjectProperties(DependencyObject o) {
            var properties = GetAttachedProperties(o);
            for (int i = 0; i < properties.Count; i++) {
                var prop = properties[i];
                if (SupportedWpfProperties.IsSupported(prop.Name)) {
                    object value = o.GetValue(prop);
                    Indent();
                    _sb.Append(prop.Name);
                    _sb.Append(" = ");
                    _sb.Append(value != null ? value.ToString().Trim() : "null");
                    _sb.Append("\r\n");
                }
            }
        }


        private static IList<DependencyProperty> GetAttachedProperties(DependencyObject obj) {
            List<DependencyProperty> result = new List<DependencyProperty>();

            foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(obj,
                new Attribute[] { new PropertyFilterAttribute(PropertyFilterOptions.All) })) {
                DependencyPropertyDescriptor dpd =
                    DependencyPropertyDescriptor.FromProperty(pd);

                if (dpd != null) {
                    result.Add(dpd.DependencyProperty);
                }
            }

            return result;
        }

        private void Indent() {
            _sb.Append(' ', _indent * 4);
        }
    }
}
