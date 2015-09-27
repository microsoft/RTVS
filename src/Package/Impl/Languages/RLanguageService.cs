using System;
using System.Runtime.InteropServices;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.R.Package;

namespace Microsoft.VisualStudio.R.Languages
{
    [Guid(RGuidList.RLanguageServiceGuidString)]
    internal sealed class RLanguageService : BaseLanguageService
    {
        public RLanguageService()
            : base(RGuidList.RLanguageServiceGuid, 
                   RContentTypeDefinition.LanguageName, 
                   RContentTypeDefinition.FileExtension)
        {
        }

        protected override string SaveAsFilter
        {
            get { return Resources.SaveAsFilterR; }
        }
    }
}
