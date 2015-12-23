using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Editor.Test {
    internal static class Category {
        internal static class R {
            internal class AutoFormatAttribute : CategoryAttribute {
                public AutoFormatAttribute() : base("R.AutoFormat") {}
            }

            internal class BraceMatchAttribute : CategoryAttribute {
                public BraceMatchAttribute() : base("R.BraceMatch") {}
            }

            internal class CommentingAttribute : CategoryAttribute {
                public CommentingAttribute() : base("R.Commenting") {}
            }

            internal class CompletionAttribute : CategoryAttribute {
                public CompletionAttribute() : base("R.Completion") {}
            }

            internal class EditorTreeAttribute : CategoryAttribute {
                public EditorTreeAttribute() : base("R.EditorTree") { }
            }

            internal class FormattingAttribute : CategoryAttribute {
                public FormattingAttribute() : base("R.Formatting") { }
            }

            internal class OutliningAttribute : CategoryAttribute {
                public OutliningAttribute() : base("R.Outlining") { }
            }

            internal class SettingsAttribute : CategoryAttribute {
                public SettingsAttribute() : base("R.Settings") {}
            }

            internal class SignaturesAttribute : CategoryAttribute {
                public SignaturesAttribute() : base("R.Signatures") {}
            }

            internal class SmartIndentAttribute : CategoryAttribute {
                public SmartIndentAttribute() : base("R.SmartIndent") {}
            }
        }
    }
}