// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Common.Core.OS;
using Microsoft.Win32;

namespace Microsoft.R.Containers.Windows.Test {
    internal class RegistryMock : IRegistry {

        public RegistryMock(IEnumerable<string> failPaths) {
            _failPaths = failPaths;
        }

        IEnumerable<string> _failPaths;

        public string LocalMachineHive => @"Software\Microsoft\R Tools";

        public IRegistryKey OpenBaseKey(RegistryHive hive, RegistryView view)
            => new RegistryKeyMock(RegistryKey.OpenBaseKey(hive, view), _failPaths);
    }
}
