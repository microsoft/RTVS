// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Host.Client.Test.Fixtures;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;

[assembly: TestFrameworkOverride]
[assembly: AssemblyFixtureImport(typeof(HostClientServicesFixture), typeof(RemoteBrokerFixture), typeof(TestMainThreadFixture))]
