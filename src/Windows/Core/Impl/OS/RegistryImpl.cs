// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Win32;

namespace Microsoft.Common.Core.OS {
    public sealed class RegistryImpl : IRegistry {
        /// <summary>
        /// Hive under HKLM that can be used by the system administrator to control
        /// certain application functionality. For example, security and privacy related
        /// features such as level of logging permitted.
        /// </summary>
        public string LocalMachineHive => @"Software\Microsoft\R Tools";

        public IRegistryKey OpenBaseKey(RegistryHive hive, RegistryView view) 
            => new RegistryKeyImpl(RegistryKey.OpenBaseKey(hive, view));
    }
}
