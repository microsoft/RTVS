using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Test.STA;
using Microsoft.Common.Core.Test.Utility;

namespace Microsoft.VisualStudio.R.Application.Test {
    [ExcludeFromCodeCoverage]
    public sealed class ControlTestScript : IDisposable {
        public ControlTestScript(Type type) {
            ControlWindow.Create(type);
        }
        /// <summary>
        /// Invokes a particular action in the editor window
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
