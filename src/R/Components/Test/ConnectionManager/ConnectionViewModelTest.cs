// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
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
            cm.Id.Should().BeNull();
            cm.Name.Should().BeNull();
        }

        [Test]
        public void Construction02() {
            var uri = new Uri("http://microsoft.com");
            var conn = Substitute.For<IConnection>();
            conn.Id.Returns(uri);
            conn.Name.Returns("name");
            conn.UserProvidedPath.Returns("path");
            conn.RCommandLineArguments.Returns("arg");
            conn.IsRemote.Returns(true);

            var cm = new ConnectionViewModel(conn);
            cm.IsUserCreated.Should().BeFalse();

            conn.IsUserCreated.Returns(true);
            cm = new ConnectionViewModel(conn);

            cm.Id.Should().Be(uri);
            conn.IsRemote.Should().BeTrue();
            cm.IsUserCreated.Should().BeTrue();
            cm.IsEditing.Should().BeFalse();
            cm.IsTestConnectionSucceeded.Should().BeFalse();
            cm.Name.Should().Be(conn.Name);
            cm.UserProvidedPath.Should().Be(conn.UserProvidedPath);
            cm.RCommandLineArguments.Should().Be(conn.RCommandLineArguments);
        }

        [Test]
        public void SaveTooltips() {
            var uri = new Uri("http://microsoft.com");
            var conn = Substitute.For<IConnection>();

            var cm = new ConnectionViewModel(conn);
            cm.SaveButtonTooltip.Should().Be(Resources.ConnectionManager_ShouldHaveName);

            conn.Name.Returns("name");
            cm = new ConnectionViewModel(conn);
            cm.SaveButtonTooltip.Should().Be(Resources.ConnectionManager_ShouldHavePath);

            conn.UserProvidedPath.Returns("c:\\path");
            cm = new ConnectionViewModel(conn);
            cm.SaveButtonTooltip.Should().Be(Resources.ConnectionManager_Save);
        }

        [CompositeTest]
        [InlineData("http://host", "http://host:80")]
        [InlineData("https://host", "https://host:443")]
        [InlineData("http://host:5000", "http://host:5000")]
        [InlineData("https://host:5100", "https://host:5100")]
        [InlineData("host", "https://host:5444")]
        [InlineData("host:4000", "host:4000")] // host == scheme in this case and 4000 is actually a host name
        [InlineData("c:\\", "c:\\")]
        [InlineData("..", "..")]
        [InlineData("~~~", "~~~")]
        public void UrlCompletion(string original, string expected) {
            var conn = Substitute.For<IConnection>();
            conn.IsRemote.Returns(true);
            conn.UserProvidedPath.Returns(original);
            var cm = new ConnectionViewModel(conn);
            cm.Path.Should().Be(expected);
        }
    }
}
