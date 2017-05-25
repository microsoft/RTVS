// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Microsoft.Common.Core.OS {
    public sealed class Win32MessageBox {
        ///<summary>
        /// Flags that define appearance and behaviour of a standard message box displayed by a call to the MessageBox function.
        /// </summary>    
        [Flags]
        public enum Flags : uint {
            OkOnly = 0x000000,
            OkCancel = 0x000001,
            AbortRetryIgnore = 0x000002,
            YesNoCancel = 0x000003,
            YesNo = 0x000004,
            RetryCancel = 0x000005,
            CancelTryContinue = 0x000006,
            IconHand = 0x000010,
            IconQuestion = 0x000020,
            IconExclamation = 0x000030,
            IconAsterisk = 0x000040,
            UserIcon = 0x000080,
            IconWarning = IconExclamation,
            IconError = IconHand,
            IconInformation = IconAsterisk,
            IconStop = IconHand,
            DefButton1 = 0x000000,
            DefButton2 = 0x000100,
            DefButton3 = 0x000200,
            DefButton4 = 0x000300,
            ApplicationModal = 0x000000,
            SystemModal = 0x001000,
            TaskModal = 0x002000,
            Help = 0x004000,
            NoFocus = 0x008000,
            SetForeground = 0x010000,
            DefaultDesktopOnly = 0x020000,
            Topmost = 0x040000,
            Right = 0x080000,
            RTLReading = 0x100000
        }

        public enum Result : uint {
            Ok = 1,
            Cancel = 2,
            Abort = 3,
            Retry = 4,
            Ignore = 5,
            Yes = 6,
            No = 7,
            Close = 8,
            Help = 9,
            TryAgain = 10,
            Continue = 11,
            Timeout = 32000
        }

        public static Result Show(IntPtr appWindowHandle, string message, Flags flags) {
            // Create a host form that is a TopMost window which will be the 
            // parent of the MessageBox.
            using (var form = new Form() { TopMost = true, Size = new Size(1, 1), StartPosition = FormStartPosition.Manual }) {
                // We do not want anyone to see this window so position it off the 
                // visible screen and make it as small as possible
                var rect = SystemInformation.VirtualScreen;
                form.Location = new Point(rect.Bottom + 10, rect.Right + 10);
                form.Show();
                form.Focus();
                form.BringToFront();
                // Finally show the MessageBox with the form just created as its owner
                flags |= Flags.ApplicationModal | Flags.Topmost | Flags.SetForeground;

                // Find VS window rectangle
                var vsWindowRect = new NativeMethods.RECT();
                NativeMethods.GetWindowRect(appWindowHandle, out vsWindowRect);

                // Position message box in a way so it appears over VS but above the progress window
                var centerX = vsWindowRect.Left + vsWindowRect.Width / 2;
                var centerY = vsWindowRect.Top + vsWindowRect.Height / 2;

                MoveMessageBoxAsync(Resources.MessageBoxTitle, centerX, centerY).DoNotWait();
                return (Result)NativeMethods.MessageBox(form.Handle, message, Resources.MessageBoxTitle, (uint)flags);
            }
        }

        private static Task MoveMessageBoxAsync(string title, int centerX, int centerY) {
            return Task.Run(() => {
                var msgBoxHandle = IntPtr.Zero;
                var startTime = DateTime.Now;

                while (msgBoxHandle == IntPtr.Zero && (DateTime.Now - startTime).TotalMilliseconds < 2000) {
                    msgBoxHandle = NativeMethods.FindWindow(null, title);
                }
                if (msgBoxHandle != IntPtr.Zero) {
                    var bottom = 0;

                    var progressBoxHandle = NativeMethods.FindWindow("#32770", "Microsoft Visual Studio");
                    if (progressBoxHandle != IntPtr.Zero) {
                        var rcProgress = new NativeMethods.RECT();
                        NativeMethods.GetWindowRect(progressBoxHandle, out rcProgress);
                        bottom = rcProgress.Top - 20;
                    }

                    var rc = new NativeMethods.RECT();
                    NativeMethods.GetWindowRect(msgBoxHandle, out rc);
                    var x = centerX - rc.Width / 2;
                    var y = centerY - rc.Height / 2;
                    if (bottom > 0) {
                        y = bottom - rc.Height;
                    }
                    NativeMethods.MoveWindow(msgBoxHandle, x, y, rc.Width, rc.Height, true);
                }
            });
        }
    }
}
