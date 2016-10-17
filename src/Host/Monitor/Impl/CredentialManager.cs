// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Extensions.Logging;

namespace Microsoft.R.Host.Monitor {
    internal class CredentialManager {
        private const string BrokerUserCredName = "RHostBrokerUserCred";

        private static object _isBrokerUserCredentialSavedLock = new object();
        public static async Task<bool> IsBrokerUserCredentialSavedAsync(ILogger logger = null) {
            await TaskUtilities.SwitchToBackgroundThread();
            lock (_isBrokerUserCredentialSavedLock) {
                
                IntPtr credPtr = IntPtr.Zero;
                bool credStatus = false;
                try {
                    if (CredRead(BrokerUserCredName, CredType.GENERIC, 0, out credPtr)) {
                        credStatus = true;
                    } else {
                        int error = Marshal.GetLastWin32Error();
                        if (error == Win32ErrorCodes.ERROR_NOT_FOUND) {
                            credStatus = false;
                            logger?.LogDebug(Resources.Info_DidNotFindSavedCredentails);
                        }
                    }
                } finally {
                    if (credPtr != IntPtr.Zero) {
                        CredFree(credPtr);
                    }
                }

                return credStatus;
            }
        }

        private const int CRED_MAX_USERNAME_LENGTH = 256;
        private const int CRED_MAX_DOMAIN_LENGTH = 256;
        private const int CRED_MAX_PASSWORD_LENGTH = 256;

        public static int MaxCredUIAttempts => 3;

        private static object _getAndSaveCredentialsFromUserLock = new object();
        public static async Task<int> GetAndSaveCredentialsFromUserAsync(ILogger logger = null) {
            await TaskUtilities.SwitchToBackgroundThread();

            lock (_getAndSaveCredentialsFromUserLock) {
                CredUIInfo credInfo = new CredUIInfo();
                credInfo.cbSize = Marshal.SizeOf(credInfo);
                credInfo.pszCaptionText = Resources.Text_RHostBrokerCredentials;
                credInfo.pszMessageText = Resources.Text_RHostBrokerCredentialsDetail;
                credInfo.hwndParent = Process.GetCurrentProcess().MainWindowHandle;

                int errorCode = 0;
                uint authenticationPackage = 0;
                IntPtr credentialBuffer = IntPtr.Zero;
                uint credentialSize;
                bool saveCreds = false;

                IntPtr token = IntPtr.Zero;

                int attempts = 0;
                try {
                    while (attempts++ < MaxCredUIAttempts) {
                        logger?.LogInformation(Resources.Info_CredAttempt, attempts);
                        uint ret = CredUIPromptForWindowsCredentials(ref credInfo, errorCode, ref authenticationPackage, IntPtr.Zero, 0, out credentialBuffer, out credentialSize, ref saveCreds, PromptForWindowsCredentialsFlags.CREDUIWIN_GENERIC);

                        // User clicked the cancel button.
                        if (ret == Win32ErrorCodes.ERROR_CANCELLED) {
                            logger?.LogInformation(Resources.Info_CredUIDismissed);
                            break;
                        } else if (ret == Win32ErrorCodes.ERROR_SUCCESS) {
                            // User entered credentials
                            logger?.LogInformation(Resources.Info_CredUICredsReceived);
                            StringBuilder username = new StringBuilder(CRED_MAX_USERNAME_LENGTH);
                            StringBuilder domain = new StringBuilder(CRED_MAX_USERNAME_LENGTH);
                            StringBuilder password = new StringBuilder(CRED_MAX_USERNAME_LENGTH);

                            int usernameLen = username.Capacity;
                            int domainLen = domain.Capacity;
                            int passwordLen = password.Capacity;

                            if (!CredUnPackAuthenticationBuffer(0, credentialBuffer, credentialSize, username, ref usernameLen, domain, ref domainLen, password, ref passwordLen)) {
                                // Do another attempt by showing user the CredUI with the errorCode set.
                                errorCode = Marshal.GetLastWin32Error();
                                logger?.LogError(Resources.Error_CredentialUnpackingFailed, errorCode);
                                if (credentialBuffer != IntPtr.Zero) {
                                    Marshal.ZeroFreeCoTaskMemUnicode(credentialBuffer);
                                }
                                continue;
                            } else {
                                logger?.LogInformation(Resources.Info_CredUnpacked);
                                // Credential buffer is no longer needed
                                if (credentialBuffer != IntPtr.Zero) {
                                    Marshal.ZeroFreeCoTaskMemUnicode(credentialBuffer);
                                }
                            }


                            var usernameBldr = new StringBuilder(CRED_MAX_USERNAME_LENGTH + 1);
                            var domainBldr = new StringBuilder(CRED_MAX_USERNAME_LENGTH + 1);

                            uint error = CredUIParseUserName(username.ToString(), usernameBldr, usernameBldr.Capacity, domainBldr, domainBldr.Capacity);
                            if (error != 0) {
                                // Couldn't parse the user name. Do another attempt by showing user the CredUI with the errorCode set.
                                errorCode = (int)error;
                                logger?.LogError(Resources.Error_UserNameParsing, errorCode);
                                continue;
                            }

                            if (LogonUser(usernameBldr.ToString(), domainBldr.ToString(), password.ToString(), (int)LogonType.LOGON32_LOGON_INTERACTIVE, (int)LogonProvider.LOGON32_PROVIDER_DEFAULT, ref token)) {
                                logger?.LogInformation(Resources.Info_LogOnAttemptSucceeded);

                                Credential creds = new Credential();
                                creds.targetName = BrokerUserCredName;
                                creds.type = (int)CredType.GENERIC;
                                creds.userName = username.ToString();
                                creds.attributeCount = 0;
                                creds.persist = (int)CredPersist.LOCAL_MACHINE;
                                byte[] bpassword = Encoding.Unicode.GetBytes(password.ToString());
                                creds.credentialBlobSize = bpassword.Length;
                                creds.credentialBlob = Marshal.StringToCoTaskMemUni(password.ToString());

                                if (!CredWrite(ref creds, 0)) {
                                    // Failed to save credentials. Do another attempt by showing user the CredUI with the errorCode set.
                                    errorCode = Marshal.GetLastWin32Error();
                                    logger?.LogInformation(Resources.Error_CredSaveFailed, errorCode);
                                    if (token != IntPtr.Zero) {
                                        CloseHandle(token);
                                    }
                                    continue;
                                }

                                logger?.LogInformation(Resources.Info_CredSaveSucceeded);
                            } else {
                                // Failed to login user. Do another attempt by showing user the CredUI with the errorCode set.
                                errorCode = Marshal.GetLastWin32Error();
                                logger?.LogError(Resources.Error_LogOnAttemptFailed, errorCode);
                                if (token != IntPtr.Zero) {
                                    CloseHandle(token);
                                }
                                continue;
                            }

                            // Credentials received successfully.
                            // Exit loop, another attempt is not needed.
                            break;
                        } else {
                            // CredUIPromptForWindowsCredentials failed to load some component
                            logger?.LogError(Resources.Error_CredUIFailedToLoad, Marshal.GetLastWin32Error());
                            throw new Win32Exception(string.Format(Resources.Error_CredUIFailedToLoad, ret));
                        }
                    }
                } finally {
                    if (token != IntPtr.Zero) {
                        CloseHandle(token);
                    }

                    if (credentialBuffer != IntPtr.Zero) {
                        Marshal.ZeroFreeCoTaskMemUnicode(credentialBuffer);
                    }
                }

                return attempts;
            }
        }

        public static void RemoveCredentials(ILogger logger = null) {
            CredDelete(BrokerUserCredName, CredType.GENERIC, 0);
            logger?.LogInformation(Resources.Info_RemovedCredentials);
        }

        private static object _setCredentialsOnProcessLock = new object();
        public static void SetCredentialsOnProcess(ProcessStartInfo psi, ILogger logger = null) {
            lock (_setCredentialsOnProcessLock) {
                IntPtr cred = IntPtr.Zero;
                try {
                    if (CredRead(BrokerUserCredName, CredType.GENERIC, 0, out cred)) {
                        Credential credential = new Credential();
                        credential = (Credential)Marshal.PtrToStructure(cred, typeof(Credential));

                        byte[] passwordBytes = new byte[credential.credentialBlobSize];
                        Marshal.Copy(credential.credentialBlob, passwordBytes, 0, credential.credentialBlobSize);
                        string passwordText = Encoding.Unicode.GetString(passwordBytes);

                        var usernameBldr = new StringBuilder(CRED_MAX_USERNAME_LENGTH + 1);
                        var domainBldr = new StringBuilder(CRED_MAX_USERNAME_LENGTH + 1);

                        uint error = CredUIParseUserName(credential.userName, usernameBldr, usernameBldr.Capacity, domainBldr, domainBldr.Capacity);
                        if (error != 0) {
                            logger?.LogError(Resources.Error_UserNameParsing, error);
                            throw new Win32Exception((int)error, Resources.Error_UserNameParsing.FormatInvariant(error));
                        }

                        SecureString pass = new SecureString();
                        foreach (char c in passwordText) {
                            pass.AppendChar(c);
                        }

                        psi.UserName = usernameBldr.ToString();
                        psi.Domain = domainBldr.ToString();
                        psi.Password = pass;

                        logger?.LogInformation(Resources.Info_CredRetreiveSucceeded);
                    } else {
                        logger?.LogError(Resources.Error_CredRetreiveFailed, Marshal.GetLastWin32Error());
                    }
                } finally {
                    if (cred != IntPtr.Zero) {
                        CredFree(cred);
                    }
                }
            }
        }

        internal enum LogonType {
            /// <summary>
            /// This logon type is intended for users who will be interactively using the computer, such as a user being logged on  
            /// by a terminal server, remote shell, or similar process.
            /// This logon type has the additional expense of caching logon information for disconnected operations; 
            /// therefore, it is inappropriate for some client/server applications,
            /// such as a mail server.
            /// </summary>
            LOGON32_LOGON_INTERACTIVE = 2,

            /// <summary>
            /// This logon type is intended for high performance servers to authenticate plaintext passwords.

            /// The LogonUser function does not cache credentials for this logon type.
            /// </summary>
            LOGON32_LOGON_NETWORK = 3,

            /// <summary>
            /// This logon type is intended for batch servers, where processes may be executing on behalf of a user without 
            /// their direct intervention. This type is also for higher performance servers that process many plaintext
            /// authentication attempts at a time, such as mail or Web servers. 
            /// The LogonUser function does not cache credentials for this logon type.
            /// </summary>
            LOGON32_LOGON_BATCH = 4,

            /// <summary>
            /// Indicates a service-type logon. The account provided must have the service privilege enabled. 
            /// </summary>
            LOGON32_LOGON_SERVICE = 5,

            /// <summary>
            /// This logon type is for GINA DLLs that log on users who will be interactively using the computer. 
            /// This logon type can generate a unique audit record that shows when the workstation was unlocked. 
            /// </summary>
            LOGON32_LOGON_UNLOCK = 7,

            /// <summary>
            /// This logon type preserves the name and password in the authentication package, which allows the server to make 
            /// connections to other network servers while impersonating the client. A server can accept plaintext credentials 
            /// from a client, call LogonUser, verify that the user can access the system across the network, and still 
            /// communicate with other servers.
            /// NOTE: Windows NT:  This value is not supported. 
            /// </summary>
            LOGON32_LOGON_NETWORK_CLEARTEXT = 8,

            /// <summary>
            /// This logon type allows the caller to clone its current token and specify new credentials for outbound connections.
            /// The new logon session has the same local identifier but uses different credentials for other network connections. 
            /// NOTE: This logon type is supported only by the LOGON32_PROVIDER_WINNT50 logon provider.
            /// NOTE: Windows NT:  This value is not supported. 
            /// </summary>
            LOGON32_LOGON_NEW_CREDENTIALS = 9,
        }

        internal enum LogonProvider {
            /// <summary>
            /// Use the standard logon provider for the system. 
            /// The default security provider is negotiate, unless you pass NULL for the domain name and the user name 
            /// is not in UPN format. In this case, the default provider is NTLM. 
            /// NOTE: Windows 2000/NT:   The default security provider is NTLM.
            /// </summary>
            LOGON32_PROVIDER_DEFAULT = 0,
            LOGON32_PROVIDER_WINNT35 = 1,
            LOGON32_PROVIDER_WINNT40 = 2,
            LOGON32_PROVIDER_WINNT50 = 3
        }

        private enum CredType : int {
            GENERIC = 1,
            DOMAIN_PASSWORD = 2,
            DOMAIN_CERTIFICATE = 3,
            DOMAIN_VISIBLE_PASSWORD = 4,
            MAXIMUM = 5
        }

        private enum CredPersist : uint {
            SESSION = 1,
            LOCAL_MACHINE = 2,
            ENTERPRISE = 3
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct Credential {
            public int flags;
            public int type;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string targetName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME lastWritten;
            public int credentialBlobSize;
            public IntPtr credentialBlob;
            public int persist;
            public int attributeCount;
            public IntPtr credAttribute;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string targetAlias;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string userName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CredUIInfo {
            public int cbSize;
            public IntPtr hwndParent;
            public string pszMessageText;
            public string pszCaptionText;
            public IntPtr hbmBanner;
        }

        [Flags]
        private enum PromptForWindowsCredentialsFlags {
            /// <summary>
            /// The caller is requesting that the credential provider return the user name and password in plain text.
            /// This value cannot be combined with SECURE_PROMPT.
            /// </summary>
            CREDUIWIN_GENERIC = 0x1,
            /// <summary>
            /// The Save check box is displayed in the dialog box.
            /// </summary>
            CREDUIWIN_CHECKBOX = 0x2,
            /// <summary>
            /// Only credential providers that support the authentication package specified by the authPackage parameter should be enumerated.
            /// This value cannot be combined with CREDUIWIN_IN_CRED_ONLY.
            /// </summary>
            CREDUIWIN_AUTHPACKAGE_ONLY = 0x10,
            /// <summary>
            /// Only the credentials specified by the InAuthBuffer parameter for the authentication package specified by the authPackage parameter should be enumerated.
            /// If this flag is set, and the InAuthBuffer parameter is NULL, the function fails.
            /// This value cannot be combined with CREDUIWIN_AUTHPACKAGE_ONLY.
            /// </summary>
            CREDUIWIN_IN_CRED_ONLY = 0x20,
            /// <summary>
            /// Credential providers should enumerate only administrators. This value is intended for User Account Control (UAC) purposes only. We recommend that external callers not set this flag.
            /// </summary>
            CREDUIWIN_ENUMERATE_ADMINS = 0x100,
            /// <summary>
            /// Only the incoming credentials for the authentication package specified by the authPackage parameter should be enumerated.
            /// </summary>
            CREDUIWIN_ENUMERATE_CURRENT_USER = 0x200,
            /// <summary>
            /// The credential dialog box should be displayed on the secure desktop. This value cannot be combined with CREDUIWIN_GENERIC.
            /// Windows Vista: This value is not supported until Windows Vista with SP1.
            /// </summary>
            CREDUIWIN_SECURE_PROMPT = 0x1000,
            /// <summary>
            /// The credential provider should align the credential BLOB pointed to by the refOutAuthBuffer parameter to a 32-bit boundary, even if the provider is running on a 64-bit system.
            /// </summary>
            CREDUIWIN_PACK_32_WOW = 0x10000000,
        }

        [DllImport("advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredRead(string target, CredType type, int reservedFlag, out IntPtr credentialPtr);

        [DllImport("advapi32.dll", SetLastError = true, EntryPoint = "CredWriteW", CharSet = CharSet.Unicode)]
        private static extern bool CredWrite([In] ref Credential userCredential, [In] uint flags);


        [DllImport("credui.dll", CharSet = CharSet.Unicode)]
        private static extern uint CredUIPromptForWindowsCredentials(
            ref CredUIInfo credInfo,
            int authError,
            ref uint authPackage,
            IntPtr InAuthBuffer,
            uint InAuthBufferSize,
            out IntPtr refOutAuthBuffer,
            out uint refOutAuthBufferSize,
            ref bool fSave,
            PromptForWindowsCredentialsFlags flags);


        [DllImport("credui.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool CredUnPackAuthenticationBuffer(
            int dwFlags,
            IntPtr pAuthBuffer,
            uint cbAuthBuffer,
            StringBuilder pszUserName,
            ref int pcchMaxUserName,
            StringBuilder pszDomainName,
            ref int pcchMaxDomainame,
            StringBuilder pszPassword,
            ref int pcchMaxPassword);

        [DllImport("advapi32.dll", SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        private static extern bool LogonUser(
            [MarshalAs(UnmanagedType.LPStr)] string pszUserName,
            [MarshalAs(UnmanagedType.LPStr)] string pszDomain,
            [MarshalAs(UnmanagedType.LPStr)] string pszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken);


        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hHandle);

        [DllImport("advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode)]
        private static extern bool CredDelete(string target, CredType type, int flags);

        [DllImport("credui.dll", CharSet = CharSet.Unicode)]
        internal static extern uint CredUIParseUserName(
                string userName,
                StringBuilder user,
                int userMaxChars,
                StringBuilder domain,
                int domainMaxChars);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool CredFree([In] IntPtr buffer);
    }
}
