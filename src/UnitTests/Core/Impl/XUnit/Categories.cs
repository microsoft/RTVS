// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.UnitTests.Core.XUnit {
    [ExcludeFromCodeCoverage]
    public static class Category {
        [ExcludeFromCodeCoverage]
        public static class Languages {
            [ExcludeFromCodeCoverage]
            public class CoreAttribute : CategoryAttribute {
                public CoreAttribute() : base("Languages.Core") { }
            }
            [ExcludeFromCodeCoverage]
            public class ContainedAttribute : CategoryAttribute {
                public ContainedAttribute() : base("Languages.Contained") { }
            }
        }

        [ExcludeFromCodeCoverage]
        public class HistoryAttribute : CategoryAttribute {
            public HistoryAttribute() : base("History") { }
        }

        [ExcludeFromCodeCoverage]
        public class PackageManagerAttribute : CategoryAttribute {
            public PackageManagerAttribute() : base("PackageManager") { }
        }

        [ExcludeFromCodeCoverage]
        public class LoggingAttribute : CategoryAttribute {
            public LoggingAttribute() : base("Logging") { }
        }

        [ExcludeFromCodeCoverage]
        public class CoreExtensionsAttribute : CategoryAttribute {
            public CoreExtensionsAttribute() : base("Core.Extensions") { }
        }

        [ExcludeFromCodeCoverage]
        public class InteractiveAttribute : CategoryAttribute {
            public InteractiveAttribute() : base("Interactive") { }
        }

        [ExcludeFromCodeCoverage]
        public class ConnectionsAttribute : CategoryAttribute {
            public ConnectionsAttribute() : base("Connections") { }
        }

        [ExcludeFromCodeCoverage]
        public class InformationAttribute : CategoryAttribute {
            public InformationAttribute() : base("Information") { }
        }

        [ExcludeFromCodeCoverage]
        public static class Md {
            [ExcludeFromCodeCoverage]
            public class ClassifierAttribute : CategoryAttribute {
                public ClassifierAttribute() : base("Markdown.Classifier") { }
            }

            [ExcludeFromCodeCoverage]
            public class TokenizerAttribute : CategoryAttribute {
                public TokenizerAttribute() : base("Markdown.Tokenizer") { }
            }

            [ExcludeFromCodeCoverage]
            public class RCodeAttribute : CategoryAttribute {
                public RCodeAttribute() : base("Markdown.RCode") { }
            }

            [ExcludeFromCodeCoverage]
            public class PreviewAttribute : CategoryAttribute {
                public PreviewAttribute() : base("Markdown.Preview") { }
            }
        }

        [ExcludeFromCodeCoverage]
        public class PlotsAttribute : CategoryAttribute {
            public PlotsAttribute() : base("Plots") { }
        }

        [ExcludeFromCodeCoverage]
        public class ConfigurationAttribute : CategoryAttribute {
            public ConfigurationAttribute() : base("Configuration") { }
        }

        [ExcludeFromCodeCoverage]
        public class SurveyNewsAttribute : CategoryAttribute {
            public SurveyNewsAttribute() : base("SurveyNews") { }
        }

        [ExcludeFromCodeCoverage]
        public class HelpAttribute : CategoryAttribute {
            public HelpAttribute() : base("Help") { }
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
            [ExcludeFromCodeCoverage]
            public class FileSystemMirrorAttribute : CategoryAttribute {
                public FileSystemMirrorAttribute() : base("Project.FileSystemMirror") { }
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
                public AutoFormatAttribute() : base("R.AutoFormat") { }
            }

            [ExcludeFromCodeCoverage]
            public class BraceMatchAttribute : CategoryAttribute {
                public BraceMatchAttribute() : base("R.BraceMatch") { }
            }

            [ExcludeFromCodeCoverage]
            public class ClassifierAttribute : CategoryAttribute {
                public ClassifierAttribute() : base("R.Classifier") { }
            }

            [ExcludeFromCodeCoverage]
            public class CommentingAttribute : CategoryAttribute {
                public CommentingAttribute() : base("R.Commenting") { }
            }

            [ExcludeFromCodeCoverage]
            public class CompletionAttribute : CategoryAttribute {
                public CompletionAttribute() : base("R.Completion") { }
            }

            [ExcludeFromCodeCoverage]
            public class DragDropAttribute : CategoryAttribute {
                public DragDropAttribute() : base("R.DragDrop") { }
            }

            [ExcludeFromCodeCoverage]
            public class DataInspectionAttribute : CategoryAttribute {
                public DataInspectionAttribute() : base("R.DataInspection") { }
            }

            [ExcludeFromCodeCoverage]
            public class DataGridAttribute : CategoryAttribute {
                public DataGridAttribute() : base("R.DataGrid") { }
            }

            [ExcludeFromCodeCoverage]
            public class DocumentationAttribute : CategoryAttribute {
                public DocumentationAttribute() : base("R.Documentation") { }
            }

            [ExcludeFromCodeCoverage]
            public class EditorTreeAttribute : CategoryAttribute {
                public EditorTreeAttribute() : base("R.EditorTree") { }
            }

            [ExcludeFromCodeCoverage]
            public class ExecutionTracingAttribute : CategoryAttribute {
                public ExecutionTracingAttribute() : base("R.ExecutionTracing") { }
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
            public class NavigationAttribute : CategoryAttribute {
                public NavigationAttribute() : base("R.Navigation") { }
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
            public class RtvsPackageAttribute : CategoryAttribute {
                public RtvsPackageAttribute() : base("R.RtvsPackage") { }
            }

            [ExcludeFromCodeCoverage]
            public class SessionAttribute : CategoryAttribute {
                public SessionAttribute() : base("R.Session") { }
            }

            [ExcludeFromCodeCoverage]
            public static class Session { 
                [ExcludeFromCodeCoverage]
                public class ApiAttribute : CategoryAttribute {
                    public ApiAttribute() : base("R.Session.API") { }
                }
            }

            [ExcludeFromCodeCoverage]
            public class SettingsAttribute : CategoryAttribute {
                public SettingsAttribute() : base("R.Settings") { }
            }

            [ExcludeFromCodeCoverage]
            public class SignaturesAttribute : CategoryAttribute {
                public SignaturesAttribute() : base("R.Signatures") { }
            }

            [ExcludeFromCodeCoverage]
            public class SmartIndentAttribute : CategoryAttribute {
                public SmartIndentAttribute() : base("R.SmartIndent") { }
            }

            [ExcludeFromCodeCoverage]
            public class StackTracingAttribute : CategoryAttribute {
                public StackTracingAttribute() : base("R.StackTracing") { }
            }

            [ExcludeFromCodeCoverage]
            public class TokenizerAttribute : CategoryAttribute {
                public TokenizerAttribute() : base("R.Tokenizer") { }
            }

            [ExcludeFromCodeCoverage]
            public class LinterAttribute : CategoryAttribute {
                public LinterAttribute() : base("R.Linter") { }
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
        public class RoxygenAttribute : CategoryAttribute {
            public RoxygenAttribute() : base("Roxygen") { }
        }


        [ExcludeFromCodeCoverage]
        public class SqlAttribute : CategoryAttribute {
            public SqlAttribute() : base("SQL") { }
        }

        [ExcludeFromCodeCoverage]
        public class ProjectAttribute : CategoryAttribute {
            public ProjectAttribute() : base("Project") { }
        }

        [ExcludeFromCodeCoverage]
        public static class Variable {
            [ExcludeFromCodeCoverage]
            public class ExplorerAttribute : CategoryAttribute {
                public ExplorerAttribute() : base("Variable.Explorer") { }
            }
        }

        [ExcludeFromCodeCoverage]
        public class ThreadsAttribute : CategoryAttribute {
            public ThreadsAttribute() : base("Threads") { }
        }

        [ExcludeFromCodeCoverage]
        public class ViewersAttribute : CategoryAttribute {
            public ViewersAttribute() : base("Viewers") { }
        }

        [ExcludeFromCodeCoverage]
        public class HtmlAttribute : CategoryAttribute {
            public HtmlAttribute() : base("Html") { }
        }

        [ExcludeFromCodeCoverage]
        public class FuzzTestAttribute : CategoryAttribute {
            public FuzzTestAttribute() : base("FuzzTest") { }
        }

        [ExcludeFromCodeCoverage]
        public static class VsPackage {
            [ExcludeFromCodeCoverage]
            public class SettingsAttribute : CategoryAttribute {
                public SettingsAttribute() : base("VS.Package.Settings") { }
            }
        }
    }
}