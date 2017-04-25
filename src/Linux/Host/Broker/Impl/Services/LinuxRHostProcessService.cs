// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using Microsoft.Common.Core.OS;
using Microsoft.R.Host.Broker.Interpreters;

namespace Microsoft.R.Host.Broker.Services {
    class LinuxRHostProcessService : IRHostProcessService {
        public IProcess StartHost(Interpreter interpreter, string profilePath, string userName, WindowsIdentity useridentity, string commandLine) {
            throw new NotImplementedException();
        }
    }
}