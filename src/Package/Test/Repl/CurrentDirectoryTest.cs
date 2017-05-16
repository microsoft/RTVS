// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client.Session;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    [Category.Repl]
    [Collection(CollectionNames.NonParallel)]
    public class CurrentDirectoryTest : HostBasedInteractiveTest {
        private readonly WorkingDirectoryCommand _cmd;

        public CurrentDirectoryTest(IServiceContainer services) : base(services) {
            _cmd = new WorkingDirectoryCommand(Workflow as IRInteractiveWorkflowVisual);
        }

        public override async Task InitializeAsync() {
            await _cmd.InitializationTask;
            await base.InitializeAsync();
        }

        [Test]
        public async Task DefaultDirectoryTest() {
            var myDocs = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            _cmd.UserDirectory.Should().BeEquivalentTo(myDocs);
            var actual = await HostScript.Session.GetRWorkingDirectoryAsync();
            actual.Should().Be(myDocs);
        }

        [Test]
        public async Task SetDirectoryTest() {
            var dir = "c:\\";
            await _cmd.SetDirectory(dir);
            var actual = Workflow.RSession.GetRWorkingDirectoryAsync().Result;
            actual.Should().Be(dir);
        }

        [Test]
        public void GetFriendlyNameTest01() {
            var actual = _cmd.GetFriendlyDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
            actual.Should().Be("~");
        }

        [Test]
        public void GetFriendlyNameTest02() {
            var actual = _cmd.GetFriendlyDirectoryName("c:\\");
            actual.Should().Be("c:/");
        }

        [Test]
        public void GetFullPathNameTest() {
            var dir = _cmd.GetFullPathName("~");
            var actual = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            actual.Should().Be(dir);
        }
    }
}
