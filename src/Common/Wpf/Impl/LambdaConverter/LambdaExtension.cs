// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;
using System.Xaml;

namespace Microsoft.Common.Wpf {
    [ContentProperty("Lambda")]
    public class LambdaExtension : MarkupExtension {
        public string Lambda { get; set; }

        public LambdaExtension() {
        }

        public LambdaExtension(string lambda) {
            Lambda = lambda;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            var isDesignMode = (bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue;
            if (isDesignMode) {
                return null;
            }

            if (Lambda == null) {
                throw new InvalidOperationException("Lambda not specified");
            }

            var rootProvider = (IRootObjectProvider)serviceProvider.GetService(typeof(IRootObjectProvider));
            var root = rootProvider.RootObject;
            if (root == null) {
                throw new InvalidOperationException("Cannot locate root object - service provider did not provide IRootObjectProvider");
            }

            var provider = root as ILambdaConverterProvider;
            if (provider == null) {
                throw new InvalidOperationException("Root object does not implement ILambdaConverterProvider - code generator not run");
            }

            return provider.GetConverterForLambda(Lambda);
        }
    }
}
