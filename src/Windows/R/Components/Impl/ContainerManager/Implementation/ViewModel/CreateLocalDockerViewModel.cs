// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Security;
using System.Text.RegularExpressions;
using Microsoft.Common.Core.Net;
using Microsoft.R.Common.Wpf.Controls;

namespace Microsoft.R.Components.ContainerManager.Implementation.ViewModel {
    internal sealed class CreateLocalDockerViewModel : BindableBase {
        private static readonly Regex NameRegex = new Regex("^[a-zA-Z0-9][a-zA-Z0-9_-]+$", RegexOptions.Compiled);
        private static readonly string ExistingPasswordWatermark = new string('●', 8);
        private static readonly int _minPort = 5500;
        private static readonly int _maxPort = 7000;
        private static readonly string _defaultVersion = "latest";

        private string _name;
        private string _username;
        private SecureString _password;
        private string _passwordWatermark;
        private string _version;
        private int _port;
        private bool _isNameValid;
        private bool _isUsernameValid;
        private bool _isPasswordValid;
        private bool _isValid;
        private bool _isPortAvailable;

        public CreateLocalDockerViewModel(string username, SecureString password) {
            Username = username;
            Password = password;
            _passwordWatermark = IsPasswordValid ? ExistingPasswordWatermark : Resources.ContainerManager_CreateLocalDocker_Password;
            Port = PortUtil.GetAvailablePort(_minPort, _maxPort);
            Version = _defaultVersion;
        }

        public string Name {
            get => _name;
            set {
                if (SetProperty(ref _name, value)) {
                    IsNameValid = NameRegex.IsMatch(value);
                    UpdateIsValid();
                }
            }
        }

        public bool IsNameValid {
            get => _isNameValid;
            set => SetProperty(ref _isNameValid, value);
        }

        public string Username {
            get => _username;
            set {
                if (SetProperty(ref _username, value)) {
                    IsUsernameValid = !string.IsNullOrWhiteSpace(value);
                    UpdateIsValid();
                }
            }
        }

        public bool IsUsernameValid {
            get => _isUsernameValid;
            set => SetProperty(ref _isUsernameValid, value);
        }

        public SecureString Password {
            get => _password;
            set {
                if (SetProperty(ref _password, value)) {
                    IsPasswordValid = value != null && value.Length > 0;
                    PasswordWatermark = Resources.ContainerManager_CreateLocalDocker_Password;
                    UpdateIsValid();
                }
            }
        }

        public string PasswordWatermark {
            get => _passwordWatermark;
            private set => SetProperty(ref _passwordWatermark, value);
        }

        public bool IsPasswordValid {
            get => _isPasswordValid;
            set => SetProperty(ref _isPasswordValid, value);
        }

        public string Version {
            get => _version;
            set => SetProperty(ref _version, value);
        }

        public int Port {
            get => _port;
            set {
                if(SetProperty(ref _port, value)) {
                    IsPortAvailable = PortUtil.IsPortAvailable(value);
                }
            }
        }

        public bool IsPortAvailable {
            get => _isPortAvailable;
            set => SetProperty(ref _isPortAvailable, value);
        }

        public bool IsValid {
            get => _isValid;
            private set => SetProperty(ref _isValid, value);
        }

        private void UpdateIsValid() => IsValid = IsNameValid && IsUsernameValid && IsPasswordValid;
    }
}