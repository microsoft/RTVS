using System;

namespace Microsoft.Languages.Core.Classification
{
    /// <summary>
    /// Classifier context is a generic descriptor of a colorable item type.
    /// </summary>
    public interface IClassifierContext
    {
        /// <summary>
        /// Determines if context is a 'default' context, i.e. something that is not colorized.
        /// </summary>
        bool IsDefault();

        /// <summary>
        /// Returns type of the context items
        /// </summary>
        Type TypeOfContextObject { get; }

        /// <summary>
        /// Value of the context item. Typically enum value converted to an integer.
        /// </summary>
        int ContextValue { get; }

        /// <summary>
        /// Name of the classification context. A string that can be used with core editor classification registry.
        /// </summary>
        /// <returns></returns>
        string ClassificationName { get; }

        /// <summary>
        /// Compares two contexts and determines if they are equal.
        /// </summary>
        bool IsEqualTo(int contextTypeValue, Type contextObjectType);
    }
}
