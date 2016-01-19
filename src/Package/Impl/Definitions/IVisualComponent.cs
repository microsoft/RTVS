using System;
using System.Windows.Controls;
using Microsoft.Languages.Editor.Controller;

namespace Microsoft.VisualStudio.R.Package.Definitions {
    /// <summary>
    /// Represents visual component such a control inside a tool window
    /// </summary>
    internal interface IVisualComponent: IDisposable {
        /// <summary>
        /// Controller to send commands to
        /// </summary>
        ICommandTarget Controller { get; }

        /// <summary>
        /// WPF control to embed in the tool window
        /// </summary>
        Control Control { get; }
    }
}
