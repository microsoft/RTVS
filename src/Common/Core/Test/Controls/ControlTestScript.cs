// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Test.Script;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.UnitTests.Core.Threading;

namespace Microsoft.Common.Core.Test.Controls {
    [ExcludeFromCodeCoverage]
    public sealed class ControlTestScript : TestScript, IDisposable {
        public ControlTestScript(Type type, IServiceContainer services) {
            ControlWindow.Create(type, services);
        }
        /// <summary>
        /// Invokes a particular action in the control window thread
        /// </summary>
        public void Invoke(Action action) => UIThreadHelper.Instance.Invoke(action);

        public void Dispose() => ControlWindow.Close();

        public DependencyObject Control => ControlWindow.Control;

        public string WriteVisualTree(bool writeProperties = true) {
            var w = new VisualTreeWriter();
            string tree = null;
            Invoke(() => tree = w.WriteTree(ControlWindow.Control, writeProperties));
            return tree;
        }
    }
}
