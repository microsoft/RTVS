// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Shell;
using NSubstitute;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.DataInspect {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]   // required for tests using R Host 
    public class ReplCommandsTest {
        [Test]
        [Category.Variable.Explorer]
        public async Task ViewLibraryTest() {
            var provider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            var cb = Substitute.For<IRSessionCallback>();
            cb.ViewLibrary().Returns(Task.CompletedTask);
            using (var hostScript = new RHostScript(provider, cb)) {
                using (var inter = await hostScript.Session.BeginInteractionAsync()) {
                    await inter.RespondAsync("library()" + Environment.NewLine);
                }
            }
            await cb.Received().ViewLibrary();
        }
    }
}
