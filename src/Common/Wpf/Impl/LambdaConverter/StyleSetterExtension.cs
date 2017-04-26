// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Markup;

namespace Microsoft.Common.Wpf {
    public class StyleSetterExtension : MarkupExtension {
        public object Style { get; set; }
        public DependencyProperty Property { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            var styleMarkupExtension = Style as MarkupExtension;
            var style = (styleMarkupExtension?.ProvideValue(serviceProvider) ?? Style) as Style;
            while (style != null) {
                var propertySetter = style.Setters
                    .OfType<Setter>()
                    .FirstOrDefault(s => Equals(Property, s.Property));
                if (propertySetter != null) {
                    return propertySetter.Value;
                }

                style = style.BasedOn;
            }

            return null;
        }
    }
}