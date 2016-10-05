// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Host.Client.BrokerServices;
using static Microsoft.R.Host.Client.NativeMethods;

namespace Microsoft.R.Host.Client.Host {
    internal class RemoteCredentialsDecorator : ICredentialsDecorator {
        private readonly IntPtr _applicationWindowHandle;
        private readonly NetworkCredential _credentials;
        private readonly AutoResetEvent _credentialsValidated = new AutoResetEvent(true);
        private readonly string _authority;
        private readonly AsyncReaderWriterLock _lock;
        private bool _credentialsAreValid;

        public RemoteCredentialsDecorator(IntPtr applicationWindowHandle, Uri brokerUri) {
            _applicationWindowHandle = applicationWindowHandle;
            _authority = new UriBuilder { Scheme = brokerUri.Scheme, Host = brokerUri.Host, Port = brokerUri.Port }.ToString();
            _credentials = new NetworkCredential();
            _lock = new AsyncReaderWriterLock();
        }

        public NetworkCredential GetCredential(Uri uri, string authType) => _credentials;

        public async Task<IDisposable> LockCredentialsAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            string userName, password;

            // If there is already a LockCredentialsAsync request for which there hasn't been a validation yet, wait until it completes.
            // This can happen when two sessions are being created concurrently, and we don't want to pop the credential prompt twice -
            // the first prompt should be validated and saved, and then the same credentials will be reused for the second session.
            var token = await _lock.WriterLockAsync(cancellationToken);
            try {
                GetCredentials(_applicationWindowHandle, _authority, Volatile.Read(ref _credentialsAreValid), out userName, out password);
            } catch (Exception) {
                token.Dispose();
                throw;
            }

            _credentials.UserName = userName;
            _credentials.Password = password;

            return Disposable.Create(() => {
                CredUIConfirmCredentials(_authority, Volatile.Read(ref _credentialsAreValid));
                token.Dispose();
            });
        }

        public void InvalidateCredentials() {
            Volatile.Write(ref _credentialsAreValid, false);
        }

        public void OnCredentialsValidated(bool isValid) {
            CredUIConfirmCredentials(_authority, isValid);
            _credentialsValidated.Set();
        }

        private static void GetCredentials(IntPtr hwndParent, string authority, bool ignoreSavedCredentials, out string userName, out string password) {
            var userNameBuilder = new StringBuilder(CREDUI_MAX_USERNAME_LENGTH + 1);
            var passwordBuilder = new StringBuilder(CREDUI_MAX_PASSWORD_LENGTH + 1);

            var save = false;

            var flags = CREDUI_FLAGS_EXCLUDE_CERTIFICATES | CREDUI_FLAGS_PERSIST | CREDUI_FLAGS_EXPECT_CONFIRMATION | CREDUI_FLAGS_GENERIC_CREDENTIALS;
            if (ignoreSavedCredentials)
            {
                flags |= CREDUI_FLAGS_ALWAYS_SHOW_UI;
            }

            var credui = new CREDUI_INFO
            {
                cbSize = Marshal.SizeOf(typeof(CREDUI_INFO)),
                hwndParent = hwndParent,
            };
            var err = CredUIPromptForCredentials(ref credui, authority, IntPtr.Zero, 0, userNameBuilder, userNameBuilder.Capacity, passwordBuilder, passwordBuilder.Capacity, ref save, flags);
            if (err != 0)
            {
                throw new OperationCanceledException("No credentials entered.");
            }

            userName = userNameBuilder.ToString();
            password = passwordBuilder.ToString();
        }
    }
}