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
        private bool _isNameValid;
        private bool _isUsernameValid;
        private bool _isPasswordValid;
        private bool _isVersionValid = true;
        private bool _isValid;

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

        public string Password {
            get => _password;
            set {
                if (SetProperty(ref _password, value)) {
                    IsPasswordValid = !string.IsNullOrWhiteSpace(value);
                    UpdateIsValid();
                }
            }
        }

        public bool IsPasswordValid {
            get => _isPasswordValid;
            set => SetProperty(ref _isPasswordValid, value);
        }

        public string Version {
            get => _version;
            set {
                if (SetProperty(ref _version, value)) {
                    IsVersionValid = string.IsNullOrEmpty(value) || VersionRegex.IsMatch(value);
                    UpdateIsValid();
                }
            }
        }

        public bool IsVersionValid {
            get => _isVersionValid;
            set => SetProperty(ref _isVersionValid, value);
        }

        public bool IsValid {
            get => _isValid;
            set => SetProperty(ref _isValid, value);
        }

        private void UpdateIsValid() => IsValid = IsNameValid && IsUsernameValid && IsPasswordValid && IsVersionValid;
    }
}