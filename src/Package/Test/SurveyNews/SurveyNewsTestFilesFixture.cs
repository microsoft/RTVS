// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.VisualStudio.R.Package.Test.SurveyNews {
    [ExcludeFromCodeCoverage]
    [AssemblyFixture]
    public class SurveyNewsTestFilesFixture : DeployFilesFixture {
        public SurveyNewsTestFilesFixture() : base(@"Package\Test\SurveyNews\Feeds", @"SurveyNews\Feeds") { }
    }
}
