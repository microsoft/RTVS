// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Test;

[assembly: TestFrameworkOverride]
[assembly: CpsAssemblyLoader]
[assembly: AssemblyFixtureImport(typeof(TestMainThreadFixture))]