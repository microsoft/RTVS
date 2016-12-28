// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using Microsoft.UnitTests.Core;

namespace Microsoft.Languages.Editor.Test.Shell {
    public static class AssemblyLocations {
        public static string EditorPath {
            get { return Path.Combine(Paths.VsRoot, @"CommonExtensions\Microsoft\Editor"); }
        }

        public static string PrivatePath {
            get { return Path.Combine(Paths.VsRoot, @"PrivateAssemblies\"); }
        }

        public static string CpsPath {
            get { return Path.Combine(Paths.VsRoot, @"CommonExtensions\Microsoft\Project"); }
        }

        public static string SharedPath {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Common Files\Microsoft Shared\MsEnv\PublicAssemblies"); }
        }
    }
}
