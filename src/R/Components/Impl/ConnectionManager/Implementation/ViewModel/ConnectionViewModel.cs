// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Threading;
using Microsoft.Common.Core;
using Microsoft.Common.Wpf;
using Microsoft.R.Components.ConnectionManager.ViewModel;
using static System.FormattableString;

namespace Microsoft.R.Components.ConnectionManager.Implementation.ViewModel {
    internal sealed class ConnectionViewModel : BindableBase, IConnectionViewModel {
        private const int DefaultPort = 5444;

        private readonly IConnection _connection;

        private string _name;
        private string _userProvidedPath;
        private string _rCommandLineArguments;
        private bool _isUserCreated;
        private string _saveButtonTooltip;
        private string _testConnectionResult;
        private bool _isActive;
        private bool _isEditing;
        private bool _isConnected;
        private CancellationTokenSource _testingConnectionCts;
        private bool _isTestConnectionSucceeded;
        private bool _isRemote;
        private bool _hasChanges;
        private bool _isValid;

        public ConnectionViewModel() {
            IsUserCreated = true;
            UpdateCalculated();
        }

        public ConnectionViewModel(IConnection connection) {
            _connection = connection;

            Id = _connection.Id;
            Reset();
        }

        public Uri Id { get; }

        /// <summary>
        /// User-friendly name of the connection.
        /// </summary>
        public string Name {
            get { return _name; }
            set {
                SetProperty(ref _name, value);
                UpdateCalculated();
            }
        }

        /// <summary>
        /// Remote machine name or URL as entered by the user. In the local case 
        /// it is same as <see cref="Path"/>. In the remote case user can enter 
        /// just the machine name and assume default protocol and port are suppied.
        /// </summary>
        public string UserProvidedPath {
            get { return _userProvidedPath; }
            set {
                SetProperty(ref _userProvidedPath, value);
                Path = ToCompletePath(_userProvidedPath);
            }
        }

        /// <summary>
        /// Path to local interpreter installation or URL to remote machine.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Optional command line arguments to R interpreter.
        /// </summary>
        public string RCommandLineArguments {
            get { return _rCommandLineArguments; }
            set {
                SetProperty(ref _rCommandLineArguments, value);
                UpdateCalculated();
            }
        }

        public bool IsUserCreated {
            get { return _isUserCreated; }
            set { SetProperty(ref _isUserCreated, value); }
        }

        public string SaveButtonTooltip {
            get { return _saveButtonTooltip; }
            private set { SetProperty(ref _saveButtonTooltip, value); }
        }

        public bool IsActive {
            get { return _isActive; }
            set { SetProperty(ref _isActive, value); }
        }

        public bool IsEditing {
            get { return _isEditing; }
            set { SetProperty(ref _isEditing, value); }
        }

        public bool IsRemote {
            get { return _isRemote; }
            private set { SetProperty(ref _isRemote, value); }
        }

        public bool IsValid {
            get { return _isValid; }
            private set { SetProperty(ref _isValid, value); }
        }

        public bool HasChanges {
            get { return _hasChanges; }
            private set { SetProperty(ref _hasChanges, value); }
        }

        public bool IsConnected {
            get { return _isConnected; }
            set { SetProperty(ref _isConnected, value); }
        }

        public CancellationTokenSource TestingConnectionCts {
            get { return _testingConnectionCts; }
            set { SetProperty(ref _testingConnectionCts, value); }
        }

        public bool IsTestConnectionSucceeded {
            get { return _isTestConnectionSucceeded; }
            set { SetProperty(ref _isTestConnectionSucceeded, value); }
        }

        public string TestConnectionFailedText {
            get { return _testConnectionResult; }
            set { SetProperty(ref _testConnectionResult, value); }
        }

        /// <summary>
        /// Tooltip when hovered over connection name
        /// </summary>
        public string ConnectionTooltip {
            get {
                var cmdLineInfo = !string.IsNullOrWhiteSpace(RCommandLineArguments) ? RCommandLineArguments : Resources.ConnectionManager_None;

                if (IsRemote) {
                    Uri uri;
                    Uri.TryCreate(Path, UriKind.Absolute, out uri);

                    return string.Format(CultureInfo.InvariantCulture, Resources.ConnectionManager_InformationTooltipFormatRemote,
                        IsActive ? Resources.ConnectionManager_Connected : Resources.ConnectionManager_Disconnected,
                        uri != null ? uri.Host : Resources.ConnectionManager_Unknown,
                        uri != null ? uri.Port.ToString(CultureInfo.InvariantCulture) : Resources.ConnectionManager_Default, cmdLineInfo);
                } else {
                    return string.Format(CultureInfo.InvariantCulture, Resources.ConnectionManager_InformationTooltipFormatLocal,
                        IsActive ? Resources.ConnectionManager_Active : Resources.ConnectionManager_Inactive,
                        Path, cmdLineInfo);
                }
            }
        }

        public DateTime LastUsed => _connection.LastUsed;

        public void Reset() {
            // Make sure user path and remoteness are set first
            IsRemote = _connection?.IsRemote ?? false;
            IsUserCreated = _connection?.IsUserCreated ?? false;
            UserProvidedPath = _connection?.UserProvidedPath;

            Name = _connection?.Name;
            RCommandLineArguments = _connection?.RCommandLineArguments;
            IsEditing = false;
            IsTestConnectionSucceeded = false;
            TestConnectionFailedText = null;
            TestingConnectionCts?.Cancel();
            TestingConnectionCts = null;
        }

        private void UpdateCalculated() {
            HasChanges = !Name.EqualsIgnoreCase(_connection?.Name)
                || !Path.EqualsIgnoreCase(_connection?.Path)
                || !RCommandLineArguments.EqualsIgnoreCase(_connection?.RCommandLineArguments);

            Uri uri = null;
            var isPathValid = Uri.TryCreate(Path, UriKind.Absolute, out uri);
            if (string.IsNullOrEmpty(Name)) {
                IsValid = false;
                SaveButtonTooltip = Resources.ConnectionManager_ShouldHaveName;
            } else if (!isPathValid) {
                IsValid = false;
                SaveButtonTooltip = Resources.ConnectionManager_ShouldHavePath;
            } else {
                IsValid = true;
                SaveButtonTooltip = Resources.ConnectionManager_Save;
            }

            IsRemote = !(uri?.IsFile ?? true);
        }

        private string ToCompletePath(string path) {
            // https://foo:5444 -> https://foo:5444 (no change)
            // https://foo -> https://foo (no change)
            // http://foo -> http://foo (no change)
            // foo->https://foo:5444
            // foo: 5000->https://foo:5000
            // username: password @foo -> https://username:password@foo:5444
            if (IsRemote) {
                Uri uri = null;
                try {
                    Uri.TryCreate(path, UriKind.Absolute, out uri);
                } catch (InvalidOperationException) { } catch (ArgumentException) { } catch(UriFormatException) { }

                if (uri == null || !(uri.IsFile || string.IsNullOrEmpty(uri.Host))) {
                    bool hasScheme = uri != null && !string.IsNullOrEmpty(uri.Scheme);
                    bool hasPort = uri != null && uri.Port >= 0;

                    if (hasScheme) {
                        if (hasPort) {
                            return Invariant($"{uri.Scheme}{Uri.SchemeDelimiter}{uri.Host}:{uri.Port}");
                        }
                        return Invariant($"{uri.Scheme}{Uri.SchemeDelimiter}{uri.Host}");
                    } else {
                        if (Uri.CheckHostName(path) != UriHostNameType.Unknown) {
                            var port = hasPort ? uri.Port : DefaultPort;
                            return Invariant($"{Uri.UriSchemeHttps}{Uri.SchemeDelimiter}{path}:{port}");
                        }
                    }
                }
            }
            return path;
        }
    }
}