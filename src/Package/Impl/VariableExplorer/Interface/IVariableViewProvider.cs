using System.Drawing;
using System.Threading.Tasks;
using System.Windows;

namespace Microsoft.VisualStudio.VariableWindow {
    /// <summary>
    /// Implemented by services that can provide a view over certain variables.
    /// </summary>
    public interface IVariableViewProvider {
        /// <summary>
        /// A user-friendly string describing the type of view. This should use
        /// the user's current UI culture.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// A value for ordering views, where lower values sort earlier.
        /// When two views have the same value, they are ordered by
        /// <see cref="DisplayName"/>.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Determines whether the view provider supports the specified
        /// variable.
        /// </summary>
        /// <param name="session">
        /// The session that owns the variable.
        /// </param>
        /// <param name="variable">
        /// The variable to view.
        /// </param>
        /// <param name="interactive">
        /// <c>true</c> if the UI is allowed to be interactive. If <c>false</c>,
        /// the variable should only be supported if a static view can be
        /// provided.
        /// </param>
        /// <returns>
        /// <c>true</c> if the view provider supports the variable.
        /// </returns>
        bool IsSupported(IVariableSession session, IVariable variable, bool interactive);

        /// <summary>
        /// <para>Gets a UI view for the specified variable. Returns <c>null</c>
        /// if the variable is not supported.</para>
        /// <para>While the UI element may change over time for a specific
        /// variable, if the UI element is not changing then the original
        /// instance should be returned again. The view provider is responsible
        /// for preserving object identity for variables until the session is
        /// closed.</para>
        /// </summary>
        /// <param name="session">
        /// The session that owns the variable.
        /// </param>
        /// <param name="variable">
        /// The variable to view.
        /// </param>
        /// <param name="bounds">
        /// The approximate allocated space for the view. The returned UI is
        /// responsible for detecting and handling resize.
        /// </param>
        /// <param name="interactive">
        /// <c>true</c> if the UI is allowed to be interactive. If <c>false</c>,
        /// the returned element should not accept focus or respond to input
        /// events.
        /// </param>
        /// <remarks>
        /// This function is initially called from off the UI thread, to allow
        /// implementations to determine whether they support the variable. When
        /// instantiating the return value, the implementation is responsible
        /// for mashalling to the UI thread.
        /// </remarks>
        Task<UIElement> GetUIElementAsync(IVariableSession session, IVariable variable, Size bounds, bool interactive);
    }
}
