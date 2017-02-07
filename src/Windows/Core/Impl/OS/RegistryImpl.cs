// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.OS;
using Microsoft.Win32;

namespace Microsoft.Windows.Core.OS {
    public sealed class RegistryImpl : IRegistry {
        public IRegistryKey OpenBaseKey(RegistryHive hive, RegistryView view) {
            return new RegistryKeyImpl(RegistryKey.OpenBaseKey(hive, view));
        }
    }
}
