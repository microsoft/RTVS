// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Components.PackageManager.Model;

namespace Microsoft.R.Components.Test.PackageManager {
    [ExcludeFromCodeCoverage]
    internal static class TestPackages {
        public static RPackage CreateRtvsLib1() => new RPackage {
            Package = RtvsLib1Description.Package,
            Version = RtvsLib1Description.Version,
            Depends = RtvsLib1Description.Depends,
            License = RtvsLib1Description.License,
            Built = RtvsLib1Description.Built,
            Author = RtvsLib1Description.Author,
            Title = RtvsLib1Description.Title,
            NeedsCompilation = "no",
            Description = RtvsLib1Description.DescriptionFromInstalled,
        };

        public static RPackage CreateRtvsLib1Additional() => new RPackage {
            Package = RtvsLib1Description.Package,
            Version = RtvsLib1Description.Version,
            Depends = RtvsLib1Description.Depends,
            License = RtvsLib1Description.License,
            Built = RtvsLib1Description.Built,
            Author = RtvsLib1Description.Author,
            Title = RtvsLib1Description.Title,
            NeedsCompilation = "no",
            Description = RtvsLib1Description.Description,
            Published = RtvsLib1Description.Published,
            Maintainer = RtvsLib1Description.Maintainer,
        };

        /// <summary>
        /// Contents of DESCRIPTION and index.html for rtvslib1 package.
        /// </summary>
        public static class RtvsLib1Description {
            public const string Package = "rtvslib1";
            public const string Version = "0.1.0";
            public const string Depends = "R (>= 3.2.0)";
            public const string License = "MIT";
            public const string Built = "3.2.3";
            public const string Author = "RTVS Team";
            public const string Title = "The title for rtvslib1";
            public const string Description = "This is a library that is used only for testing package installation. It doesn't do anything.";
            public const string DescriptionFromInstalled = "This is a library that is used only for testing package installation.  It doesn't do anything.";
            public const string Published = "2016-03-28";
            public const string Maintainer = "RTVS Team <rtvsuserfeedback@microsoft.com>";
        }
    }
}
