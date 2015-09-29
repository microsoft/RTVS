using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.VariableWindow {
    /// <summary>
    /// <para>Represents a single variable with one value. See
    /// <see cref="IVariableViewProvider"/> for details on how this value may
    /// be rendered using different views.</para>
    /// <para>Expressions are also represented as instances of
    /// <see cref="IVariable"/>, as the interface is identical.</para>
    /// <para>Reference equality is used on <see cref="IVariable"/> instances
    /// to identify duplicates.</para>
    /// </summary>
    public interface IVariable {
        /// <summary>
        /// The name of the type of this variable. This should use the user's
        /// current UI culture.
        /// </summary>
        string TypeName { get; }

        /// <summary>
        /// A string representation of the variable's name. This should use a
        /// format that can be evaluated by
        /// <see cref="IVariableSession.GetExpressionAsync(string, CancellationToken)"/>,
        /// and may include multiple lines.
        /// </summary>
        string Expression { get; }

        /// <summary>
        /// Changes the value of the variable based on an expression. The
        /// meaning of the expression is defined by the implementation of the
        /// variable, but it is intended to update the actual state that the
        /// session is viewing.
        /// </summary>
        /// <param name="valueExpression">
        /// The expression representing the new value.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// This variable cannot have its value changed.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The expression does not result in a value that can be assigned to
        /// the variable. If this is thrown, the value of the variable is
        /// assumed to be unchanged.
        /// </exception>
        Task SetValueAsync(string valueExpression);

        /// <summary>
        /// Returns <c>true</c> if <see cref="SetValueAsync(string)"/> is known
        /// to raise <see cref="NotSupportedException"/> for all values.
        /// </summary>
        bool IsValueReadOnly { get; }

        /// <summary>
        /// Returns a collection representing the children of this variable.
        /// If <see cref="HasNoChildren"/> is <c>true</c>, may return
        /// <c>null</c> rather than an empty collection.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<IImmutableVariableCollection> GetChildrenAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Returns <c>true</c> if <see cref="GetChildrenAsync"/> is known to
        /// return <c>null</c> or an empty collection.
        /// </summary>
        bool HasNoChildren { get; }

        /// <summary>
        /// Returns a plain-text representation of the value of this variable.
        /// This should use the user's current UI culture.
        /// </summary>
        /// <param name="maximumLength">
        /// The desired maximum length of the result. If zero or negative, any
        /// length string may be returned.
        /// </param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <remarks>
        /// This is a simplification of <see cref="IVariableViewProvider"/> that
        /// all variables are expected to implement. If no views are available
        /// or the context is unsuitable, this function may be called.
        /// </remarks>
        Task<string> ToPlainTextAsync(int maximumLength, CancellationToken cancellationToken);

        /// <summary>
        /// Identifies an image collection to use for identifying this variable.
        /// </summary>
        /// <remarks>
        /// To render a blank space, return <see cref="Guid.Empty"/>.
        /// </remarks>
        Guid ImageMonikerGuid { get; }

        /// <summary>
        /// Identifies an image to use for identifying this variable.
        /// </summary>
        /// <remarks>
        /// If <see cref="ImageMonikerGuid"/> returns <see cref="Guid.Empty"/>,
        /// this property is ignored.
        /// </remarks>
        int ImageMonikerId { get; }
    }
}
