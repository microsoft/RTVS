namespace Microsoft.UnitTests.Core.XUnit {
    public static class Category {
        public static class Languages {
            public class CoreAttribute : CategoryAttribute {
                public CoreAttribute() : base("Languages.Core") { }
            }
        }

        public class LoggingAttribute : CategoryAttribute {
            public LoggingAttribute() : base("Logging") { }
        }

        public class InteractiveAttribute : CategoryAttribute {
            public InteractiveAttribute() : base("Interactive") { }
        }

        public static class Md {
            public class ClassifierAttribute : CategoryAttribute {
                public ClassifierAttribute() : base("Md.Classifier") { }
            }

            public class TokenizerAttribute : CategoryAttribute {
                public TokenizerAttribute() : base("Md.Tokenizer") { }
            }
        }

        public class PlotsAttribute : CategoryAttribute {
            public PlotsAttribute() : base("Plots") { }
        }

        public static class Project {
            public class ServicesAttribute : CategoryAttribute {
                public ServicesAttribute() : base("Project.Services") { }
            }
        }

        public static class R {
            public class AstAttribute : CategoryAttribute {
                public AstAttribute() : base("R.AST") { }
            }

            public class AutoFormatAttribute : CategoryAttribute {
                public AutoFormatAttribute() : base("R.AutoFormat") {}
            }

            public class BraceMatchAttribute : CategoryAttribute {
                public BraceMatchAttribute() : base("R.BraceMatch") {}
            }

            public class ClassifierAttribute : CategoryAttribute {
                public ClassifierAttribute() : base("R.Classifier") {}
            }

            public class CommentingAttribute : CategoryAttribute {
                public CommentingAttribute() : base("R.Commenting") {}
            }

            public class CompletionAttribute : CategoryAttribute {
                public CompletionAttribute() : base("R.Completion") {}
            }

            public class EditorTreeAttribute : CategoryAttribute {
                public EditorTreeAttribute() : base("R.EditorTree") { }
            }

            public class FormattingAttribute : CategoryAttribute {
                public FormattingAttribute() : base("R.Formatting") { }
            }

            public class InstallAttribute : CategoryAttribute {
                public InstallAttribute() : base("R.Install") { }
            }

            public class OutliningAttribute : CategoryAttribute {
                public OutliningAttribute() : base("R.Outlining") { }
            }

            public class ParserAttribute : CategoryAttribute {
                public ParserAttribute() : base("R.Parser") { }
            }

            public class PackageAttribute : CategoryAttribute {
                public PackageAttribute() : base("R.Package") { }
            }

            public class SettingsAttribute : CategoryAttribute {
                public SettingsAttribute() : base("R.Settings") {}
            }

            public class SignaturesAttribute : CategoryAttribute {
                public SignaturesAttribute() : base("R.Signatures") {}
            }

            public class SmartIndentAttribute : CategoryAttribute {
                public SmartIndentAttribute() : base("R.SmartIndent") {}
            }

            public class TokenizerAttribute : CategoryAttribute {
                public TokenizerAttribute() : base("R.Tokenizer") { }
            }
        }

        public static class Rd {
            public class BraceMatchAttribute : CategoryAttribute {
                public BraceMatchAttribute() : base("Rd.BraceMatch") { }
            }

            public class ClassifierAttribute : CategoryAttribute {
                public ClassifierAttribute() : base("Rd.Classifier") { }
            }

            public class TokenizerAttribute : CategoryAttribute {
                public TokenizerAttribute() : base("Rd.Tokenizer") { }
            }
        }

        public class ReplAttribute : CategoryAttribute {
            public ReplAttribute() : base("Repl") { }
        }

        public static class Variable {
            public class ExplorerAttribute : CategoryAttribute {
                public ExplorerAttribute() : base("Variable.Explorer") {}
            }
        }
    }
}