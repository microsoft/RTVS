// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.Security;
using System.Diagnostics;
using System.Text;
using System.Collections.Specialized;

namespace Microsoft.R.Host.Broker.Pipes {
    internal static class ProcessHelpers {
        public static int StartProcessAsUser(
            string username,
            string domain,
            string password,
            string applicationName,
            string commandLineArgs,
            Dictionary<string,string> environment,
            string workingDirectory,
            out Stream stdin, 
            out Stream stdout) {

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

            

            string envStr = null;
            {
                byte[] envBytes = null;

                using (MemoryStream ms = new MemoryStream())
                using (StreamWriter w = new StreamWriter(ms)) {
                    w.Flush();
                    ms.Position = 0; //Skip any byte order marks to identify the encoding
                    foreach (string k in environment.Keys) {
                        w.Write(k.ToCharArray());
                        w.Write('=');
                        w.Write(environment[k].ToCharArray());
                        w.Write((char)0); // environment variable separator
                    }
                    w.Write((char)0); // double null
                    w.Write((char)0);
                    w.Flush();
                    ms.Flush();
                    envBytes = ms.ToArray();
                }

                char[] envChar = new char[envBytes.Length];

                int i = 0;
                foreach (byte b in envBytes) {
                    envChar[i++] = (char)b;
                }
                envStr = new string(envChar, 0, envChar.Length);
            }

            PROCESS_INFORMATION pi;
            if (!NativeMethods.CreateProcessWithLogonW(
                username, 
                domain, 
                password, 
                LogonFlags.LOGON_WITH_PROFILE, 
                applicationName, 
                commandLineArgs, 
                CreationFlags.CREATE_DEFAULT_ERROR_MODE | CreationFlags.CREATE_NEW_CONSOLE | CreationFlags.CREATE_UNICODE_ENVIRONMENT,
                envStr,
                workingDirectory, 
                ref si, 
                out pi)) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            stdin = new FileStream(new SafeFileHandle(stdinWrite, true), FileAccess.Write, 0x1000, false);
            stdout = new FileStream(new SafeFileHandle(stdoutRead, true), FileAccess.Read, 0x1000, false);
            return pi.dwProcessId;
        }

    }
}
