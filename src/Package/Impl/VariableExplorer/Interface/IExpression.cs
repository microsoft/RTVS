using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.VariableWindow {
    /// <summary>
    /// <para>Represents a modifiable variable.</para>
    /// </summary>
    public interface IExpression : IVariable {
        /// <summary>
        /// Updates the expression.
        /// </summary>
        /// <param name="expression">The new expression.</param>
        /// <exception cref="NotSupportedException">This expression cannot be
        /// modified.</exception>
        /// <remarks>
        /// When an expression is set, the session should raise
        /// <see cref="IVariableSession.VariablesChanged"/> to alert all views.
        /// The event should be raised before this task completes.
        /// </remarks>
        Task SetExpressionAsync(string expression);
    }
}
