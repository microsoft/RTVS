using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Xml.Serialization;
using Microsoft.Common.Core.Test.Script;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.UnitTests.Core.Threading;

namespace Microsoft.Common.Core.Test.Controls {
    [ExcludeFromCodeCoverage]
    public sealed class ControlTestScript : TestScript, IDisposable {
        public ControlTestScript(Type type) {
            ControlWindow.Create(type);
        }
        /// <summary>
        /// Invokes a particular action in the control window thread
        /// </summary>
        public void Invoke(Action action) {
            UIThreadHelper.Instance.Invoke(action);
        }

        public void Dispose() {
            ControlWindow.Close();
        }

        public DependencyObject Control {
            get {
                return ControlWindow.Control;
            }
        }

        public string WriteVisualTree(bool writeProperties = true) {
            VisualTreeWriter w = new VisualTreeWriter();
            string tree = null;
            Invoke(() => {
                tree = w.WriteTree(ControlWindow.Control, writeProperties);
            });
            return tree;
        }
    }
}
