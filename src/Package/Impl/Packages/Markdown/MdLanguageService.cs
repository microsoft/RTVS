using System;
using System.Runtime.InteropServices;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.R.Languages;
using Microsoft.VisualStudio.R.Package;

namespace Microsoft.VisualStudio.R.Packages.Markdown {
    [Guid(MdGuidList.MdLanguageServiceGuidString)]
    internal sealed class MdLanguageService : BaseLanguageService {
        public MdLanguageService()
            : base(MdGuidList.MdLanguageServiceGuid,
                   MdContentTypeDefinition.LanguageName,
                   MdContentTypeDefinition.FileExtension1 + ";" +
                   MdContentTypeDefinition.FileExtension2 + ";" +
                   MdContentTypeDefinition.FileExtension3) {
        }

        protected override string SaveAsFilter {
            get { return Resources.SaveAsFilterMD; }
        }
    }
}
