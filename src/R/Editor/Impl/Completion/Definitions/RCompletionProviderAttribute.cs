using System;
using System.ComponentModel.Composition;

namespace Microsoft.R.Editor.Completions.Definitions
{
    /// <summary>
    /// Attribute that allows completion provider to specify
    /// what exactly does it supply copletion sets for.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class RCompletionProviderAttribute : ExportAttribute
    {
        public string CompletionType { get; private set; }

        public RCompletionProviderAttribute(string completionType)
            : base(typeof(IRCompletionListProvider))
        {
            this.CompletionType = completionType;
        }
    }
}
