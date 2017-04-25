// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Common.Core;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Interop {
    public sealed class OleToCommandTargetShim : ICommandTarget {
        public IOleCommandTarget OleTarget { get; set; }
        private CommandTargetToOleShimVariantStacks _variantStacks;

        public OleToCommandTargetShim(ITextView textView, IOleCommandTarget oleTarget) {
            OleTarget = oleTarget;
            _variantStacks = CommandTargetToOleShimVariantStacks.EnsureConnected(textView);
        }

        #region ICommandTarget Members

        public CommandStatus Status(Guid group, int id) {
            OLECMD[] oleCmd = new OLECMD[1];

            oleCmd[0].cmdID = (uint)id;
            oleCmd[0].cmdf = 0;

            int oleStatus = OleTarget.QueryStatus(ref group, 1, oleCmd, IntPtr.Zero);

            return OleCommand.MakeCommandStatus(oleStatus, oleCmd[0].cmdf);
        }

        public CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            IntPtr variantIn = IntPtr.Zero;
            IntPtr variantOut = IntPtr.Zero;
            bool allocateVariants = false;

            var commandStatus = Status(group, id);
            if ((commandStatus & CommandStatus.SupportedAndEnabled) != CommandStatus.SupportedAndEnabled) {
                return CommandResult.NotSupported;
            }

            if (!_variantStacks.IsEmpty) {
                _variantStacks.Peek(out variantIn, out variantOut, out allocateVariants);
            } else {
                Debug.Fail("Not expecting to use OleToCommandTargetShim without a prior CommandTargetToOleShim");
            }

            if (allocateVariants) {
                // I have to allocate my own variants
                if (inputArg != null) {
                    variantIn = Marshal.AllocCoTaskMem(16);
                    Marshal.GetNativeVariantForObject(inputArg, variantIn);
                }

                if (outputArg != null) {
                    variantOut = Marshal.AllocCoTaskMem(16);
                    Marshal.GetNativeVariantForObject(outputArg, variantOut);
                }
            }

            var oleResult = 0;

            try {
                oleResult = OleTarget.Exec(ref group, (uint)id, 0, variantIn, variantOut);
                if (oleResult >= 0 && (variantOut != IntPtr.Zero)) {
                    outputArg = Marshal.GetObjectForNativeVariant(variantOut);
                }
            } finally {
                if (allocateVariants) {
                    if (variantIn != IntPtr.Zero) {
                        NativeMethods.VariantClear(variantIn);
                        Marshal.FreeCoTaskMem(variantIn);
                    }

                    if (variantOut != IntPtr.Zero) {
                        NativeMethods.VariantClear(variantOut);
                        Marshal.FreeCoTaskMem(variantOut);
                    }
                }
            }
            return OleCommand.MakeCommandResult(oleResult);
        }

        public void PostProcessInvoke(CommandResult result, Guid group, int id, object inputArg, ref object outputArg) {
        }

        #endregion
    }
}
