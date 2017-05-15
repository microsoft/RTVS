// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using static System.FormattableString;
using static Microsoft.Common.Core.NativeMethods;


namespace Microsoft.Common.Core.OS {
    public class Win32EnvironmentBlock : IEnumerable<KeyValuePair<string, string>> {
        private readonly ConcurrentDictionary<string, string> _environment;

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _environment.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _environment.GetEnumerator();

        public string this[string key] {
            get => _environment[key];
            set => _environment[key] = value;
        }

        private Win32EnvironmentBlock() {
            _environment = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public static Win32EnvironmentBlock Create(IntPtr token, bool inherit = false) {
            var env = IntPtr.Zero;
            var eb = new Win32EnvironmentBlock();
            string[] delimiter = { "=" };
            try {
                if (CreateEnvironmentBlock(out env, token, inherit)) {
                    var ptr = env;
                    while (true) {
                        var envVar = Marshal.PtrToStringUni(ptr);
                        var data = Encoding.Unicode.GetBytes(envVar);
                        if (data.Length <= 2) {
                            // detected double null
                            break;
                        } else {
                            var envVarParts = envVar.Split(delimiter, 2, StringSplitOptions.RemoveEmptyEntries);
                            if (envVarParts.Length == 2 && !string.IsNullOrWhiteSpace(envVarParts[0])) {
                                var key = envVarParts[0];
                                var value = envVarParts[1];
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
            using (var ms = new MemoryStream()) {
                byte[] nulls = { 0, 0 };
                foreach (var p in _environment.ToArray()) {
                    var envData = Invariant($"{p.Key}={p.Value}");
                    var data = Encoding.Unicode.GetBytes(envData);
                    ms.Write(data, 0, data.Length);
                    ms.Write(nulls, 0, nulls.Length);
                }
                ms.Write(nulls, 0, nulls.Length);
                return ms.ToArray();
            }
        }

        public Win32NativeEnvironmentBlock GetNativeEnvironmentBlock() => Win32NativeEnvironmentBlock.Create(ToByteArray());
    }
}
