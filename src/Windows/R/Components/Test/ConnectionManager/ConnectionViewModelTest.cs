// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.ConnectionManager.Implementation.ViewModel;
using Microsoft.UnitTests.Core.XUnit;
using NSubstitute;
using Xunit;

namespace Microsoft.R.Components.Test.ConnectionManager {
    [ExcludeFromCodeCoverage]
    [Category.Connections]
    public sealed class ConnectionViewModelTest {
        [Test]
        public void Construction01() {
            var cm = new ConnectionViewModel();
            cm.IsUserCreated.Should().BeTrue();
            cm.IsValid.Should().BeFalse();
            cm.IsTestConnectionSucceeded.Should().BeFalse();
            cm.Name.Should().BeNull();
        }

        [Test]
        public void Construction02() {
            var uri = new Uri("http://microsoft.com");
            var conn = Substitute.For<IConnection>();
            conn.Uri.Returns(uri);
            conn.Name.Returns("name");
            conn.Path.Returns("path");
            conn.RCommandLineArguments.Returns("arg");
            conn.IsRemote.Returns(true);

            var cm = new ConnectionViewModel(conn);
            cm.IsUserCreated.Should().BeFalse();

            conn.IsUserCreated.Returns(true);
            cm = new ConnectionViewModel(conn);

            conn.IsRemote.Should().BeTrue();
            cm.IsUserCreated.Should().BeTrue();
            cm.IsEditing.Should().BeFalse();
            cm.IsTestConnectionSucceeded.Should().BeFalse();
            cm.Name.Should().Be(conn.Name);
            cm.Path.Should().Be(conn.Path);
            cm.RCommandLineArguments.Should().Be(conn.RCommandLineArguments);
        }

        public enum FieldState {
            Valid = 0,
            Missing,
            Invalid
        }

        [CompositeTest]
        [InlineData(null, null, FieldState.Missing, FieldState.Missing)]
        [InlineData("$", null, FieldState.Invalid, FieldState.Missing)]
        [InlineData("..", null, FieldState.Invalid, FieldState.Missing)]
        [InlineData("R 3.3.1", null, FieldState.Valid, FieldState.Missing)]
        [InlineData("MRO (3.3.2)", null, FieldState.Valid, FieldState.Missing)]
        [InlineData("MRO [3.3.2]", null, FieldState.Valid, FieldState.Missing)]
        [InlineData("M1_M2-3", null, FieldState.Valid, FieldState.Missing)]
        [InlineData("a", "https://", FieldState.Valid, FieldState.Invalid)]
        [InlineData("R 3.3.1", "A", FieldState.Valid, FieldState.Valid)]
        [InlineData("R 3.3.1", "https://abc.com", FieldState.Valid, FieldState.Valid)]
        public void InputFieldTooltips(string name, string path, FieldState expectedNameState, FieldState expectedPathState) {
            string nameTooltip = null;
            string pathTooltip = null;

            switch (expectedNameState) {
                case FieldState.Missing: nameTooltip = Resources.ConnectionManager_ShouldHaveName; break;
                case FieldState.Invalid: nameTooltip = Resources.ConnectionManager_InvalidName; break;
            }
            switch (expectedPathState) {
                case FieldState.Missing: pathTooltip = Resources.ConnectionManager_ShouldHavePath; break;
                case FieldState.Invalid: pathTooltip = Resources.ConnectionManager_InvalidPath; break;
            }
            var saveTooltip = nameTooltip ?? (pathTooltip ?? Resources.ConnectionManager_Save);

            var conn = Substitute.For<IConnection>();
            conn.Name.Returns(name);
            conn.Path.Returns(path);
            var cm = new ConnectionViewModel(conn);

            cm.NameTextBoxTooltip.Should().Be(nameTooltip);
            cm.PathTextBoxTooltip.Should().Be(pathTooltip);
            cm.SaveButtonTooltip.Should().Be(saveTooltip);

            cm.IsNameValid.Should().Be(expectedNameState == 0);
            cm.IsPathValid.Should().Be(expectedPathState == 0);
            cm.IsValid.Should().Be(cm.IsNameValid && cm.IsPathValid);
        }

        [Test]
        public void UpdatePathAndName() {
            var cm = new ConnectionViewModel(Substitute.For<IConnection>());

            // Name is updated to match the host name
            cm.Path = "server";
            cm.Name.Should().Be("server");
            cm.Path.Should().Be("server");

            // Path is completed to include default scheme and port
            cm.UpdatePath();
            cm.Name.Should().Be("server");
            cm.Path.Should().Be("https://server:5444");
        }

        [Test]
        public void UpdatePathAndNameExtraSpace() {
            var cm = new ConnectionViewModel(Substitute.For<IConnection>());

            // Name doesn't have extra spaces
            cm.Path = "server ";
            cm.Name.Should().Be("server");
            cm.Path.Should().Be("server ");

            // Path is completed to include default scheme and port
            cm.UpdatePath();
            cm.Name.Should().Be("server");
            cm.Path.Should().Be("https://server:5444");
        }

        [CompositeTest]
        [InlineData("https://server:5555", "server", "https://newserver:5555", "newserver")]  // match
        [InlineData("https://server:5555", "myserver", "https://newserver:5555", "myserver")] // mismatch
        [InlineData("https://server:5555", "serveR", "https://newserver:5555", "newserver")]  // match different case
        [InlineData("https://server:5555", "serveR", "https://server:4444", "serveR")]        // case preserved
        public void UpdateName(string originalPath, string originalName, string changedPath, string expectedUpdatedName) {
            var conn = Substitute.For<IConnection>();
            conn.Name.Returns(originalName);
            conn.Path.Returns(originalPath);

            var cm = new ConnectionViewModel(conn);

            cm.Path = changedPath;
            cm.Name.Should().Be(expectedUpdatedName);
        }

        [CompositeTest]
        [InlineData("http://host", "host")]
        [InlineData("http://HOST", "host")]
        [InlineData("https://host", "host")]
        [InlineData("http://host:5000", "host")]
        [InlineData("https://host:5100", "host")]
        [InlineData("https://HOST:5100", "host")]
        [InlineData("host", "host")]
        [InlineData("HOST", "host")]
        [InlineData("host:", "host")]
        [InlineData("HOST:", "host")]
        [InlineData("host:4000", "4000")] // host == scheme in this case and 4000 is actually a host name
        [InlineData("HOST:4000", "4000")] // host == scheme in this case and 4000 is actually a host name
        [InlineData("c:\\", "c:/")]
        [InlineData("", "")]
        public void ProposedName(string path, string expectedName) {
            ConnectionViewModel.GetProposedName(path).Should().Be(expectedName);
        }

        [CompositeTest]
        [InlineData("http://host", "https://host:5444")]
        [InlineData("http://HOST", "https://host:5444")]
        [InlineData("http://host#1234", "https://host:5444#1234")]
        [InlineData("http://HOST#1234", "https://host:5444#1234")]
        [InlineData("https://host", "https://host:5444")]
        [InlineData("https://host#1234", "https://host:5444#1234")]
        [InlineData("https://host/path", "https://host:5444/path")]
        [InlineData("https://host/path#1234", "https://host:5444/path#1234")]
        [InlineData("http://host:80", "https://host:80")]
        [InlineData("http://host:5000", "https://host:5000")]
        [InlineData("http://host:5000#1234", "https://host:5000#1234")]
        [InlineData("https://host:5100", "https://host:5100")]
        [InlineData("https://HOST:5100", "https://host:5100")]
        [InlineData("https://HOST:443", "https://host:443")]
        [InlineData("https://host:5100#1234", "https://host:5100#1234")]
        [InlineData("https://HOST:5100#1234", "https://host:5100#1234")]
        [InlineData("HOST", "https://host:5444")]
        [InlineData("host", "https://host:5444")]
        [InlineData("host:4000", "https://host:4000")]
        [InlineData("HOST:4000", "https://host:4000")]
        [InlineData("host#1234", "https://host:5444#1234")]
        [InlineData("HOST#1234", "https://host:5444#1234")]
        [InlineData("c:\\", "c:\\")]
        public void CompletePath(string original, string expected) {
            ConnectionViewModel.GetCompletePath(original).Should().Be(expected);
        }

        [Test]
        public void ConnectionTooltip() {
            var conn = Substitute.For<IConnection>();
            conn.IsRemote.Returns(true);
            conn.Path.Returns("http://host");
            var cm = new ConnectionViewModel(conn);
            cm.ConnectionTooltip.Should().Be(
                Resources.ConnectionManager_InformationTooltipFormatRemote.FormatInvariant(cm.Path, Resources.ConnectionManager_None));

            conn = Substitute.For<IConnection>();
            conn.Path.Returns("C:\\");
            cm = new ConnectionViewModel(conn);
            cm.ConnectionTooltip.Should().Be(
                Resources.ConnectionManager_InformationTooltipFormatLocal.FormatInvariant(cm.Path, Resources.ConnectionManager_None));
        }
    }
}
