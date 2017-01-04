// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Wpf;
using Microsoft.R.Components.ConnectionManager.ViewModel;
using static System.FormattableString;

namespace Microsoft.R.Components.ConnectionManager.Implementation.ViewModel {
    internal sealed class ConnectionViewModel : BindableBase, IConnectionViewModel {
        private const int DefaultPort = 5444;
        private readonly IConnection _connection;
        private readonly ICoreShell _coreShell;

        private string _name;
        private string _path;
        private string _rCommandLineArguments;
        private bool _isUserCreated;
        private string _saveButtonTooltip;
        private string _testConnectionResult;
        private bool _isActive;
        private bool _isEditing;
        private bool _isConnected;
        private bool _isRunning;
        private CancellationTokenSource _testingConnectionCts;
        private bool _isTestConnectionSucceeded;
        private bool _isRemote;
        private bool _hasChanges;
        private bool _isValid;
        private bool _isRenamed;
        private string _previousPath;

        public ConnectionViewModel(ICoreShell coreShell) {
            _coreShell = coreShell;
            IsUserCreated = true;
            UpdateCalculated();
        }

        public ConnectionViewModel(IConnection connection) {
            _connection = connection;
            Reset();
        }

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
                UpdateName();
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
            private set { SetProperty(ref _isUserCreated, value); }
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
            set {
                SetProperty(ref _isEditing, value);
                _previousPath = Path;
            }
        }

        public bool IsRemote {
            get { return _isRemote; }
            private set { SetProperty(ref _isRemote, value); }
        }

        public bool IsValid {
            get { return _isValid; }
            private set { SetProperty(ref _isValid, value); }
        }

        public bool IsRenamed {
            get { return _isRenamed; }
            private set { SetProperty(ref _isRenamed, value); }
        }

        public bool HasChanges {
            get { return _hasChanges; }
            private set { SetProperty(ref _hasChanges, value); }
        }

        public bool IsConnected {
            get { return _isConnected; }
            set { SetProperty(ref _isConnected, value); }
        }

        public bool IsRunning {
            get { return _isRunning; }
            set { SetProperty(ref _isRunning, value); }
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

        public DateTime LastUsed {
            get { return _connection?.LastUsed ?? DateTime.MinValue; }
            set { _connection.LastUsed = value; }
        }

        public string OriginalName => _connection?.Name;

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
            var nameHasChanged = !Name.EqualsIgnoreCase(_connection?.Name);
            IsRenamed = nameHasChanged && _connection?.Name != null;
            HasChanges = nameHasChanged
                || !Path.EqualsIgnoreCase(_connection?.Path)
                || !RCommandLineArguments.EqualsIgnoreCase(_connection?.RCommandLineArguments);

            Uri uri;
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

        /// <summary>
        /// Update the name with a value calculated based on the current path,
        /// if name is determined to be tracking the path. Otherwise, it is not changed.
        /// </summary>
        private void UpdateName() {
            var currentPath = Path?.Trim() ?? string.Empty;
            var previousPath = _previousPath?.Trim() ?? string.Empty;

            var previousProposedName = GetProposedName(previousPath);
            var currentProposedName = GetProposedName(currentPath);

            // Avoid changing anything if the edit
            // has no effect on the name (like changing the path's port)
            if (previousProposedName != currentProposedName) {
                // Check if the name was calculated from the previous path
                var currentName = Name ?? string.Empty;
                if (string.IsNullOrEmpty(currentName) || string.Compare(currentName, previousProposedName, StringComparison.CurrentCultureIgnoreCase) == 0) {
                    // Broker derives log name from connection name and hence the connection
                    // cannot contain all characters. Characters allowed in the path is a good bet.
                    var invalidChars = System.IO.Path.GetInvalidPathChars().Union(System.IO.Path.GetInvalidFileNameChars()).ToArray();
                    if (currentProposedName == null || currentProposedName.IndexOfAny(invalidChars) < 0) {
                        Name = currentProposedName;
                    }
                }
            }

            // Remember the path, for the next update
            _previousPath = currentPath;
        }

        public void UpdatePath() {
            // Automatically update the Path with a more complete version
            Path = GetCompletePath(Path?.Trim() ?? string.Empty, _coreShell);
        }

        internal static string GetProposedName(string path) {
            try {
                Uri uri;
                path = path.TrimEnd(':');
                if (Uri.TryCreate(path, UriKind.Absolute, out uri)) {
                    if (!string.IsNullOrEmpty(uri.Host)) {
                        return uri.Host;
                    } else {
                        return uri.AbsolutePath;
                    }
                }
            } catch (InvalidOperationException) { } catch (ArgumentException) { } catch (UriFormatException) { }
            return path.ToLower();
        }

        internal static string GetCompletePath(string path, ICoreShell shell) {
            // We ALWAYS use HTTPS so no reason to accept anything else.
            // Default RTVS port is 5444.
            // https://foo:5444 -> https://foo:5444 (no change)
            // https://foo -> https://foo:5444
            // http://foo -> https://foo:5444
            // http://FOO -> https://foo:5444
            // http://FOO:80 -> https://foo:80
            // foo->https://foo:5444
            Uri uri = null;
            try {
                Uri.TryCreate(path, UriKind.Absolute, out uri);
            } catch (InvalidOperationException) { } catch (ArgumentException) { } catch (UriFormatException) { }

            if(uri != null && uri.IsFile) {
                return path;
            }

            var userProvidedPath = path;

            if (path.IndexOfOrdinal("://") < 0) {
                path = Invariant($"{Uri.UriSchemeHttps}{Uri.SchemeDelimiter}{path.ToLower()}");
                try {
                    Uri.TryCreate(path, UriKind.Absolute, out uri);
                } catch (InvalidOperationException) { } catch (ArgumentException) { } catch (UriFormatException) { }
            }

            if (uri != null && !string.IsNullOrEmpty(uri.Host)) {
                var hasPort = uri.Port >= 0 && (!uri.IsDefaultPort || uri.Port != 443);
                var port = hasPort ? uri.Port : DefaultPort;
                var hasPathOrQuery = !string.IsNullOrEmpty(uri.PathAndQuery) && uri.PathAndQuery != "/";
                var mainPart = Invariant($"{Uri.UriSchemeHttps}{Uri.SchemeDelimiter}{uri.Host.ToLower()}:{port}");

                path = hasPathOrQuery 
                        ? Invariant($"{mainPart}{uri.PathAndQuery}{uri.Fragment}")
                        : Invariant($"{mainPart}{uri.Fragment}");
            } else {
                shell.ShowErrorMessage(Resources.Error_InvalidURL.FormatInvariant(userProvidedPath));
                path = string.Empty;
            }

            return path;
        }
    }
}