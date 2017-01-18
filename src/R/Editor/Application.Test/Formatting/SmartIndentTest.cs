// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class SmartIndentTest : IAsyncLifetime {
        private readonly IExportProvider _exportProvider;
        private readonly IRSessionProvider _sessionProvider;
        private readonly EditorHostMethodFixture _editorHost;

        public SmartIndentTest(IExportProvider exportProvider, EditorHostMethodFixture editorHost) {
            _exportProvider = exportProvider;
            _sessionProvider = UIThreadHelper.Instance.Invoke(() => _exportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate()).RSessions;
            _editorHost = editorHost;
        }

        public Task InitializeAsync() => _sessionProvider.TrySwitchBrokerAsync(nameof(SmartIndentTest));

        public Task DisposeAsync() => Task.CompletedTask;

        [Test]
        [Category.Interactive]
        public async Task R_SmartIndentTest01() {
            using (var script = await _editorHost.StartScript(_exportProvider, string.Empty, RContentTypeDefinition.ContentType)) {
                REditorSettings.FormatOptions.BracesOnNewLine = false;
                script.MoveRight();
                script.Type("{{ENTER}a");
                script.DoIdle(300);

                string expected = "{\r\n    a\r\n}";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public async Task R_SmartIndentTest02() {
            using (var script = await _editorHost.StartScript(_exportProvider, string.Empty, "file", RContentTypeDefinition.ContentType, _sessionProvider)) {
                REditorSettings.FormatOptions.BracesOnNewLine = false;
                script.Type("if(TRUE)");
                script.DoIdle(300);
                script.Type("{ENTER}abb");
                script.DoIdle(300);
                script.Type("{ENTER}{ENTER}x <-1{ENTER}");
                script.DoIdle(300);

                string expected = "if (TRUE)\r\n    abbreviate\r\nx <- 1\r\n";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public async Task R_SmartIndentTest03() {
            using (var script = await _editorHost.StartScript(_exportProvider, string.Empty, RContentTypeDefinition.ContentType)) {
                REditorSettings.FormatOptions.BracesOnNewLine = false;
                script.MoveRight();
                script.Type("while(TRUE){{ENTER}if(1){");
                script.DoIdle(200);
                script.Type("{ENTER}a");
                 script.DoIdle(200);
                script.MoveDown();
                script.DoIdle(200);
                script.Type("else {{ENTER}b");
                script.DoIdle(200);

                string expected = "while (TRUE) {\r\n    if (1) {\r\n        a\r\n    } else {\r\n        b\r\n    }\r\n}";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public async Task R_SmartIndentTest04() {
            using (var script = await _editorHost.StartScript(_exportProvider, string.Empty, RContentTypeDefinition.ContentType)) {
                REditorSettings.FormatOptions.BracesOnNewLine = false;
                script.MoveRight();
                script.Type("{{ENTER}if(1)");
                script.DoIdle(200);
                script.Type("{ENTER}a<-1{ENTER}");
                script.DoIdle(200);
                script.Type("else {ENTER}z<-2;");
                script.DoIdle(200);

                string expected = "{\r\n    if (1)\r\n        a <- 1\r\n    else\r\n        z <- 2;\r\n}";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }
    }
}
