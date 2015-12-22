using System;
using System.ComponentModel.Composition;

namespace Microsoft.R.Editor.Completions.Providers {
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CompletionTypeAttribute : Attribute {
        public string CompletionType { get; private set; }

        public CompletionTypeAttribute(string completionType) {
            CompletionType = completionType;
        }
    }
}
