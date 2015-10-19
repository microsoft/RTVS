using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Classification
{
    [ExcludeFromCodeCoverage]
    internal sealed class ClassificationDefinitions
    {
        [Export]
        [Name("R Type Function")]
        internal ClassificationTypeDefinition TypeFunctionClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = "R Type Functions")]
        [Name("R Type Function")]
        internal sealed class TypeFunctionClassificationFormat : ClassificationFormatDefinition
        {
            public TypeFunctionClassificationFormat()
            {
                ForegroundColor = Colors.Teal;
                this.DisplayName = Resources.ColorName_R_TypeFunction;
            }
        }

        [Export]
        [Name("R Function Default Parameter")]
        internal ClassificationTypeDefinition FunctionDefaultParameterClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = "R Function Default Parameter")]
        [Name("R Function Default Parameter")]
        internal sealed class FunctionDefaultParameterClassificationFormat : ClassificationFormatDefinition
        {
            public FunctionDefaultParameterClassificationFormat()
            {
                ForegroundColor = Colors.DarkGray;
                this.DisplayName = Resources.ColorName_R_FunctionDefaultParameter;
            }
        }

        [Export]
        [Name("R Function Reference")]
        internal ClassificationTypeDefinition FunctionReferenceClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = "R Function Reference")]
        [Name("R Function Reference")]
        internal sealed class FunctionReferenceClassificationFormat : ClassificationFormatDefinition
        {
            public FunctionReferenceClassificationFormat()
            {
                ForegroundColor = Colors.Maroon;
                this.DisplayName = Resources.ColorName_R_FunctionReference;
            }
        }
    }
}
