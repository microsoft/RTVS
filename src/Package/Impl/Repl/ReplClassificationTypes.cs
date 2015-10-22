using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Repl
{
    public class ReplClassificationTypes
    {
        public const string ReplPromptClassification = "RReplPrompt";

        [Export]
        [Name(ReplPromptClassification)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        internal readonly ClassificationTypeDefinition ReplPromptTypeDefinition;

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = ReplPromptClassification)]
        [Name(ReplPromptClassification + "Format")]
        [Order(After = Priority.Default, Before = Priority.High)]
        [UserVisible(true)]
        private sealed class ReplPromptClassificationFormat
            : ClassificationFormatDefinition
        {
            private ReplPromptClassificationFormat()
            {
                this.DisplayName = Resources.RPromptClassification;
                this.ForegroundColor = Colors.Blue;
                this.BackgroundColor = Colors.LightGray;
            }
        }
    }
}