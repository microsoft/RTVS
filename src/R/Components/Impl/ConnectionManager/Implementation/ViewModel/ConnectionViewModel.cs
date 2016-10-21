// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
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
        private string _path;
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
        public string Path {
            get { return _path; }
            set {
                SetProperty(ref _path, value);
                UpdateCalculated();
            }
        }

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
                var format = IsRemote
                                ? Resources.ConnectionManager_InformationTooltipFormatRemote
                                : Resources.ConnectionManager_InformationTooltipFormatLocal;
                return format.FormatInvariant(Path, cmdLineInfo);
            }
        }

        public DateTime LastUsed => _connection.LastUsed;

        public void Reset() {
            Name = _connection?.Name;
            Path = _connection?.Path;
            RCommandLineArguments = _connection?.RCommandLineArguments;
            IsUserCreated = _connection?.IsUserCreated ?? false;
            IsRemote = _connection?.IsRemote ?? false;
            IsEditing = false;
            IsTestConnectionSucceeded = false;
            TestConnectionFailedText = null;
            TestingConnectionCts?.Cancel();
            TestingConnectionCts = null;

            HasChanges = false;
        }

        private void UpdateCalculated() {
            HasChanges = !Name.EqualsIgnoreCase(_connection?.Name)
                || !Path.EqualsIgnoreCase(_connection?.Path)
                || !RCommandLineArguments.EqualsIgnoreCase(_connection?.RCommandLineArguments);

            Uri uri = null;
            var isPathValid = Uri.TryCreate(GetCompletePath(), UriKind.Absolute, out uri);
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

       public string GetCompletePath() {
            // https://foo:5444 -> https://foo:5444 (no change)
            // https://foo -> https://foo (no change)
            // http://foo -> http://foo (no change)
            // foo->https://foo:5444

            Uri uri = null;
            try {
                Uri.TryCreate(Path, UriKind.Absolute, out uri);
            } catch (InvalidOperationException) { } catch (ArgumentException) { } catch (UriFormatException) { }

            if (uri == null || !(uri.IsFile || string.IsNullOrEmpty(uri.Host))) {
                bool hasScheme = uri != null && !string.IsNullOrEmpty(uri.Scheme);
                bool hasPort = uri != null && uri.Port >= 0;

                if (hasScheme) {
                    if (hasPort) {
                        return Invariant($"{uri.Scheme}{Uri.SchemeDelimiter}{uri.Host}:{uri.Port}");
                    }
                    return Invariant($"{uri.Scheme}{Uri.SchemeDelimiter}{uri.Host}");
                } else {
                    if (Uri.CheckHostName(Path) != UriHostNameType.Unknown) {
                        var port = hasPort ? uri.Port : DefaultPort;
                        return Invariant($"{Uri.UriSchemeHttps}{Uri.SchemeDelimiter}{Path}:{port}");
                    }
                }
            }
            return Path;
        }
    }
}