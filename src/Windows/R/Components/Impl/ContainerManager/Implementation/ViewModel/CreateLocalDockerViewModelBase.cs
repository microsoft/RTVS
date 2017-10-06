// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.RegularExpressions;
using Microsoft.Common.Core.Net;
using Microsoft.R.Common.Wpf.Controls;

namespace Microsoft.R.Components.ContainerManager.Implementation.ViewModel {
    internal class CreateLocalDockerViewModelBase : BindableBase {
        private static readonly Regex NameRegex = new Regex("^[a-zA-Z0-9][a-zA-Z0-9_-]+$", RegexOptions.Compiled);
        private static readonly int _minPort = 5500;
        private static readonly int _maxPort = 7000;

        private string _name;
        private int _port;
        private bool _isNameValid;
        private bool _isValid;
        private bool _isPortAvailable;

        public CreateLocalDockerViewModelBase() {
            Port = PortUtil.GetAvailablePort(_minPort, _maxPort);
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

        protected void UpdateIsValid() => IsValid = IsNameValid && IsValidOverride();

        protected virtual bool IsValidOverride() => true;
    }
}