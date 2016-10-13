// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Microsoft.R.Host.Broker {
    public class Win32Process {
        public static int StartProcessAsUser(WindowsIdentity winIdentity, string applicationName, string commandLine, string workingDirectory, Win32EnvironmentBlock environment, out Stream stdin, out Stream stdout, out Stream stderror) {

            STARTUPINFO si = new STARTUPINFO();
            si.cb = Marshal.SizeOf(typeof(STARTUPINFO));

            /*
            When a process is started by means of the CreateProcessAsUser function, the process will be started into a windowstation 
            and desktop combination based on the value of lpDesktop in the STARTUPINFO structure parameter:
            * lpDesktop = "<windowsta>\<desktop>"; the system will try to start the process into that windowstation and desktop.
            * lpDesktop = NULL; the system will try to use the same windowstation and desktop as the calling process if the system is associated with the interactive windowstation.
            * lpDesktop = <somevalue>; the system will create a new windowstation and desktop that you cannot see.
            * lpDesktop = ""; it will either create a new windowstation and desktop that you cannot see, or if one has been created by means of a prior call by using the same access token, the existing windowstation and desktop will be used.
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

            if (!SetHandleInformation(stdinWrite, HANDLE_FLAGS.INHERIT , HANDLE_FLAGS.None)) {
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

            SECURITY_ATTRIBUTES processAttr = CreateSecurityAttributes();
            SECURITY_ATTRIBUTES threadAttr = CreateSecurityAttributes();

            PROCESS_INFORMATION pi;
            if(!CreateProcessAsUser(
                winIdentity.Token, applicationName, commandLine, ref processAttr, ref threadAttr, true,
                (uint)(CREATE_PROCESS_FLAGS.CREATE_UNICODE_ENVIRONMENT | CREATE_PROCESS_FLAGS.CREATE_NO_WINDOW),
                environment.ToPtr(),
                workingDirectory,
                ref si,
                out pi)) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            stdin = new FileStream(new SafeFileHandle(stdinWrite, true), FileAccess.Write, 0x1000, false);
            stdout = new FileStream(new SafeFileHandle(stdoutRead, true), FileAccess.Read, 0x1000, false);
            stderror = new FileStream(new SafeFileHandle(stderrorRead, true), FileAccess.Read, 0x1000, false);

            // TODO: handle cleanup for process and thread
            // TODO: cleanup security attributes
            return pi.dwProcessId;
        }

        private static SECURITY_ATTRIBUTES CreateSecurityAttributes() {
            // Grant access to Network Service.
            SecurityIdentifier networkService = new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null);
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

        const int MAX_PATH = 260;

        const int STARTF_USESTDHANDLES = 0x00000100;
        const int STD_INPUT_HANDLE = -10;
        const int STD_OUTPUT_HANDLE = -11;
        const int STD_ERROR_HANDLE = -12;


        [Flags]
        enum HANDLE_FLAGS : uint {
            None = 0,
            INHERIT = 1,
            PROTECT_FROM_CLOSE = 2
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetHandleInformation(IntPtr hObject, HANDLE_FLAGS dwMask, HANDLE_FLAGS dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        enum CREATE_PROCESS_FLAGS : uint {
            CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
            CREATE_NEW_CONSOLE = 0x00000010,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_NO_WINDOW = 0x08000000,
            CREATE_PROTECTED_PROCESS = 0x00040000,
            CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
            CREATE_SEPARATE_WOW_VDM = 0x00000800,
            CREATE_SHARED_WOW_VDM = 0x00001000,
            CREATE_SUSPENDED = 0x00000004,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            DEBUG_ONLY_THIS_PROCESS = 0x00000002,
            DEBUG_PROCESS = 0x00000001,
            DETACHED_PROCESS = 0x00000008,
            EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
            INHERIT_PARENT_AFFINITY = 0x00010000,
        }

        [DllImport("kernel32.dll")]
        static extern bool CreatePipe(
            out IntPtr hReadPipe,
            out IntPtr hWritePipe,
            ref SECURITY_ATTRIBUTES lpPipeAttributes,
            uint nSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hHandle);

        [Flags]
        enum DuplicateOptions : uint {
            DUPLICATE_CLOSE_SOURCE = (0x00000001),// Closes the source handle. This occurs regardless of any error status returned.
            DUPLICATE_SAME_ACCESS = (0x00000002), //Ignores the dwDesiredAccess parameter. The duplicate handle has the same access as the source handle.
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DuplicateHandle(
            IntPtr hSourceProcessHandle,
            IntPtr hSourceHandle,
            IntPtr hTargetProcessHandle,
            out IntPtr lpTargetHandle,
            uint dwDesiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
            uint dwOptions);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetCurrentProcess();

        [StructLayout(LayoutKind.Sequential)]
        struct SECURITY_ATTRIBUTES {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct PROCESS_INFORMATION {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFO {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool CreateProcessAsUser(
            IntPtr hToken,
            string lpApplicationName,
            string lpCommandLine,
            ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);
    }
}
