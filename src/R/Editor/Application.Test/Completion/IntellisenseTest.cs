using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.R.Support.Settings;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Text;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Completion {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class IntellisenseTest {
        [Test]
        [Category.Interactive]
        public void R_KeywordIntellisense() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                script.Type("funct");
                script.DoIdle(100);
                script.Type("{TAB}");

                string expected = "function";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public void R_LibraryIntellisense() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                script.Type("library(ut");
                script.DoIdle(100);
                script.Type("{TAB}");

                string expected = "library(utils)";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public void R_RequireIntellisense() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                script.Type("require(uti");
                script.DoIdle(100);
                script.Type("{TAB}");

                string expected = "require(utils)";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public void R_CompletionFilter01() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                script.Type("x <- lm");
                script.DoIdle(100);
                script.Type("mmm");
                script.DoIdle(100);
                script.Backspace();
                script.Backspace();
                script.Backspace();
                script.Backspace();
                script.DoIdle(100);
                script.Type("abels.{TAB}");

                string expected = "x <- labels.default";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public void R_CompletionFilter02() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                script.Type("x <- lm");
                script.DoIdle(100);
                script.Type("+");

                string expected = "x <- lm+";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public void R_LoadedPackageFunctionCompletion() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                var provider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
                using (new RHostScript(provider)) {
                    REvaluationResult result;

                    script.Type("c");
                    script.DoIdle(200);
                    var session = script.GetCompletionSession();
                    session.Should().NotBeNull();
                    script.DoIdle(500);

                    var list = session.SelectedCompletionSet.Completions.ToList();
                    var item = list.FirstOrDefault(x => x.DisplayText == "codoc");
                    item.Should().BeNull();

                    var rSession = provider.GetOrCreate(GuidList.InteractiveWindowRSessionGuid, null);
                    rSession.Should().NotBeNull();

                    using (var eval = rSession.BeginEvaluationAsync().Result) {
                        result = eval.EvaluateAsync("library('tools')").Result;
                    }

                    script.DoIdle(1000);

                    script.Type("{ESC}");
                    script.DoIdle(200);
                    script.Backspace();
                    script.Type("{ENTER}");
                    script.DoIdle(100);

                    script.Type("c");
                    script.DoIdle(500);
                    script.Backspace();

                    session = script.GetCompletionSession();
                    session.Should().NotBeNull();
                    list = session.SelectedCompletionSet.Completions.ToList();
                    item = list.FirstOrDefault(x => x.DisplayText == "codoc");
                    item.Should().NotBeNull();
                }
            }
        }

        [Test]
        [Category.Interactive]
        public void R_CompletionFiles() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                string asmPath = Assembly.GetExecutingAssembly().GetAssemblyPath();
                RToolsSettings.Current.WorkingDirectory = Path.GetDirectoryName(asmPath);

                script.DoIdle(100);
                script.Type("x <- \"");
                script.DoIdle(1000);
                script.Type("{TAB}");
                script.DoIdle(100);

                var session = script.GetCompletionSession();
                session.Should().NotBeNull();
                script.DoIdle(200);

                var list = session.SelectedCompletionSet.Completions.ToList();
                var item = list.FirstOrDefault(x => x.DisplayText == "ItemTemplates");
                item.Should().NotBeNull();
            }
        }

        [Test]
        [Category.Interactive]
        public void R_CompletionFunctionBraces01() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                var provider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
                using (var hostScript = new RHostScript(provider)) {

                    string message = null;
                    hostScript.Session.Output += (s, e) => {
                        message = e.Message;
                    };

                    script.DoIdle(100);
                    script.Type("instal");
                    script.DoIdle(1000);
                    script.Type("{TAB}");
                    script.DoIdle(100);

                    string actual = script.EditorText;
                    actual.Should().Be("install.packages()");
                    EditorWindow.CoreEditor.View.Caret.Position.BufferPosition.Position.Should().Be(actual.Length - 1);

                    message.Should().NotContain("Error");
                }
            }
        }

        [Test]
        [Category.Interactive]
        public void R_CompletionFunctionBraces02() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                var provider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
                using (var hostScript = new RHostScript(provider)) {

                    string message = null;
                    hostScript.Session.Output += (s, e) => {
                        message = e.Message;
                    };

                    script.DoIdle(100);
                    script.Type("bas");
                    script.DoIdle(1000);
                    script.Type("{TAB}");
                    script.DoIdle(100);

                    string actual = script.EditorText;
                    actual.Should().Be("base");

                    message.Should().NotContain("Error");
                }
            }
        }

        [Test]
        [Category.Interactive]
        public void R_NoCompletionOnTab() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                var provider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
                using (var hostScript = new RHostScript(provider)) {

                    script.DoIdle(100);
                    script.Type("f1<-function(x,y");
                    script.DoIdle(300);
                    script.Type("{TAB}");
                    script.DoIdle(100);

                    string actual = script.EditorText;
                    actual.Should().Be("f1<-function(x,y)");

                    EditorWindow.CoreEditor.View.Caret.Position.BufferPosition.Position.Should().Be(actual.Length);
                }
            }
        }

        [Test]
        [Category.Interactive]
        public void R_CompletionOnTab() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                var provider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
                using (var hostScript = new RHostScript(provider)) {

                    REditorSettings.ShowCompletionOnTab = true;
                    script.DoIdle(100);
                    script.Type("f1<-x");
                    EditorShell.Current.DispatchOnUIThread(() => script.GetCompletionSession().Dismiss());

                    script.DoIdle(300);
                    script.Type("{TAB}");
                    script.DoIdle(500);
                    script.Type("{TAB}");
                    script.DoIdle(200);

                    string actual = script.EditorText;
                    actual.Should().Be("f1<-x11()");

                    REditorSettings.ShowCompletionOnTab = false;
                }
            }
        }

        //[Test]
        //[Category.Interactive]
        public void R_DeclaredVariablesCompletion() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                var provider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
                using (new RHostScript(provider)) {
                    REvaluationResult result;

                    var rSession = provider.GetOrCreate(GuidList.InteractiveWindowRSessionGuid, null);
                    rSession.Should().NotBeNull();

                    using (var eval = rSession.BeginEvaluationAsync().Result) {
                        result = eval.EvaluateAsync("x111 <- 1; x111$y222 <- 2").Result;
                    }

                    script.DoIdle(1000);

                    script.Type("x1");
                    script.DoIdle(500);
                    script.Type("{TAB}");
                    script.DoIdle(500);
                    script.Type("$");
                    script.DoIdle(500);
                    script.Type("y2");
                    script.DoIdle(500);
                    script.Type("{TAB}");

                    string expected = "x111$y222";
                    string actual = script.EditorText;

                    actual.Should().Be(expected);
                }
            }
        }
    }
}
