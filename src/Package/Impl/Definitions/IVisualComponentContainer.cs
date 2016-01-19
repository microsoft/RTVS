using System;
using System.Windows.Controls;
using Microsoft.Languages.Editor.Controller;

namespace Microsoft.VisualStudio.R.Package.Definitions {
    /// <summary>
    /// Represents UI element that holds visual component
    /// (typically a tool window)
    /// </summary>
    public interface IVisualComponentContainer<T> where T : IVisualComponent {
        T Component { get; }
    }
}
