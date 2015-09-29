using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.VariableWindow {
    /// <summary>
    /// <para>Represents a variable session, which is a source for a list of
    /// variables and expressions.</para>
    /// </summary>
    /// <remarks>
    /// Sessions are compared for equality using reference equality.
    /// </remarks>
    public interface IVariableSession {
        /// <summary>
        /// The string displayed to users when selecting between sessions. This
        /// should use the user's current UI culture.
        /// </summary>
        /// <remarks>
        /// Sessions with identical <see cref="SessionDisplayName"/> values
        /// may be merged in some UI views, with variable collections
        /// concatenated in <see cref="Priority"/> order.
        /// </remarks>
        string SessionDisplayName { get; }

        /// <summary>
        /// A value for ordering sessions, where lower values sort earlier.
        /// When two sessions have the same value, they are ordered by
        /// <see cref="SessionDisplayName"/>.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Gets a collection of variables contained by this session.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="ObjectDisposedException">
        /// The session has been closed.
        /// </exception>
        /// <remarks>
        /// This collection should be sorted.
        /// </remarks>
        Task<IImmutableVariableCollection> GetVariablesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets a mutable variable representing the given expression. If the
        /// expression is invalid, the returned value should reflect that in its
        /// visualizations rather than causing this method to fail.
        /// </summary>
        /// <remarks>
        /// Expressions behave identically to variables, but may be created from
        /// user-provided text.
        /// </remarks>
        /// <param name="expression">The expression</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="ObjectDisposedException">
        /// The session has been closed.
        /// </exception>
        Task<IExpression> GetExpressionAsync(string expression, CancellationToken cancellationToken);

        /// <summary>
        /// Raised when the collection of variables has changed. This implies
        /// that listeners should call <see cref="GetVariablesAsync"/> again,
        /// and reevaluate all other variables and expressions associated with
        /// this session.
        /// </summary>
        /// <remarks>
        /// <para>The session is not required to invalidate its own caching (if
        /// any) if it is aware that not all variables have changed. However,
        /// listeners to this event must assume that all variables may have
        /// changed and need to be reevaluated.</para>
        /// <para>This event should be raised from a background thread.</para>
        /// </remarks>
        event EventHandler VariablesChanged;

        /// <summary>
        /// Raised when the session is invalidated and should no longer be
        /// offered to the user.
        /// </summary>
        /// <remarks>
        /// <para>When this event is raised, all variables associated with this
        /// session are assumed to have been destroyed. Subsequent calls into
        /// this session may raise <see cref="ObjectDisposedException"/>.</para>
        /// <para>This event should be raised from a background thead.</para>
        /// </remarks>
        event EventHandler SessionClosed;
    }
}
