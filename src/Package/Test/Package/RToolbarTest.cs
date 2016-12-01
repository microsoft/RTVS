// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using EnvDTE80;
using FluentAssertions;
using Microsoft.R.Support.Settings;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.R.Package.Packages;
using NSubstitute;
using Microsoft.VisualStudio.Shell.Mocks;

namespace Microsoft.VisualStudio.R.Package.Test.Package {
    [ExcludeFromCodeCoverage]
    public sealed class RToolbarTest {
        [Test]
        public void Visibility() {
            var settings = Substitute.For<IRToolsSettings>();
            // NSub does not work with dynamic, see "https://github.com/nsubstitute/NSubstitute/issues/143"
            var dte = new DteMock();
            var cbs = Substitute.For<CommandBars.CommandBars>();
            dte.CommandBars = cbs;

            var cb = Substitute.For<CommandBar>();
            cbs["R Toolbar"].Returns(cb);
            var tb = new RToolbar(dte, settings);
            tb.Show();
            cb.Visible.Should().BeFalse();

            settings.ShowRToolbar.Returns(true);
            tb.Show();
            cb.Visible.Should().BeTrue();

            tb.SaveState();
            settings.ShowRToolbar.Should().BeTrue();

            cb.Visible = false;
            tb.SaveState();
            settings.ShowRToolbar.Should().BeFalse();
        }
    }
}
