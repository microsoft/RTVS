// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.UnitTests.Core.Threading;

namespace Microsoft.VisualStudio.R.Interactive.Test.Utility {
    [ExcludeFromCodeCoverage]
    public class VisualTreeObject {
        public VisualTreeObject() {
            Properties = new List<VisualTreeProperty>();
            Children = new List<VisualTreeObject>();
        }

        public string Name { get; set; }

        public List<VisualTreeProperty> Properties { get; set; }

        public List<VisualTreeObject> Children { get; set; }

        public static VisualTreeObject Create(DependencyObject o) {
            VisualTreeObject visualTreeObj = null;

            UIThreadHelper.Instance.Invoke(() => {
                visualTreeObj = new VisualTreeObject();

                visualTreeObj.Name = o.GetType().Name;
                visualTreeObj.Properties = VisualTreeProperty.GetProperties(o).Where(p => SupportedWpfProperties.IsSupported(p.Name)).ToList();
                visualTreeObj.Children = GetChildren(o);
            });

            return visualTreeObj;
        }

        private static List<VisualTreeObject> GetChildren(DependencyObject o) {
            List<VisualTreeObject> children = new List<VisualTreeObject>();

            UIThreadHelper.Instance.Invoke(() => {
                int childrenCount = VisualTreeHelper.GetChildrenCount(o);
                if (childrenCount > 0) {
                    for (int i = 0; i < childrenCount; i++) {
                        var child = VisualTreeHelper.GetChild(o, i);
                        children.Add(Create(child));
                    }
                }
            });

            return children;
        }
    }
}
