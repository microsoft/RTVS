// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.R.Host.Broker.Pipes {
    internal static class ProcessHelpers {
        public static int StartProcessAsUser(IIdentity user, string applicationName, string commandLine, string workingDirectory, out Stream stdin, out Stream stdout) {
            var winUser = user as WindowsIdentity;
            if (winUser == null) {
                throw new ArgumentException($"Provided identity must be a {nameof(WindowsIdentity)}", "user");
            }

            bool impersonate = false;  //WindowsIdentity.GetCurrent().User != winUser.User;
            using (impersonate ? winUser.Impersonate() : null) {
                var primaryToken = IntPtr.Zero;
                if (impersonate) {
                    IntPtr threadHandle = NativeMethods.GetCurrentThread();

                    IntPtr impersonatedToken;
                    if (!NativeMethods.OpenThreadToken(
                        threadHandle,
                        NativeMethods.TOKEN_QUERY | NativeMethods.TOKEN_DUPLICATE | NativeMethods.TOKEN_ASSIGN_PRIMARY,
                        impersonate,
                        out impersonatedToken)
                    ) {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }

                    if (!NativeMethods.DuplicateTokenEx(
                        impersonatedToken,
                        NativeMethods.TOKEN_QUERY | NativeMethods.TOKEN_DUPLICATE | NativeMethods.TOKEN_ASSIGN_PRIMARY,
                        IntPtr.Zero,
                        SECURITY_IMPERSONATION_LEVEL.SecurityDelegation,
                        TOKEN_TYPE.TokenPrimary,
                        out primaryToken)
                    ) {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }

                IntPtr stdinRead, stdinWrite, stdoutRead, stdoutWrite, tmp = IntPtr.Zero;
                try {
                    IntPtr currentProcess = NativeMethods.GetCurrentProcess();

                    var sa = default(SECURITY_ATTRIBUTES);
                    sa.nLength = Marshal.SizeOf(sa);
                    sa.bInheritHandle = true;

                    if (!NativeMethods.CreatePipe(out stdinRead, out tmp, ref sa, 0)) {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }

                    if (!NativeMethods.DuplicateHandle(currentProcess, tmp, currentProcess, out stdinWrite, 0, false, NativeMethods.DUPLICATE_SAME_ACCESS)) {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }

                    NativeMethods.CloseHandle(tmp);
                    tmp = IntPtr.Zero;

                    if (!NativeMethods.CreatePipe(out tmp, out stdoutWrite, ref sa, 0)) {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }

                    if (!NativeMethods.DuplicateHandle(currentProcess, tmp, currentProcess, out stdoutRead, 0, false, NativeMethods.DUPLICATE_SAME_ACCESS)) {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }

                    NativeMethods.CloseHandle(tmp);
                    tmp = IntPtr.Zero;
                } finally {
                    if (tmp != IntPtr.Zero) {
                        NativeMethods.CloseHandle(tmp);
                    }
                }

                //if (!NativeMethods.SetHandleInformation(stdinWrite, HANDLE_FLAGS.INHERIT, 0)) {
                //    throw new Win32Exception(Marshal.GetLastWin32Error());
                //}

                //if (!NativeMethods.SetHandleInformation(stdoutRead, HANDLE_FLAGS.INHERIT, 0)) {
                //    throw new Win32Exception(Marshal.GetLastWin32Error());
                //}

                IntPtr stderr = NativeMethods.GetStdHandle(NativeMethods.STD_ERROR_HANDLE);
                if (stderr == (IntPtr)(-1)) {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                var si = default(STARTUPINFO);
                si.cb = Marshal.SizeOf(si);
                si.dwFlags = NativeMethods.STARTF_USESTDHANDLES;
                si.hStdInput = stdinRead;
                si.hStdOutput = stdoutWrite;
                si.hStdError = stderr;

                PROCESS_INFORMATION pi;
                if (impersonate) {
                    if (!NativeMethods.CreateProcessAsUser(primaryToken, applicationName, commandLine, IntPtr.Zero, IntPtr.Zero, true, 0, IntPtr.Zero, workingDirectory, ref si, out pi)) {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                } else {
                    if (!NativeMethods.CreateProcess(applicationName, commandLine, IntPtr.Zero, IntPtr.Zero, true, 0, IntPtr.Zero, workingDirectory, ref si, out pi)) {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }

                stdin = new FileStream(new SafeFileHandle(stdinWrite, true), FileAccess.Write, 0x1000, false);
                stdout = new FileStream(new SafeFileHandle(stdoutRead, true), FileAccess.Read, 0x1000, false);
                return pi.dwProcessId;
            }
        }
    }
}
