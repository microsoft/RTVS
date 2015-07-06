using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.Languages.Core.Classification
{
    [ExcludeFromCodeCoverage]
    public abstract class ClassifierContext<T> : IClassifierContext
    {
        public T ContextType { get; set; }

        public abstract bool IsDefault();

        public Type TypeOfContextObject
        {
            get { return typeof(T); }
        }

        public int ContextValue
        {
            get { return Convert.ToInt32(ContextType, CultureInfo.InvariantCulture); }
        }

        public bool IsEqualTo(int contextTypeValue, Type contextObjectType)
        {
            return (this.TypeOfContextObject == contextObjectType) && (this.ContextValue == contextTypeValue);
        }

        public abstract string ClassificationName { get; }
    }
}
