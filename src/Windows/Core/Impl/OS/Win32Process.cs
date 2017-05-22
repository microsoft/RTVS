// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Common.Core.OS {
    public class Win32Process : IProcess {
        private static readonly object _createProcessLock = new object();

        private readonly SafeProcessHandle _processHandle;
        private readonly RegisteredWaitHandle _registeredWait;
        private readonly object _exitCodeLock;
        private bool _hasExited;
        private uint _exitCode;

        public int Id { get; }
        public int MainThreadId { get; }
        public Stream StandardInput { get; }
        public Stream StandardOutput { get; }
        public Stream StandardError { get; }
        public bool HasExited => _hasExited;
        public int ExitCode => (int)_exitCode;

        public event EventHandler Exited;

        private Win32Process(NativeMethods.PROCESS_INFORMATION pi, Stream stdin, Stream stdout, Stream stderror) {
            StandardInput = stdin;
            StandardOutput = stdout;
            StandardError = stderror;
            _hasExited = false;
            _exitCodeLock = new object();
            Id = pi.dwProcessId;
            MainThreadId = pi.dwThreadId;
            _processHandle = new SafeProcessHandle(pi.hProcess, true);

            var threadHandle = new SafeThreadHandle(pi.hThread);
            var wait = new ProcessWaitHandle(_processHandle);
            _registeredWait = ThreadPool.RegisterWaitForSingleObject(wait, (o, t) => {
                _registeredWait.Unregister(wait);
                SetExitState();
                Exited?.Invoke(this, EventArgs.Empty);
                _processHandle.Close();
                threadHandle.Close();
                wait.Close();
            }, null, -1, true);
        }

        public void WaitForExit(int milliseconds) {
            using (var processWaitHandle = new ProcessWaitHandle(_processHandle)) {
                if (processWaitHandle.WaitOne(milliseconds)) {
                    // This means the process exited while waiting.
                    SetExitState();
                }
            }
        }

        public void Kill() {
            if (!_processHandle.IsClosed) {
                if(!NativeMethods.TerminateProcess(_processHandle, IntPtr.Zero)) {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
        }

        private void SetExitState() {
            lock (_exitCodeLock) {
                if (!_hasExited) {
                    _hasExited = true;
                    if (!NativeMethods.GetExitCodeProcess(_processHandle, out _exitCode)) {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }
            }
        }

        public static Win32Process StartProcessAsUser(WindowsIdentity winIdentity, string applicationName, string commandLine, string workingDirectory, Win32NativeEnvironmentBlock environment) {

            var si = new NativeMethods.STARTUPINFO {
                cb = Marshal.SizeOf(typeof(NativeMethods.STARTUPINFO)),
                lpDesktop = ""
            };

            /* 
            When a process is started using CreateProcessAsUser function, the process will be started into a windowstation 
            and desktop combination based on the value of lpDesktop in the STARTUPINFO structure parameter:
            lpDesktop = "<windowsta>\<desktop>"; the system will try to start the process into that windowstation and desktop.
            lpDesktop = NULL; the system will try to use the same windowstation and desktop as the calling process if the system is associated with the interactive windowstation.
            lpDesktop = <somevalue>; the system will create a new windowstation and desktop that you cannot see.
            lpDesktop = ""; it will either create a new windowstation and desktop that you cannot see, or if one has been created by means of a prior call by using the same access token, the existing windowstation and desktop will be used.
            */

            var sa = default(NativeMethods.SECURITY_ATTRIBUTES);
            sa.nLength = Marshal.SizeOf(sa);
            sa.lpSecurityDescriptor = IntPtr.Zero;
            sa.bInheritHandle = true;

            if (!NativeMethods.CreatePipe(out var stdinRead, out var stdinWrite, ref sa, 0)) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            if (!NativeMethods.SetHandleInformation(stdinWrite, NativeMethods.HANDLE_FLAGS.INHERIT, NativeMethods.HANDLE_FLAGS.None)) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            if (!NativeMethods.CreatePipe(out var stdoutRead, out var stdoutWrite, ref sa, 0)) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            if (!NativeMethods.SetHandleInformation(stdoutRead, NativeMethods.HANDLE_FLAGS.INHERIT, NativeMethods.HANDLE_FLAGS.None)) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            if (!NativeMethods.CreatePipe(out var stderrorRead, out var stderrorWrite, ref sa, 0)) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            if (!NativeMethods.SetHandleInformation(stderrorRead, NativeMethods.HANDLE_FLAGS.INHERIT, NativeMethods.HANDLE_FLAGS.None)) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            si.dwFlags = NativeMethods.STARTF_USESTDHANDLES;
            si.hStdInput = stdinRead;
            si.hStdOutput = stdoutWrite;
            si.hStdError = stderrorWrite;

            var processAttr = CreateSecurityAttributes(winIdentity == null ? WellKnownSidType.AuthenticatedUserSid : WellKnownSidType.NetworkServiceSid);
            var threadAttr = CreateSecurityAttributes(winIdentity == null ? WellKnownSidType.AuthenticatedUserSid : WellKnownSidType.NetworkServiceSid);

            lock (_createProcessLock) {
                var oldErrorMode = NativeMethods.SetErrorMode(NativeMethods.ErrorModes.SEM_FAILCRITICALERRORS);
                try {
                    NativeMethods.PROCESS_INFORMATION pi;
                    if (winIdentity == null) {
                        if (!NativeMethods.CreateProcess(applicationName, commandLine, ref processAttr, ref threadAttr, true,
                            (uint)(NativeMethods.CREATE_PROCESS_FLAGS.CREATE_UNICODE_ENVIRONMENT | NativeMethods.CREATE_PROCESS_FLAGS.CREATE_NO_WINDOW),
                            environment.NativeEnvironmentBlock,
                            workingDirectory,
                            ref si,
                            out pi)) {
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                        }
                    } else {
                        if (!NativeMethods.CreateProcessAsUser(
                            winIdentity.Token, applicationName, commandLine, ref processAttr, ref threadAttr, true,
                            (uint)(NativeMethods.CREATE_PROCESS_FLAGS.CREATE_UNICODE_ENVIRONMENT | NativeMethods.CREATE_PROCESS_FLAGS.CREATE_NO_WINDOW),
                            environment.NativeEnvironmentBlock,
                            workingDirectory,
                            ref si,
                            out pi)) {
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                        }
                    }

                    var stdin = new FileStream(new SafeFileHandle(stdinWrite, true), FileAccess.Write, 0x1000, false);
                    var stdout = new FileStream(new SafeFileHandle(stdoutRead, true), FileAccess.Read, 0x1000, false);
                    var stderror = new FileStream(new SafeFileHandle(stderrorRead, true), FileAccess.Read, 0x1000, false);

                    return new Win32Process(pi, stdin, stdout, stderror);
                } finally {
                    NativeMethods.SetErrorMode(oldErrorMode);

                    if (processAttr.lpSecurityDescriptor != IntPtr.Zero) {
                        Marshal.FreeHGlobal(processAttr.lpSecurityDescriptor);
                    }

                    if (threadAttr.lpSecurityDescriptor != IntPtr.Zero) {
                        Marshal.FreeHGlobal(threadAttr.lpSecurityDescriptor);
                    }
                }
            }
        }

        private static NativeMethods.SECURITY_ATTRIBUTES CreateSecurityAttributes(WellKnownSidType sidType) {
            // Grant access to Network Service.
            var networkService = new SecurityIdentifier(sidType, null);
            var dacl = new DiscretionaryAcl(false, false, 1);
            dacl.AddAccess(AccessControlType.Allow, networkService, -1, InheritanceFlags.None, PropagationFlags.None);
            var csd = new CommonSecurityDescriptor(false, false, ControlFlags.DiscretionaryAclPresent | ControlFlags.OwnerDefaulted | ControlFlags.GroupDefaulted, null, null, null, dacl);

            var buffer = new byte[csd.BinaryLength];
            csd.GetBinaryForm(buffer, 0);

            var dest = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, dest, buffer.Length);

            var sa = new NativeMethods.SECURITY_ATTRIBUTES();
            sa.nLength = Marshal.SizeOf(sa);
            sa.lpSecurityDescriptor = dest;

            return sa;
        }
    }
}
