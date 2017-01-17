// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;
using System.Threading;
using static Microsoft.Common.Core.NativeMethods;

namespace Microsoft.Common.Core.OS {
    public class Win32Process {
        private Win32Process(PROCESS_INFORMATION pi) {
            _hasExited = false;
            ProcessId = pi.dwProcessId;
            MainThreadId = pi.dwThreadId;
            _processHandle = new SafeProcessHandle(pi.hProcess, true);
            _threadHandle = new SafeThreadHandle(pi.hThread);
            _wait = new ProcessWaitHandle(_processHandle);
            _registeredWait = ThreadPool.RegisterWaitForSingleObject(_wait, (o, t) => {
                _registeredWait.Unregister(_wait);
                _hasExited = true;
                if (!GetExitCodeProcess(_processHandle, out _exitCode)) {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                Exited?.Invoke(this, null);
                _processHandle.Close();
                _threadHandle.Close();
                _wait.Close();
            }, null, -1, true);
        }

        private SafeProcessHandle _processHandle;
        private SafeThreadHandle _threadHandle;
        private ProcessWaitHandle _wait;
        private RegisteredWaitHandle _registeredWait;
        private bool _hasExited;
        private uint _exitCode;

        public readonly int ProcessId;
        public readonly int MainThreadId;

        public bool HasExited => _hasExited;

        public event EventHandler Exited;

        public int ExitCode =>  (int)_exitCode;

        public void WaitForExit(int milliseconds) {
            ProcessWaitHandle processWaitHandle = new ProcessWaitHandle(_processHandle);
            processWaitHandle.WaitOne(milliseconds);
            processWaitHandle.Close();
        }

        public void Kill() {
            if (!_processHandle.IsClosed) {
                if(!TerminateProcess(_processHandle, IntPtr.Zero)) {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
        }

        public static Win32Process StartProcessAsUser(WindowsIdentity winIdentity, string applicationName, string commandLine, string workingDirectory, Win32NativeEnvironmentBlock environment, out Stream stdin, out Stream stdout, out Stream stderror) {

            STARTUPINFO si = new STARTUPINFO();
            si.cb = Marshal.SizeOf(typeof(STARTUPINFO));

            /* 
            When a process is started using CreateProcessAsUser function, the process will be started into a windowstation 
            and desktop combination based on the value of lpDesktop in the STARTUPINFO structure parameter:
            lpDesktop = "<windowsta>\<desktop>"; the system will try to start the process into that windowstation and desktop.
            lpDesktop = NULL; the system will try to use the same windowstation and desktop as the calling process if the system is associated with the interactive windowstation.
            lpDesktop = <somevalue>; the system will create a new windowstation and desktop that you cannot see.
            lpDesktop = ""; it will either create a new windowstation and desktop that you cannot see, or if one has been created by means of a prior call by using the same access token, the existing windowstation and desktop will be used.
            */
            si.lpDesktop = "";

            IntPtr stdinRead, stdinWrite, stdoutRead, stdoutWrite, stderrorRead, stderrorWrite;

            SECURITY_ATTRIBUTES sa = default(SECURITY_ATTRIBUTES);
            sa.nLength = Marshal.SizeOf(sa);
            sa.lpSecurityDescriptor = IntPtr.Zero;
            sa.bInheritHandle = true;

            if (!CreatePipe(out stdinRead, out stdinWrite, ref sa, 0)) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            if (!SetHandleInformation(stdinWrite, HANDLE_FLAGS.INHERIT, HANDLE_FLAGS.None)) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            if (!CreatePipe(out stdoutRead, out stdoutWrite, ref sa, 0)) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            if (!SetHandleInformation(stdoutRead, HANDLE_FLAGS.INHERIT, HANDLE_FLAGS.None)) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            if (!CreatePipe(out stderrorRead, out stderrorWrite, ref sa, 0)) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            if (!SetHandleInformation(stderrorRead, HANDLE_FLAGS.INHERIT, HANDLE_FLAGS.None)) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            si.dwFlags = STARTF_USESTDHANDLES;
            si.hStdInput = stdinRead;
            si.hStdOutput = stdoutWrite;
            si.hStdError = stderrorWrite;

            SECURITY_ATTRIBUTES processAttr = CreateSecurityAttributes(winIdentity == null ? WellKnownSidType.AuthenticatedUserSid : WellKnownSidType.NetworkServiceSid);
            SECURITY_ATTRIBUTES threadAttr = CreateSecurityAttributes(winIdentity == null ? WellKnownSidType.AuthenticatedUserSid : WellKnownSidType.NetworkServiceSid);

            PROCESS_INFORMATION pi;
            try {
                if(winIdentity == null) {
                    if(!CreateProcess(applicationName, commandLine, ref processAttr, ref threadAttr, true,
                        (uint)(CREATE_PROCESS_FLAGS.CREATE_UNICODE_ENVIRONMENT | CREATE_PROCESS_FLAGS.CREATE_NO_WINDOW),
                        environment.NativeEnvironmentBlock,
                        workingDirectory,
                        ref si,
                        out pi)) {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                } else {
                    if (!CreateProcessAsUser(
                        winIdentity.Token, applicationName, commandLine, ref processAttr, ref threadAttr, true,
                        (uint)(CREATE_PROCESS_FLAGS.CREATE_UNICODE_ENVIRONMENT | CREATE_PROCESS_FLAGS.CREATE_NO_WINDOW),
                        environment.NativeEnvironmentBlock,
                        workingDirectory,
                        ref si,
                        out pi)) {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }

                stdin = new FileStream(new SafeFileHandle(stdinWrite, true), FileAccess.Write, 0x1000, false);
                stdout = new FileStream(new SafeFileHandle(stdoutRead, true), FileAccess.Read, 0x1000, false);
                stderror = new FileStream(new SafeFileHandle(stderrorRead, true), FileAccess.Read, 0x1000, false);
            } finally {
                if(processAttr.lpSecurityDescriptor != IntPtr.Zero) {
                    Marshal.FreeHGlobal(processAttr.lpSecurityDescriptor);
                }

                if (threadAttr.lpSecurityDescriptor != IntPtr.Zero) {
                    Marshal.FreeHGlobal(threadAttr.lpSecurityDescriptor);
                }
            }

            return new Win32Process(pi);
        }

        private static SECURITY_ATTRIBUTES CreateSecurityAttributes(WellKnownSidType sidType) {
            // Grant access to Network Service.
            SecurityIdentifier networkService = new SecurityIdentifier(sidType, null);
            DiscretionaryAcl dacl = new DiscretionaryAcl(false, false, 1);
            dacl.AddAccess(AccessControlType.Allow, networkService, -1, InheritanceFlags.None, PropagationFlags.None);
            CommonSecurityDescriptor csd = new CommonSecurityDescriptor(false, false, ControlFlags.DiscretionaryAclPresent | ControlFlags.OwnerDefaulted | ControlFlags.GroupDefaulted, null, null, null, dacl);

            byte[] buffer = new byte[csd.BinaryLength];
            csd.GetBinaryForm(buffer, 0);

            IntPtr dest = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, dest, buffer.Length);

            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
            sa.nLength = Marshal.SizeOf(sa);
            sa.lpSecurityDescriptor = dest;

            return sa;
        }
    }
}
