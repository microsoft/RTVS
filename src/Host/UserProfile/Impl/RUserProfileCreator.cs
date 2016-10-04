
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Microsoft.R.Host.UserProfile {
    internal class RUserProfileCreator {

        internal static IRUserProfileCreatorResult Create(string username, string domain, string password) {
            IntPtr token;
            IRUserProfileCreatorResult result = null;
            if (LogonUser(username, domain, password, (int)LogonType.LOGON32_LOGON_NETWORK, (int)LogonProvider.LOGON32_PROVIDER_DEFAULT, out token)) {
                WindowsIdentity winIdentity = new WindowsIdentity(token);
                StringBuilder profileDir = new StringBuilder(MAX_PATH);
                uint size = (uint)profileDir.Capacity;

                bool profileExists = false;
                uint error = CreateProfile(winIdentity.User.Value, username, profileDir, size);
                // 0x800700b7 - Profile already exists.
                if (error != 0 && error != 0x800700b7) {
                    result = new RUserProfileCreatorResult(null, error, profileExists);
                } else if (error == 0x800700b7) {
                    profileExists = true;
                }

                profileDir = new StringBuilder(MAX_PATH * 2);
                size = (uint)profileDir.Capacity;
                if (GetUserProfileDirectory(token, profileDir, ref size)) {
                    result = new RUserProfileCreatorResult(profileDir.ToString(), 0, profileExists);
                } else {
                    result = new RUserProfileCreatorResult(profileDir.ToString(), (uint)Marshal.GetLastWin32Error(), profileExists);
                }
            } else {
                result = new RUserProfileCreatorResult(null, (uint)Marshal.GetLastWin32Error(), false);
            }

            if(token != IntPtr.Zero) {
                CloseHandle(token);
            }

            return result;
        }

        const int MAX_PATH = 260;

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool LogonUser(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            out IntPtr phToken);

        [DllImport("userenv.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool GetUserProfileDirectory(
            IntPtr hToken,
            StringBuilder pszProfilePath,
            ref uint cchProfilePath);

        [DllImport("userenv.dll", CharSet = CharSet.Auto)]
        static extern uint CreateProfile(
            [MarshalAs(UnmanagedType.LPWStr)] string pszUserSid,
            [MarshalAs(UnmanagedType.LPWStr)] string pszUserName,
            [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszProfilePath,
            uint cchProfilePath);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);
        enum LogonType : int {
            LOGON32_LOGON_INTERACTIVE = 2,
            LOGON32_LOGON_NETWORK = 3,
            LOGON32_LOGON_BATCH = 4,
            LOGON32_LOGON_SERVICE = 5,
            LOGON32_LOGON_UNLOCK = 7,
            LOGON32_LOGON_NETWORK_CLEARTEXT = 8,
            LOGON32_LOGON_NEW_CREDENTIALS = 9,
        }

        enum LogonProvider : int {
            LOGON32_PROVIDER_DEFAULT = 0,
            LOGON32_PROVIDER_WINNT35 = 1,
            LOGON32_PROVIDER_WINNT40 = 2,
            LOGON32_PROVIDER_WINNT50 = 3
        }
    }
}
