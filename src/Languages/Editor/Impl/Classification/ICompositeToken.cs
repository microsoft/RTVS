using System.Collections.ObjectModel;

namespace Microsoft.Languages.Editor.Classification
{
    public interface ICompositeToken<TTokenType>
    {
        ReadOnlyCollection<TTokenType> TokenList
        {
            get;
        }
    }
}
