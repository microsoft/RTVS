// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.UI.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Commands {
    internal static class CommandAsyncToOleMenuCommandShimFactory {
        public static CommandAsyncToOleMenuCommandShim CreateRCmdSetCommand(int id, IAsyncCommand command) => new CommandAsyncToOleMenuCommandShim(RGuidList.RCmdSetGuid, id, command);
        public static AsyncCommandRangeToOleMenuCommandShim CreateRCmdSetCommand(int id, IAsyncCommandRange command) => new AsyncCommandRangeToOleMenuCommandShim(RGuidList.RCmdSetGuid, id, command);
    }
}