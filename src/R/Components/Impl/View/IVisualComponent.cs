using System;
using System.Windows;
using Microsoft.R.Components.Controller;

namespace Microsoft.R.Components.View {
    /// <summary>
    /// Represents visual component such a control inside a tool window
    /// </summary>
    public interface IVisualComponent: IDisposable {
        /// <summary>
        /// Controller to send commands to
        /// </summary>
        ICommandTarget Controller { get; }

        /// <summary>
        /// WPF control to embed in the tool window
        /// </summary>
        FrameworkElement Control { get; }

        /// <summary>
        /// 
        /// </summary>
        IVisualComponentContainer<IVisualComponent> Container { get; }
    }
}
