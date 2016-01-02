using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Test.Script;
using Microsoft.Common.Core.Test.STA;
using Microsoft.Common.Core.Test.Utility;

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
            StaThread.Invoke(action);
        }

        public void Dispose() {
            ControlWindow.Close();
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
