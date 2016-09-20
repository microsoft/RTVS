// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Editor.Snippets;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.R.Support.Settings;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Language.Intellisense;
using Xunit;
using static System.FormattableString;

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
        public async Task R_LoadedPackageFunctionCompletion() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                var provider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
                using (new RHostScript(provider)) {
                    script.Type("c");
                    script.DoIdle(200);
                    var session = script.GetCompletionSession();
                    session.Should().NotBeNull();
                    script.DoIdle(500);

                    var list = session.SelectedCompletionSet.Completions.ToList();
                    var item = list.FirstOrDefault(x => x.DisplayText == "codoc");
                    item.Should().BeNull();

                    var rSession = provider.GetOrCreate(GuidList.InteractiveWindowRSessionGuid);
                    rSession.Should().NotBeNull();

                    await rSession.ExecuteAsync("library('tools')");

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
        public void R_CompletionFilesUserFolder() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                var provider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
                using (new RHostScript(provider)) {
                    var myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    var testFolder = Path.Combine(myDocs, "_rtvs_test_");
                    if (!Directory.Exists(testFolder)) {
                        Directory.CreateDirectory(testFolder);
                    }

                    script.DoIdle(100);
                    script.Type("x <- \"~/");
                    script.DoIdle(1000);
                    script.Type("{TAB}");
                    script.DoIdle(500);

                    var session = script.GetCompletionSession();
                    session.Should().NotBeNull();
                    script.DoIdle(200);

                    var list = session.SelectedCompletionSet.Completions.ToList();
                    var item = list.FirstOrDefault(x => x.DisplayText == "_rtvs_test_");
                    item.Should().NotBeNull();

                    if (Directory.Exists(testFolder)) {
                        Directory.Delete(testFolder);
                    }
                }
            }
        }

        [Test]
        [Category.Interactive]
        public void R_CompletionFilesAbsolute() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                var root = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.Windows));

                script.DoIdle(100);
                script.Type(Invariant($"x <- \"{root[0]}:/"));
                script.DoIdle(1000);
                script.Type("{TAB}");
                script.DoIdle(100);

                var session = script.GetCompletionSession();
                session.Should().NotBeNull();
                script.DoIdle(200);

                var list = session.SelectedCompletionSet.Completions.ToList();
                var item = list.FirstOrDefault(x => x.DisplayText == "Windows");
                item.Should().NotBeNull();
            }
        }

        // Disabled since auto-insertion of braces is off
        //[Test]
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
                    script.Type("f1<-Lapp");
                    EditorShell.Current.DispatchOnUIThread(() => script.GetCompletionSession().Dismiss());

                    script.DoIdle(300);
                    script.Type("{TAB}");
                    script.DoIdle(500);
                    script.Type("{TAB}");
                    script.DoIdle(200);

                    string actual = script.EditorText;
                    actual.Should().Be("f1<-lapply");

                    REditorSettings.ShowCompletionOnTab = false;
                }
            }
        }

        [Test]
        [Category.Interactive]
        public void R_NoCompletionOnTabWhenNoMatch() {
            // Tab only completes when selected item starts
            // with the text typed so far in the buffer
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                script.DoIdle(100);
                script.Type("while aaa");
                script.DoIdle(300);
                script.Type("{TAB}");
                script.DoIdle(100);

                string actual = script.EditorText;
                actual.Should().Be("while aaa"); // nothing was inserted from the completion list

                EditorWindow.CoreEditor.View.Caret.Position.BufferPosition.Position.Should().Be(actual.Length);
            }
        }

        [Test]
        [Category.Interactive]
        public void R_NoCompletionOnTabInComment() {
            // Tab only completes when selected item starts
            // with the text typed so far in the buffer
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                script.DoIdle(100);
                script.Type("#com");
                script.DoIdle(300);
                script.Type("{TAB}a");
                script.DoIdle(100);

                string actual = script.EditorText;
                actual.Should().Be("#com    a"); // Tab was not consumed
            }
        }

        [Test]
        [Category.Interactive]
        public void R_SnippetsCompletion01() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                script.DoIdle(100);
                script.Type("whil");
                script.DoIdle(300);

                EditorShell.Current.DispatchOnUIThread(() => {
                    var session = script.GetCompletionSession();
                    session.Should().NotBeNull();

                    var infoSourceProvider = EditorShell.Current.ExportProvider.GetExportedValue<ISnippetInformationSourceProvider>();
                    var infoSource = infoSourceProvider.InformationSource;
                    var completion = session.SelectedCompletionSet.SelectionStatus.Completion;

                    bool isSnippet = infoSource.IsSnippet(completion.DisplayText);
                    isSnippet.Should().BeTrue();

                    var glyph = completion.IconSource;
                    var snippetGlyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphCSharpExpansion, StandardGlyphItem.GlyphItemPublic);
                    glyph.Should().Be(snippetGlyph);
                });
            }
        }

        [Test]
        [Category.Interactive]
        public async Task R_DeclaredVariablesCompletion01() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                var provider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
                using (var hostScript = new RHostScript(provider)) {

                    await ExecuteRCode(hostScript.Session, "zzz111 <- 1\r\n");
                    await ExecuteRCode(hostScript.Session, "zzz111$y222 <- 2\r\n");

                    PrimeIntellisenseProviders();

                    script.DoIdle(500);
                    script.Type("zzz1");
                    script.DoIdle(500);
                    script.Type("{TAB}");
                    script.DoIdle(500);
                    script.Type("$");
                    script.DoIdle(500);
                    script.Type("y2");
                    script.DoIdle(500);
                    script.Type("{TAB}");

                    string expected = "zzz111$y222";
                    string actual = script.EditorText;

                    actual.Should().Be(expected);
                }
            }
        }

        [Test]
        [Category.Interactive]
        public async Task R_DeclaredVariablesCompletion02() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                var provider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
                using (var hostScript = new RHostScript(provider)) {

                    await ExecuteRCode(hostScript.Session, "setClass('Person', representation(name = 'character', age = 'numeric'))\r\n");
                    await ExecuteRCode(hostScript.Session, "hadley <- new('Person', name = 'Hadley', age = 31)\r\n");

                    PrimeIntellisenseProviders();

                    script.DoIdle(1000);
                    script.Type("hadle");
                    script.DoIdle(500);
                    script.Type("{TAB}");
                    script.DoIdle(500);
                    script.Type("@");
                    script.DoIdle(500);
                    script.Type("na");
                    script.DoIdle(500);
                    script.Type("{TAB}");

                    string expected = "hadley@name";
                    string actual = script.EditorText;

                    actual.Should().Be(expected);
                }
            }
        }

        [Test]
        [Category.Interactive]
        public async Task R_DeclaredVariablesCompletion03() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                var provider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
                using (var hostScript = new RHostScript(provider)) {

                    await ExecuteRCode(hostScript.Session, "i1 <- 1\r\n");
                    PrimeIntellisenseProviders();
                    script.DoIdle(1000);

                    script.Type("i");

                    var session = script.GetCompletionSession();
                    session.Should().NotBeNull();

                    var list = session.SelectedCompletionSet.Completions.ToList();
                    var item = list.FirstOrDefault(x => x.DisplayText == "i1");
                    item.Should().NotBeNull();
                    script.DoIdle(100);

                    script.Type("{ESC}");
                    script.Backspace();

                    script.DoIdle(100);
                    script.Type("graphics::");
                    script.DoIdle(300);

                    session = script.GetCompletionSession();
                    session.Should().NotBeNull();

                    list = session.SelectedCompletionSet.Completions.ToList();
                    item = list.FirstOrDefault(x => x.DisplayText == "i1");
                    item.Should().BeNull();
                }
            }
        }

        [Test]
        [Category.Interactive]
        public void R_PackageVariablesCompletion() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                var provider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
                using (var hostScript = new RHostScript(provider)) {
                    PrimeIntellisenseProviders();
                    script.DoIdle(1000);

                    script.Type("mtcars$");

                    var session = script.GetCompletionSession();
                    session.Should().NotBeNull();
                    script.DoIdle(1000);

                    var list = session.SelectedCompletionSet.Completions.ToList();
                    var item = list.FirstOrDefault(x => x.DisplayText == "cyl");
                    item.Should().NotBeNull();
                    script.DoIdle(100);
                }
            }
        }

        private void PrimeIntellisenseProviders() {
            // Prime variable provider
            EditorShell.Current.DispatchOnUIThread(() => {
                var broker = EditorShell.Current.ExportProvider.GetExportedValue<ICompletionBroker>();
                broker.TriggerCompletion(EditorWindow.CoreEditor.View);
                broker.DismissAllSessions(EditorWindow.CoreEditor.View);
            });
        }

        private async Task ExecuteRCode(IRSession session, string expression) {
            using (var interaction = await session.BeginInteractionAsync(isVisible: false)) {
                await interaction.RespondAsync(expression);
            }
        }
    }
}
