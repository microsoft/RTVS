using System.Diagnostics.CodeAnalysis;

namespace Microsoft.UnitTests.Core.XUnit {
    [ExcludeFromCodeCoverage]
    public static class Category {
        [ExcludeFromCodeCoverage]
        public static class Languages {
            public class CoreAttribute : CategoryAttribute {
                public CoreAttribute() : base("Languages.Core") { }
            }
        }

        [ExcludeFromCodeCoverage]
        public class LoggingAttribute : CategoryAttribute {
            public LoggingAttribute() : base("Logging") { }
        }

        [ExcludeFromCodeCoverage]
        public class InteractiveAttribute : CategoryAttribute {
            public InteractiveAttribute() : base("Interactive") { }
        }

        [ExcludeFromCodeCoverage]
        public static class Md {
            [ExcludeFromCodeCoverage]
            public class ClassifierAttribute : CategoryAttribute {
                public ClassifierAttribute() : base("Md.Classifier") { }
            }

            [ExcludeFromCodeCoverage]
            public class TokenizerAttribute : CategoryAttribute {
                public TokenizerAttribute() : base("Md.Tokenizer") { }
            }
        }

        [ExcludeFromCodeCoverage]
        public class PlotsAttribute : CategoryAttribute {
            public PlotsAttribute() : base("Plots") { }
        }

        [ExcludeFromCodeCoverage]
        public class TelemetryAttribute : CategoryAttribute {
            public TelemetryAttribute() : base("Telemetry") { }
        }

        [ExcludeFromCodeCoverage]
        public static class Project {
            [ExcludeFromCodeCoverage]
            public class ServicesAttribute : CategoryAttribute {
                public ServicesAttribute() : base("Project.Services") { }
            }
        }

        [ExcludeFromCodeCoverage]
        public static class R {
            [ExcludeFromCodeCoverage]
            public class AstAttribute : CategoryAttribute {
                public AstAttribute() : base("R.AST") { }
            }

            [ExcludeFromCodeCoverage]
            public class AutoFormatAttribute : CategoryAttribute {
                public AutoFormatAttribute() : base("R.AutoFormat") {}
            }

            [ExcludeFromCodeCoverage]
            public class BraceMatchAttribute : CategoryAttribute {
                public BraceMatchAttribute() : base("R.BraceMatch") {}
            }

            [ExcludeFromCodeCoverage]
            public class ClassifierAttribute : CategoryAttribute {
                public ClassifierAttribute() : base("R.Classifier") {}
            }

            [ExcludeFromCodeCoverage]
            public class CommentingAttribute : CategoryAttribute {
                public CommentingAttribute() : base("R.Commenting") {}
            }

            [ExcludeFromCodeCoverage]
            public class CompletionAttribute : CategoryAttribute {
                public CompletionAttribute() : base("R.Completion") {}
            }

            [ExcludeFromCodeCoverage]
            public class EditorTreeAttribute : CategoryAttribute {
                public EditorTreeAttribute() : base("R.EditorTree") { }
            }

            [ExcludeFromCodeCoverage]
            public class FormattingAttribute : CategoryAttribute {
                public FormattingAttribute() : base("R.Formatting") { }
            }

            [ExcludeFromCodeCoverage]
            public class InstallAttribute : CategoryAttribute {
                public InstallAttribute() : base("R.Install") { }
            }

            [ExcludeFromCodeCoverage]
            public class OutliningAttribute : CategoryAttribute {
                public OutliningAttribute() : base("R.Outlining") { }
            }

            [ExcludeFromCodeCoverage]
            public class ParserAttribute : CategoryAttribute {
                public ParserAttribute() : base("R.Parser") { }
            }

            [ExcludeFromCodeCoverage]
            public class PackageAttribute : CategoryAttribute {
                public PackageAttribute() : base("R.Package") { }
            }

            [ExcludeFromCodeCoverage]
            public class SettingsAttribute : CategoryAttribute {
                public SettingsAttribute() : base("R.Settings") {}
            }

            [ExcludeFromCodeCoverage]
            public class SignaturesAttribute : CategoryAttribute {
                public SignaturesAttribute() : base("R.Signatures") {}
            }

            [ExcludeFromCodeCoverage]
            public class SmartIndentAttribute : CategoryAttribute {
                public SmartIndentAttribute() : base("R.SmartIndent") {}
            }

            [ExcludeFromCodeCoverage]
            public class TokenizerAttribute : CategoryAttribute {
                public TokenizerAttribute() : base("R.Tokenizer") { }
            }
        }

        [ExcludeFromCodeCoverage]
        public static class Rd {
            [ExcludeFromCodeCoverage]
            public class BraceMatchAttribute : CategoryAttribute {
                public BraceMatchAttribute() : base("Rd.BraceMatch") { }
            }

            [ExcludeFromCodeCoverage]
            public class ClassifierAttribute : CategoryAttribute {
                public ClassifierAttribute() : base("Rd.Classifier") { }
            }

            [ExcludeFromCodeCoverage]
            public class TokenizerAttribute : CategoryAttribute {
                public TokenizerAttribute() : base("Rd.Tokenizer") { }
            }
        }

        [ExcludeFromCodeCoverage]
        public class ReplAttribute : CategoryAttribute {
            public ReplAttribute() : base("Repl") { }
        }

        [ExcludeFromCodeCoverage]
        public static class Variable {
            [ExcludeFromCodeCoverage]
            public class ExplorerAttribute : CategoryAttribute {
                public ExplorerAttribute() : base("Variable.Explorer") {}
            }
        }
    }
}