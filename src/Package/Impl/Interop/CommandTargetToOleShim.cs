// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Interop {
    /// <summary>
    /// Implements OLE command target over ICommandTarget allowing managed components
    /// to participate in OLE/COM environment like Visual Studio.
    /// While a command is executing, the current variant arguments are cached.
    /// </summary>
    public sealed class CommandTargetToOleShim : IOleCommandTarget, ICommandTarget {
        private readonly ICommandTarget _commandTarget;
        private readonly CommandTargetToOleShimVariantStacks _variantStacks;

        public CommandTargetToOleShim(ITextView textView, ICommandTarget commandTarget) {
            _commandTarget = commandTarget;
            if (textView != null) {
                _variantStacks = CommandTargetToOleShimVariantStacks.EnsureConnected(textView);
            }
        }

        #region IOleCommandTarget

        public int QueryStatus(ref Guid guidCommandGroup, uint commandCount, OLECMD[] commandArray, IntPtr commandText) {
            var status = _commandTarget.Status(guidCommandGroup, (int)commandArray[0].cmdID);
            return OleCommand.MakeOleCommandStatus(status, commandArray);
        }

        public int Exec(ref Guid guidCommandGroup, uint commandID, uint commandExecOpt, IntPtr variantIn, IntPtr variantOut) {
            // Cache the variants, they'll be needed if the command chain
            // goes back into another IOleCommandTarget (see CommandTargetToOleShim)
            CommandResult result;

            try {
                _variantStacks?.Push(variantIn, variantOut, false);

                var inputArg = TranslateInputArg(ref guidCommandGroup, commandID, variantIn);

                object outputArg = null;
                result = _commandTarget.Invoke(guidCommandGroup, (int)commandID, inputArg, ref outputArg);

                if (outputArg != null && variantOut != IntPtr.Zero) {
                    Marshal.GetNativeVariantForObject(outputArg, variantOut);
                }
            } catch (Exception) {
                // Some ICommandTarget object is throwing an exception, which will propagate out as a failed hr, which
                //   isn't particularly easy to track down. Put a bit of exception information in the activity log.
                //ActivityLog.LogInformation("R Tools", exc.ToString());

                //string logPath = ActivityLog.LogFilePath; // This actually flushes the activity log buffer

                throw;
            } finally {
                _variantStacks?.Pop();
            }

            return OleCommand.MakeOleResult(result);
        }

        #endregion

        private object TranslateInputArg(ref Guid guidCommandGroup, uint commandID, IntPtr variantIn) {
            object inputArg = null;

            if (variantIn != IntPtr.Zero) {
                if ((commandID == (int)VSConstants.VSStd2KCmdID.SHOWCONTEXTMENU) && (guidCommandGroup == VSConstants.VSStd2K)) {
                    inputArg = GetShortPositionFromInputArg(variantIn);
                } else {
                    inputArg = Marshal.GetObjectForNativeVariant(variantIn);
                }
            }

            return inputArg;
        }

        // Blatantly copied from core editor's handling of VSStd2KCmdID.SHOWCONTEXTMENU
        POINTS[] GetShortPositionFromInputArg(IntPtr location) {
            POINTS[] position = null;

            //the coordinates are passed as variants containing short values. The y coordinate is an offset sizeof(variant)
            //from pvaIn (which is 16 bytes)
            var xCoordinateVariant = Marshal.GetObjectForNativeVariant(location);
            var yCoordinateVariant = Marshal.GetObjectForNativeVariant(new IntPtr(location.ToInt32() + 16));
            var xCoordinate = xCoordinateVariant as short?;
            var yCoordinate = yCoordinateVariant as short?;
            Debug.Assert(xCoordinate.HasValue, "Couldn't parse the provided x coordinate for show context command");
            Debug.Assert(yCoordinate.HasValue, "Couldn't parse the provided y coordinate for show context command");
            if (xCoordinate.HasValue && yCoordinate.HasValue) {
                position = new POINTS[1];
                position[0].x = xCoordinate.Value;
                position[0].y = yCoordinate.Value;
            }

            return position;
        }

        #region ICommandTarget
        public CommandStatus Status(Guid group, int id) => _commandTarget.Status(@group, id);
        public CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg)
            => Invoke(_commandTarget, @group, id, inputArg, ref outputArg);

        public CommandResult Invoke(ICommandTarget commandTarget, Guid group, int id, object inputArg, ref object outputArg) {
            CommandResult result;
            try {
                _variantStacks?.Push(IntPtr.Zero, IntPtr.Zero, true);
                result = commandTarget.Invoke(group, id, inputArg, ref outputArg);
            } finally {
                _variantStacks?.Pop();
            }
            return result;
        }

        public void PostProcessInvoke(CommandResult result, Guid group, int id, object inputArg, ref object outputArg)
            => _commandTarget.PostProcessInvoke(result, group, id, inputArg, ref outputArg);
        #endregion
    }
}
