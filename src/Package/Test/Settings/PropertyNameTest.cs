// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.R.Components.Settings;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Options.Attributes;
using Microsoft.VisualStudio.R.Package.Options.R;

namespace Microsoft.VisualStudio.R.Package.Test.Settings {
    [ExcludeFromCodeCoverage]
    [Category.VsPackage.Settings]
    public sealed class PropertyNameTest {
        [Test]
        public void MatchNamesToInterface() {
            var toolsOptionsProps = typeof(RToolsOptionsPage).GetProperties();
            var ifcProps = typeof(IRSettings).GetProperties();
            foreach (var p in toolsOptionsProps) {
                if (p.GetCustomAttribute(typeof(LocCategoryAttribute)) != null) {
                    ifcProps.Should().Contain(x => x.Name.EqualsOrdinal(p.Name));
                }
            }
        }
    }
}
