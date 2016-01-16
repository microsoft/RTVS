using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
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
