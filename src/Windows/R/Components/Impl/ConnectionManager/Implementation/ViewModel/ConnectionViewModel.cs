// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Imaging;
using Microsoft.R.Common.Wpf.Controls;
using Microsoft.R.Components.ConnectionManager.ViewModel;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using static System.FormattableString;

namespace Microsoft.R.Components.ConnectionManager.Implementation.ViewModel {
    internal sealed class ConnectionViewModel : BindableBase, IConnectionViewModel {
        private static readonly char[] _allowedNameChars = { '(', ')', '[', ']', '_', ' ', '@', '-', '.', '\'' };
        private const int DefaultPort = 5444;

        private readonly IConnection _connection;
        private readonly IImageService _images;

        private string _name;
        private string _path;
        private string _rCommandLineArguments;
        private bool _isUserCreated;
        private string _nameTextBoxTooltip;
        private string _pathTextBoxTooltip;
        private string _saveButtonTooltip;
        private string _testConnectionResult;
        private object _icon;
        private object _overlayIcon;
        private bool _isActive;
        private bool _isEditing;
        private bool _isConnected;
        private bool _isRunning;
        private CancellationTokenSource _testingConnectionCts;
        private bool _isTestConnectionSucceeded;
        private bool _isRemote;
        private bool _hasChanges;
        private bool _isValid;
        private bool _isNameValid;
        private bool _isPathValid;
        private bool _isRenamed;
        private string _previousPath;

        public ConnectionViewModel(IImageService images) {
            _images = images;
            IsUserCreated = true;
            UpdateCalculated();
        }

        public ConnectionViewModel(IConnection connection, IConnectionManager connectionManager, IImageService images) {
            _connection = connection;
            _images = images;

            var isActive = connection == connectionManager.ActiveConnection;
            IsActive = isActive;
            IsConnected = isActive && connectionManager.IsConnected;
            IsRunning = isActive && connectionManager.IsRunning;
            Reset();
        }

        /// <summary>
        /// User-friendly name of the connection.
        /// </summary>
        public string Name {
            get => _name;
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
            get => _path;
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
            get => _rCommandLineArguments;
            set {
                SetProperty(ref _rCommandLineArguments, value);
                UpdateCalculated();
            }
        }

        public bool IsUserCreated {
            get => _isUserCreated;
            private set => SetProperty(ref _isUserCreated, value);
        }

        public string SaveButtonTooltip {
            get => _saveButtonTooltip;
            private set => SetProperty(ref _saveButtonTooltip, value);
        }

        public string NameTextBoxTooltip {
            get => _nameTextBoxTooltip;
            private set => SetProperty(ref _nameTextBoxTooltip, value);
        }

        public string PathTextBoxTooltip {
            get => _pathTextBoxTooltip;
            private set => SetProperty(ref _pathTextBoxTooltip, value);
        }

        public bool IsActive {
            get => _isActive;
            private set => SetProperty(ref _isActive, value);
        }

        public bool IsEditing {
            get => _isEditing;
            set {
                SetProperty(ref _isEditing, value);
                _previousPath = Path;
            }
        }

        public bool IsRemote {
            get => _isRemote;
            private set => SetProperty(ref _isRemote, value);
        }
        
        public bool IsValid {
            get => _isValid;
            private set => SetProperty(ref _isValid, value);
        }

        public bool IsNameValid {
            get => _isNameValid;
            private set => SetProperty(ref _isNameValid, value);
        }

        public bool IsPathValid {
            get => _isPathValid;
            private set => SetProperty(ref _isPathValid, value);
        }

        public bool IsRenamed {
            get => _isRenamed;
            private set => SetProperty(ref _isRenamed, value);
        }

        public bool HasChanges {
            get => _hasChanges;
            private set => SetProperty(ref _hasChanges, value);
        }

        public bool IsConnected {
            get => _isConnected;
            private set => SetProperty(ref _isConnected, value);
        }

        public bool IsRunning {
            get => _isRunning;
            private set => SetProperty(ref _isRunning, value);
        }

        public CancellationTokenSource TestingConnectionCts {
            get => _testingConnectionCts;
            set => SetProperty(ref _testingConnectionCts, value);
        }

        public bool IsTestConnectionSucceeded {
            get => _isTestConnectionSucceeded;
            set => SetProperty(ref _isTestConnectionSucceeded, value);
        }

        public string TestConnectionFailedText {
            get => _testConnectionResult;
            set => SetProperty(ref _testConnectionResult, value);
        }

        public DateTime LastUsed {
            get => _connection?.LastUsed ?? DateTime.MinValue;
            set => _connection.LastUsed = value;
        }

        public string OriginalName => _connection?.Name;

        /// <summary>
        /// Tooltip when hovered over connection name
        /// </summary>
        public string ConnectionTooltip { get; set; }

        /// <summary>
        /// Tooltip when hovered over connection edit button
        /// </summary>
        public string ButtonEditTooltip { get; set; }

        /// <summary>
        /// Tooltip when hovered over connection delete button
        /// </summary>
        public string ButtonDeleteDisabledTooltip { get; set; }

        public object Icon {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public object OverlayIcon {
            get => _overlayIcon;
            private set => SetProperty(ref _overlayIcon, value);
        }

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

            IsNameValid = IsValidConnectionName(Name);
            if (!IsNameValid) {
                NameTextBoxTooltip = string.IsNullOrEmpty(Name)
                    ? Resources.ConnectionManager_ShouldHaveName
                    : Resources.ConnectionManager_InvalidName;
            } else {
                NameTextBoxTooltip = null;
            }

            IsPathValid = IsValidConnectionUrl(Path, out var uri);
            if (!IsPathValid) {
                PathTextBoxTooltip = string.IsNullOrEmpty(Path)
                    ? Resources.ConnectionManager_ShouldHavePath
                    : Resources.ConnectionManager_InvalidPath;
            } else {
                PathTextBoxTooltip = null;
            }

            IsValid = IsNameValid && IsPathValid;
            if (IsValid) {
                SaveButtonTooltip = Resources.ConnectionManager_Save;
            } else {
                SaveButtonTooltip = !IsNameValid ? NameTextBoxTooltip : PathTextBoxTooltip;
            }

            IsRemote = IsValid && uri != null && !(uri.IsAbsoluteUri && uri.IsFile) && !uri.IsLoopback;
            OverlayIcon = IsRunning ? _images.GetImage("StatusOK") : IsConnected ? _images.GetImage("StatusWarning") : _images.GetImage("StatusError");
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
                    if (currentProposedName == null || IsValidConnectionName(currentProposedName)) {
                        Name = currentProposedName;
                    }
                }
            }

            // Remember the path, for the next update
            _previousPath = currentPath;
        }

        public void UpdatePath() {
            // Automatically update the Path with a more complete version
            Path = GetCompletePath(Path?.Trim() ?? string.Empty);
        }

        internal static string GetProposedName(string path) {
            try {
                path = path.TrimEnd(':');
                if (Uri.TryCreate(path, UriKind.Absolute, out var uri)) {
                    return !string.IsNullOrEmpty(uri.Host) ? uri.Host : uri.AbsolutePath;
                }
            } catch (InvalidOperationException) { } catch (ArgumentException) { } catch (UriFormatException) { }
            return path.ToLower();
        }

        internal static string GetCompletePath(string path) {
            // We ALWAYS use HTTPS so no reason to accept anything else.
            // Default RTVS port is 5444.
            // https://foo:5444 -> https://foo:5444 (no change)
            // https://foo -> https://foo:5444
            // http://foo -> https://foo:5444
            // http://FOO -> https://foo:5444
            // http://FOO:80 -> https://foo:80
            // foo->https://foo:5444
            if (Uri.TryCreate(path, UriKind.Absolute, out var uri) && uri.IsFile) {
                return path;
            }

            if (!path.Contains(Uri.SchemeDelimiter)) {
                path = Invariant($"{Uri.UriSchemeHttps}{Uri.SchemeDelimiter}{path.ToLower()}");
                Uri.TryCreate(path, UriKind.Absolute, out uri);
            }

            if (!string.IsNullOrEmpty(uri?.Host)) {
                var builder = new UriBuilder(uri) {
                    Scheme = Uri.UriSchemeHttps
                };

                const UriComponents leftPartComponents = UriComponents.Scheme | UriComponents.UserInfo | UriComponents.Host | UriComponents.StrongPort;

                if (uri.IsDefaultPort) {
                    // We need to preserve port only if it was specified
                    var leftPart = uri.GetComponents(leftPartComponents, UriFormat.UriEscaped);
                    if (!path.StartsWithIgnoreCase(leftPart)) {
                        builder.Port = DefaultPort;
                    }
                }

                var hasPath = !string.IsNullOrEmpty(builder.Path) && !builder.Path.EqualsOrdinal("/");
                return hasPath 
                    ? builder.ToString() 
                    : builder.Uri.GetComponents(leftPartComponents | UriComponents.Query | UriComponents.Fragment, UriFormat.UriEscaped);
            }

            return path;
        }

        private static bool IsValidConnectionName(string name) {
            // Broker derives log name from connection name and hence the connection cannot contain all characters. 
            if (string.IsNullOrWhiteSpace(name)) {
                return false;
            }
            if (name.IndexOfOrdinal("..") >= 0) {
                return false;
            }
            if (!char.IsLetterOrDigit(name[0]) && name[0] != '.') {
                return false;
            }
            return !name.Any(ch => !char.IsLetterOrDigit(ch) && !_allowedNameChars.Contains(ch));
        }

        private static bool IsValidConnectionUrl(string url, out Uri uri) {
            uri = null;
            if (string.IsNullOrWhiteSpace(url)) {
                return false;
            }

            return Uri.TryCreate(GetCompletePath(url), UriKind.Absolute, out uri);
        }
    }
}