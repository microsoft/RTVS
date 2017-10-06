// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Security;

namespace Microsoft.R.Components.ContainerManager.Implementation.ViewModel {
    internal sealed class CreateLocalDockerViewModel : CreateLocalDockerViewModelBase {
        private string _passwordWatermark;
        private static readonly string _defaultVersion = "latest";
        private static readonly string ExistingPasswordWatermark = new string('●', 8);

        private string _username;
        private string _version;
        private SecureString _password;
        private bool _isUsernameValid;
        private bool _isPasswordValid;

        public CreateLocalDockerViewModel(string username, SecureString password) {
            Username = username;
            Password = password;
            _passwordWatermark = IsPasswordValid ? ExistingPasswordWatermark : Resources.ContainerManager_CreateLocalDocker_Password;
            Version = _defaultVersion;
        }
        
        public void Deconstruct(out string name, out string username, out SecureString password, out string version, out int port) {
            name = Name;
            username = Username;
            password = Password;
            version = Version;
            port = Port;
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

        protected override bool IsValidOverride() => IsUsernameValid && IsPasswordValid;
    }
}