// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using static System.FormattableString;
using static Microsoft.Common.Core.NativeMethods;


namespace Microsoft.Common.Core.OS {
    public class Win32EnvironmentBlock {

        private ConcurrentDictionary<string, string> _environment;

        public string this[string key] {
            get {
                return _environment[key];
            }

            set {
                _environment[key] = value;
            }
        }

        private Win32EnvironmentBlock() {
            _environment = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public static Win32EnvironmentBlock Create(IntPtr token, bool inherit = false) {
            IntPtr env = IntPtr.Zero;
            Win32EnvironmentBlock eb = new Win32EnvironmentBlock();
            string[] delimiter = { "=" };
            try {
                if (CreateEnvironmentBlock(out env, token, inherit)) {
                    IntPtr ptr = env;
                    while (true) {
                        string envVar = Marshal.PtrToStringUni(ptr);
                        byte[] data = Encoding.Unicode.GetBytes(envVar);
                        if (data.Length <= 2) {
                            // detected double null
                            break;
                        } else {
                            string[] envVarParts = envVar.Split(delimiter, 2, StringSplitOptions.RemoveEmptyEntries);
                            if (envVarParts.Length == 2 && !string.IsNullOrWhiteSpace(envVarParts[0])) {
                                string key = envVarParts[0];
                                string value = envVarParts[1];
                                eb._environment.AddOrUpdate(key, value, (k, v) => value);
                            }
                        }
                        // unicode string + unicode null
                        ptr += data.Length + 2;
                    }
                }
            } finally {
                DestroyEnvironmentBlock(env);
            }
            return eb;
        }

        private byte[] ToByteArray() {
            using (MemoryStream ms = new MemoryStream()) {
                byte[] nulls = { 0, 0 };
                foreach (var p in _environment.ToArray()) {
                    string envData = Invariant($"{p.Key}={p.Value}");
                    byte[] data = Encoding.Unicode.GetBytes(envData);
                    ms.Write(data, 0, data.Length);
                    ms.Write(nulls, 0, nulls.Length);
                }
                ms.Write(nulls, 0, nulls.Length);
                return ms.ToArray();
            }
        }

        public Win32NativeEnvironmentBlock GetNativeEnvironmentBlock() {
            return Win32NativeEnvironmentBlock.Create(ToByteArray());
        }
    }
}
