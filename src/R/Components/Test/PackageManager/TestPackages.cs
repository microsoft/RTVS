// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Components.Test.PackageManager {
    internal class TestPackages {
        /// <summary>
        /// Contents of DESCRIPTION for rtvslib1 package.
        /// </summary>
        public class RtvsLib1 {
            public const string Package = "rtvslib1";
            public const string Version = "0.1.0";
            public const string Depends = "R (>= 3.2.0)";
            public const string License = "MIT";
            public const string Built = "3.2.3";
            public const string Author = "RTVS Team";
            public const string Title = "The title for rtvslib1";
        }
    }
}
