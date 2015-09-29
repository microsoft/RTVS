using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.VariableWindow {
    /// <summary>
    /// May be implemented on <see cref="IVariable"/> instances to provide
    /// generic data support. This may enable a range of view providers for the
    /// variable.
    /// </summary>
    public interface IVariableData {
        /// <summary>
        /// Returns whether the specified content type is supported by this
        /// variable.
        /// </summary>
        bool SupportsContentType(string contentType);

        /// <summary>
        /// Returns a stream of data for the specified content type.
        /// </summary>
        /// <param name="contentType">The content type.</param>
        /// <exception cref="NotSupportedException">
        /// <paramref name="contentType"/> is not supported by this variable.
        /// </exception>
        Task<Stream> GetDataAsync(string contentType);
    }
}
