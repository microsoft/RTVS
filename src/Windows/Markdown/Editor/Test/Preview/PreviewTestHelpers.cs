// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.Markdown.Editor.Settings;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using NSubstitute;

namespace Microsoft.Markdown.Editor.Test.Preview {
    internal static class PreviewTestHelpers {
        public static IRSession SetupSessionSubstitute(this TestCoreShell shell) {
            var wfp = Substitute.For<IRInteractiveWorkflowProvider>();
            shell.ServiceManager.AddService(wfp);

            var workflow = Substitute.For<IRInteractiveWorkflow>();
            wfp.GetOrCreate().Returns(workflow);

            var session = Substitute.For<IRSession>();
            session.IsHostRunning.Returns(true);

            var sessionProvider = Substitute.For<IRSessionProvider>();
            sessionProvider.GetOrCreate(Arg.Any<string>()).Returns(session);

            workflow.RSessions.Returns(sessionProvider);
            return session;
        }

        public static IRMarkdownEditorSettings SetupSettingsSubstitute(this TestCoreShell shell) {
            var settings = Substitute.For<IRMarkdownEditorSettings>();
            shell.ServiceManager.AddService(settings);
            return settings;
        }
    }
}
