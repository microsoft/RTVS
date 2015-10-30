using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Support.RD.Classification {
    [ExcludeFromCodeCoverage]
    internal sealed class RdClassificationDefinitions {
        [Export]
        [Name("RD Braces")]
        internal ClassificationTypeDefinition RdBracesClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = "RD Braces")]
        [Name("RD Braces")]
        internal sealed class RdBracesClassificationFormat : ClassificationFormatDefinition {
            public RdBracesClassificationFormat() {
                ForegroundColor = Colors.Gray;
                this.DisplayName = Resources.ColorName_RD_CurlyBraces;
            }
        }
    }
}
