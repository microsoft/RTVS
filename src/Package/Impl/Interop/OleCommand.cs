// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.UI.Commands;
using Microsoft.VisualStudio.OLE.Interop;

namespace Microsoft.VisualStudio.R.Package.Interop {
    public static class OleCommand {
        /// <summary>
        /// Converts CommandStatus to OLE command status
        /// </summary>
        /// <param name="commandStatus">Command status</param>
        /// <param name="commands">OLE command flags array</param>
        /// <returns>OLE command status</returns>
        public static int MakeOleCommandStatus(CommandStatus commandStatus, OLECMD[] commands) {
            if (commandStatus == CommandStatus.NotSupported) {
                commands[0].cmdf = 0;
                return (int)Constants.OLECMDERR_E_NOTSUPPORTED;
            }

            if ((commandStatus & CommandStatus.Invisible) == CommandStatus.Invisible) {
                commands[0].cmdf |= (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE);
            } else if ((commandStatus & CommandStatus.Supported) == CommandStatus.Supported) {
                commands[0].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

                if ((commandStatus & CommandStatus.Enabled) == CommandStatus.Enabled) {
                    commands[0].cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;
                }

                if ((commandStatus & CommandStatus.Latched) == CommandStatus.Latched) {
                    commands[0].cmdf |= (uint)OLECMDF.OLECMDF_LATCHED;
                }
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Converts CommandResult to OLE return code
        /// </summary>
        /// <param name="commandResult">Command result</param>
        /// <returns>OLE return code</returns>
        public static int MakeOleResult(CommandResult commandResult) {
            if ((commandResult.Status & CommandStatus.Supported) != 0) {
                return commandResult.Result;
            }

            return (int)Constants.OLECMDERR_E_NOTSUPPORTED;
        }

        /// <summary>
        /// Converts OLE result and command status to CommandStatus
        /// </summary>
        /// <param name="oleResult">OLE result</param>
        /// <param name="oleCommandFlags">OLE command state flags</param>
        /// <returns>Command status</returns>
        public static CommandStatus MakeCommandStatus(int oleResult, uint oleCommandFlags) {
            var cs = CommandStatus.NotSupported;
            var oleFlags = (OLECMDF)oleCommandFlags;

            if (oleResult != (int)Constants.OLECMDERR_E_NOTSUPPORTED) {
                if ((oleFlags & OLECMDF.OLECMDF_SUPPORTED) == OLECMDF.OLECMDF_SUPPORTED) {
                    cs |= CommandStatus.Supported;
                }

                if ((oleFlags & OLECMDF.OLECMDF_ENABLED) == OLECMDF.OLECMDF_ENABLED) {
                    cs |= CommandStatus.Enabled;
                }
                if ((oleFlags & OLECMDF.OLECMDF_INVISIBLE) == OLECMDF.OLECMDF_INVISIBLE) {
                    cs |= CommandStatus.Invisible;
                }
                if ((oleFlags & OLECMDF.OLECMDF_LATCHED) == OLECMDF.OLECMDF_LATCHED) {
                    cs |= CommandStatus.Latched;
                }
            }
            return cs;
        }

        public static CommandResult MakeCommandResult(int oleResult) {
            if (oleResult == (int)Constants.OLECMDERR_E_NOTSUPPORTED) {
                return new CommandResult(CommandStatus.NotSupported, 0);
            }
            return new CommandResult(CommandStatus.Supported, oleResult);
        }
    }
}
