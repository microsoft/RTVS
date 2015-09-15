using System.Linq;
using Microsoft.Languages.Core.Classification;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Composition;

namespace Microsoft.R.Support.Markdown.Classification
{
    internal sealed class MdRCodeClassificationNameProvider: IClassificationNameProvider
    {
        private IClassificationNameProvider _nameProvider;

        public MdRCodeClassificationNameProvider()
        {
            var providers = ComponentLocatorForContentType<IClassificationNameProvider, IComponentContentTypes>.ImportMany("R");
            if (providers != null)
            {
                _nameProvider = providers.FirstOrDefault().Value;
            }
        }
        public string GetClassificationName(object o, out ITextRange range)
        {
            range = TextRange.EmptyRange;

            if (_nameProvider != null)
            {
                return _nameProvider.GetClassificationName(o, out range);
            }

            return "Default";
        }
    }
}
