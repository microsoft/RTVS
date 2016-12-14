// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.References;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Test;

[assembly: TestFrameworkOverride]
#if VS14
[assembly: Dev14AssemblyLoader]
//[assembly: Dev14CpsAssemblyLoader]
#endif
#if VS15
[assembly: Dev15AssemblyLoader]
[assembly: Dev15CpsAssemblyLoader]
#endif