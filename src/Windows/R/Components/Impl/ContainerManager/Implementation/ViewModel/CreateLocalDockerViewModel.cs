// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.RegularExpressions;
using Microsoft.R.Common.Wpf.Controls;

namespace Microsoft.R.Components.ContainerManager.Implementation.ViewModel {
    internal sealed class CreateLocalDockerViewModel : BindableBase {
        private static readonly Regex NameRegex = new Regex("^[a-zA-Z0-9][a-zA-Z0-9_-]+$", RegexOptions.Compiled);
        private static readonly Regex VersionRegex = new Regex("^[0-9].[0-9].[0-9]$", RegexOptions.Compiled);

        private string _name;
        private string _username;
        private string _password;
        private string _version;
        private bool _nameIsValid;
        private bool _usernameIsValid;
        private bool _passwordIsValid;
        private bool _versionIsValid;

        public string Name {
            get => _name;
            set {
                if (SetProperty(ref _name, value)) {
                    NameIsValid = NameRegex.IsMatch(value);
                }
            }
        }

        public bool NameIsValid {
            get => _nameIsValid;
            set => SetProperty(ref _nameIsValid, value);
        }

        public string Username {
            get => _username;
            set {
                if (SetProperty(ref _username, value)) {
                    UsernameIsValid = string.IsNullOrWhiteSpace(value);
                }
            }
        }

        public bool UsernameIsValid {
            get => _usernameIsValid;
            set => SetProperty(ref _usernameIsValid, value);
        }

        public string Password {
            get => _password;
            set {
                if (SetProperty(ref _password, value)) {
                    PasswordIsValid = string.IsNullOrWhiteSpace(value);
                }
            }
        }

        public bool PasswordIsValid {
            get => _passwordIsValid;
            set => SetProperty(ref _passwordIsValid, value);
        }

        public string Version {
            get => _version;
            set {
                if (SetProperty(ref _version, value)) {
                    VersionIsValid = VersionRegex.IsMatch(value);
                }
            }
        }

        public bool VersionIsValid {
            get => _versionIsValid;
            set => SetProperty(ref _versionIsValid, value);
        }
    }
}