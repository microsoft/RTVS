// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.UnitTests.Core.XUnit {
    public static class Category {
        public static class Languages {
            public class CoreAttribute : CategoryAttribute {
                public CoreAttribute() : base("Languages.Core") { }
            }

            public class ContainedAttribute : CategoryAttribute {
                public ContainedAttribute() : base("Languages.Contained") { }
            }
        }

        public class HistoryAttribute : CategoryAttribute {
            public HistoryAttribute() : base("History") { }
        }

        public class PackageManagerAttribute : CategoryAttribute {
            public PackageManagerAttribute() : base("PackageManager") { }
        }

        public class LoggingAttribute : CategoryAttribute {
            public LoggingAttribute() : base("Logging") { }
        }

        public class CoreExtensionsAttribute : CategoryAttribute {
            public CoreExtensionsAttribute() : base("Core.Extensions") { }
        }

        public class InteractiveAttribute : CategoryAttribute {
            public InteractiveAttribute() : base("Interactive") { }
        }

        public class ConnectionsAttribute : CategoryAttribute {
            public ConnectionsAttribute() : base("Connections") { }
        }

        public class InformationAttribute : CategoryAttribute {
            public InformationAttribute() : base("Information") { }
        }

        public static class Md {
            public class ClassifierAttribute : CategoryAttribute {
                public ClassifierAttribute() : base("Markdown.Classifier") { }
            }

            public class TokenizerAttribute : CategoryAttribute {
                public TokenizerAttribute() : base("Markdown.Tokenizer") { }
            }

            public class RCodeAttribute : CategoryAttribute {
                public RCodeAttribute() : base("Markdown.RCode") { }
            }

            public class PreviewAttribute : CategoryAttribute {
                public PreviewAttribute() : base("Markdown.Preview") { }
            }
        }

        public class PlotsAttribute : CategoryAttribute {
            public PlotsAttribute() : base("Plots") { }
        }

        public class ContainersAttribute : CategoryAttribute {
            public ContainersAttribute() : base("Containers") { }
        }

        public class ConfigurationAttribute : CategoryAttribute {
            public ConfigurationAttribute() : base("Configuration") { }
        }

        public class HelpAttribute : CategoryAttribute {
            public HelpAttribute() : base("Help") { }
        }

        public class TelemetryAttribute : CategoryAttribute {
            public TelemetryAttribute() : base("Telemetry") { }
        }

        public static class Project {
            public class ServicesAttribute : CategoryAttribute {
                public ServicesAttribute() : base("Project.Services") { }
            }

            public class FileSystemMirrorAttribute : CategoryAttribute {
                public FileSystemMirrorAttribute() : base("Project.FileSystemMirror") { }
            }
        }

        public static class R {
            public class AstAttribute : CategoryAttribute {
                public AstAttribute() : base("R.AST") { }
            }

            public class AutoFormatAttribute : CategoryAttribute {
                public AutoFormatAttribute() : base("R.AutoFormat") { }
            }

            public class BraceMatchAttribute : CategoryAttribute {
                public BraceMatchAttribute() : base("R.BraceMatch") { }
            }

            public class ClassifierAttribute : CategoryAttribute {
                public ClassifierAttribute() : base("R.Classifier") { }
            }

            public class CommentingAttribute : CategoryAttribute {
                public CommentingAttribute() : base("R.Commenting") { }
            }

            public class CompletionAttribute : CategoryAttribute {
                public CompletionAttribute() : base("R.Completion") { }
            }

            public class DragDropAttribute : CategoryAttribute {
                public DragDropAttribute() : base("R.DragDrop") { }
            }

            public class DataInspectionAttribute : CategoryAttribute {
                public DataInspectionAttribute() : base("R.DataInspection") { }
            }

            public class DataGridAttribute : CategoryAttribute {
                public DataGridAttribute() : base("R.DataGrid") { }
            }

            public class DocumentationAttribute : CategoryAttribute {
                public DocumentationAttribute() : base("R.Documentation") { }
            }

            public class EditorAttribute : CategoryAttribute {
                public EditorAttribute() : base("R.Editor") { }
            }

            public class EditorTreeAttribute : CategoryAttribute {
                public EditorTreeAttribute() : base("R.EditorTree") { }
            }

            public class ExecutionTracingAttribute : CategoryAttribute {
                public ExecutionTracingAttribute() : base("R.ExecutionTracing") { }
            }

            public class FormattingAttribute : CategoryAttribute {
                public FormattingAttribute() : base("R.Formatting") { }
            }

            public class InstallAttribute : CategoryAttribute {
                public InstallAttribute() : base("R.Install") { }
            }

            public class NavigationAttribute : CategoryAttribute {
                public NavigationAttribute() : base("R.Navigation") { }
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

            public class RtvsPackageAttribute : CategoryAttribute {
                public RtvsPackageAttribute() : base("R.RtvsPackage") { }
            }

            public class SessionAttribute : CategoryAttribute {
                public SessionAttribute() : base("R.Session") { }
            }

            public static class Session {
                public class ApiAttribute : CategoryAttribute {
                    public ApiAttribute() : base("R.Session.API") { }
                }
            }

            public class SettingsAttribute : CategoryAttribute {
                public SettingsAttribute() : base("R.Settings") { }
            }

            public class SignaturesAttribute : CategoryAttribute {
                public SignaturesAttribute() : base("R.Signatures") { }
            }

            public class SmartIndentAttribute : CategoryAttribute {
                public SmartIndentAttribute() : base("R.SmartIndent") { }
            }

            public class StackTracingAttribute : CategoryAttribute {
                public StackTracingAttribute() : base("R.StackTracing") { }
            }

            public class TokenizerAttribute : CategoryAttribute {
                public TokenizerAttribute() : base("R.Tokenizer") { }
            }

            public class LinterAttribute : CategoryAttribute {
                public LinterAttribute() : base("R.Linter") { }
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

        public class RoxygenAttribute : CategoryAttribute {
            public RoxygenAttribute() : base("Roxygen") { }
        }

        public class SqlAttribute : CategoryAttribute {
            public SqlAttribute() : base("SQL") { }
        }

        public class ProjectAttribute : CategoryAttribute {
            public ProjectAttribute() : base("Project") { }
        }

        public static class Variable {
            public class ExplorerAttribute : CategoryAttribute {
                public ExplorerAttribute() : base("Variable.Explorer") { }
            }
        }

        public class ThreadsAttribute : CategoryAttribute {
            public ThreadsAttribute() : base("Threads") { }
        }

        public class ViewersAttribute : CategoryAttribute {
            public ViewersAttribute() : base("Viewers") { }
        }

        public class HtmlAttribute : CategoryAttribute {
            public HtmlAttribute() : base("Html") { }
        }

        public class FuzzTestAttribute : CategoryAttribute {
            public FuzzTestAttribute() : base("FuzzTest") { }
        }

        public static class VsPackage {
            public class SettingsAttribute : CategoryAttribute {
                public SettingsAttribute() : base("VS.Package.Settings") { }
            }

        }

        public class LinuxAttribute : CategoryAttribute {
            public LinuxAttribute() : base("Linux") { }
        }

        public static class VsCode {
            public class EditorAttribute : CategoryAttribute {
                public EditorAttribute() : base("VsCode.Editor") { }
            }
            public class ThreadingAttribute : CategoryAttribute {
                public ThreadingAttribute() : base("VsCode.Threading") { }
            }
        }
    }
}