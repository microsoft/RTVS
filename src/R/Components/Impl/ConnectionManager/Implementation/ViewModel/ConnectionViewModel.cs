// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Threading;
using System.Windows.Media;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Wpf;
using Microsoft.R.Components.ConnectionManager.ViewModel;

namespace Microsoft.R.Components.ConnectionManager.Implementation.ViewModel {
    internal sealed class ConnectionViewModel : BindableBase, IConnectionViewModel {
        private readonly IConnection _connection;
        private readonly IColorService _colorService;

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

        public ConnectionViewModel(IConnection connection, IColorService colorService) {
            _connection = connection;
            _colorService = colorService;

            Id = _connection.Id;
            Reset();
        }

        public Uri Id { get; }

        public string Name {
            get { return _name; }
            set {
                SetProperty(ref _name, value);
                UpdateCalculated();
            }
        }

        public string Path {
            get { return _path; }
            set {
                SetProperty(ref _path, value);
                UpdateCalculated();
            }
        }

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

        public Brush TestConnectionFailedTextColor {
            get {
                // Similar to 'error squiggly' editor color
                return new SolidColorBrush(_colorService.IsDarkTheme ? Color.FromArgb(0xFF, 0xBD, 0x35, 0x2F) : Color.FromArgb(0xFF, 0xFF, 0x40, 0x40));
            }
        }

        public Brush TestConnectionSucceededTextColor {
            get {
                // Similar to 'comment' editor color
                return new SolidColorBrush(_colorService.IsDarkTheme  ? Color.FromArgb(0xFF, 0x49, 0x8B, 0x4E) : Color.FromArgb(0xFF, 0x34, 0x80, 0x63));
            }
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
                        uri != null ? uri.Port.ToString() : Resources.ConnectionManager_Default, cmdLineInfo);
                } else {
                    return string.Format(CultureInfo.InvariantCulture, Resources.ConnectionManager_InformationTooltipFormatLocal,
                        IsActive ? Resources.ConnectionManager_Active : Resources.ConnectionManager_Inactive,
                        Path, cmdLineInfo);
                }
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
    }
}