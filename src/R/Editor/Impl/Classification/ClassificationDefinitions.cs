using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using Microsoft.R.Core.Classification;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Classification
{
    [ExcludeFromCodeCoverage]
    internal sealed class ClassificationDefinitions
    {
        [Export]
        [Name("R Punctuation")]
        internal ClassificationTypeDefinition PunctuationClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = "R Punctuation")]
        [Name("R Punctuation")]
        internal sealed class PunctuationClassificationFormat : ClassificationFormatDefinition
        {
            public PunctuationClassificationFormat()
            {
                ForegroundColor = Colors.DarkBlue;
                this.DisplayName = Resources.ColorName_R_Punctuation;
            }
        }

        [Export]
        [Name("R Braces")]
        internal ClassificationTypeDefinition BracesClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = "R Braces")]
        [Name("R Braces")]
        internal sealed class BracesClassificationFormat : ClassificationFormatDefinition
        {
            public BracesClassificationFormat()
            {
                ForegroundColor = Colors.DarkBlue;
                this.DisplayName = Resources.ColorName_R_Braces;
            }
        }

        [Export]
        [Name("R Square Brackets")]
        internal ClassificationTypeDefinition BracketsClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = "R Square Brackets")]
        [Name("R Square Brackets")]
        internal sealed class BracketsClassificationFormat : ClassificationFormatDefinition
        {
            public BracketsClassificationFormat()
            {
                ForegroundColor = Colors.DarkBlue;
                this.DisplayName = Resources.ColorName_R_Brackets;
            }
        }

        [Export]
        [Name("R Curly Braces")]
        internal ClassificationTypeDefinition CurlyBracesClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = "R Curly Braces")]
        [Name("R Curly Braces")]
        internal sealed class CurlyBracesClassificationFormat : ClassificationFormatDefinition
        {
            public CurlyBracesClassificationFormat()
            {
                ForegroundColor = Colors.DarkBlue;
                this.DisplayName = Resources.ColorName_R_CurlyBraces;
            }
        }

        [Export]
        [Name("R Builtin Function")]
        internal ClassificationTypeDefinition BuiltinClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = "R Builtin Function")]
        [Name("R Builtin Function")]
        internal sealed class BuiltinClassificationFormat : ClassificationFormatDefinition
        {
            public BuiltinClassificationFormat()
            {
                ForegroundColor = Colors.DarkKhaki;
                this.DisplayName = Resources.ColorName_R_Builtin;
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
        [Name("R Function Definition")]
        internal ClassificationTypeDefinition FunctionDefinitionClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = "R Function Definition")]
        [Name("R Function Definition")]
        internal sealed class FunctionDefinitionClassificationFormat : ClassificationFormatDefinition
        {
            public FunctionDefinitionClassificationFormat()
            {
                ForegroundColor = Colors.Maroon;
                this.DisplayName = Resources.ColorName_R_FunctionDefinition;
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

        [Export]
        [Name("R Variable Definition")]
        internal ClassificationTypeDefinition VariableDefinitionClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = "R Variable Definition")]
        [Name("R Variable Definition")]
        internal sealed class VariableDefinitionClassificationFormat : ClassificationFormatDefinition
        {
            public VariableDefinitionClassificationFormat()
            {
                ForegroundColor = Colors.Teal;
                this.DisplayName = Resources.ColorName_R_VariableDefinition;
            }
        }

        [Export]
        [Name("R Variable Reference")]
        internal ClassificationTypeDefinition VariableReferenceClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = "R Variable Reference")]
        [Name("R Variable Reference")]
        internal sealed class VariableReferenceClassificationFormat : ClassificationFormatDefinition
        {
            public VariableReferenceClassificationFormat()
            {
                ForegroundColor = Colors.Teal;
                this.DisplayName = Resources.ColorName_R_VariableReference;
            }
        }
    }
}
