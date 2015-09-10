using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Core.Classification
{
    /// <summary>
    /// Provides classification names for a particular token types
    /// </summary>
    public interface IClassificationNameProvider<T>
    {
        string GetClassificationName(T t);
    }

    /// <summary>
    /// Generic provider of classification names. Typically used
    /// in scenarios where one language is embedded into another
    /// but editor projections are not supported yet.
    /// </summary>
    public interface IClassificationNameProvider
    {
        string GetClassificationName(object o, out ITextRange range);
    }
}
