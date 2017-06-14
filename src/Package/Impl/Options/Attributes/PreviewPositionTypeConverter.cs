// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Common.Core;
using Microsoft.Markdown.Editor.Settings;
using Microsoft.R.Editor;

namespace Microsoft.VisualStudio.R.Package.Options.Attributes {
    internal sealed class PreviewPositionTypeConverter : TypeConverter {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            return sourceType == typeof(string) || sourceType == typeof(bool);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is RMarkdownPreviewPosition) {
                return value;
            }
            var s = value as string;
            if (s == null) {
                return null;
            }
            if (s.EqualsIgnoreCase(Resources.Right)) {
                return RMarkdownPreviewPosition.Right;
            }
            if (s.EqualsIgnoreCase(Resources.Below)) {
                return RMarkdownPreviewPosition.Below;
            }
            return null;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            => destinationType == typeof(string) || destinationType == typeof(RMarkdownPreviewPosition);

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (value.GetType() == destinationType) {
                return value;
            }

            if (destinationType == typeof(string) && value is RMarkdownPreviewPosition) {
                switch ((RMarkdownPreviewPosition)value) {
                    case RMarkdownPreviewPosition.Below:
                        return Resources.Below;
                    case RMarkdownPreviewPosition.Right:
                        return Resources.Right;
                }
            }
            if (destinationType == typeof(RMarkdownPreviewPosition) && value is string) {
                return ConvertFrom(context, CultureInfo.CurrentUICulture, value);
            }
            return null;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => true;
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) 
            => new StandardValuesCollection(new[] { RMarkdownPreviewPosition.Right, RMarkdownPreviewPosition.Below });
    }
}
