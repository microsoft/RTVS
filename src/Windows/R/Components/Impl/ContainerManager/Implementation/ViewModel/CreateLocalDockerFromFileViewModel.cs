// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.ContainerManager.Implementation.ViewModel {
    internal sealed class CreateLocalDockerFromFileViewModel : CreateLocalDockerViewModelBase {
        private string _templatePath;
        private bool _isTemplatePathValid;

        public string TemplatePath {
            get => _templatePath;
            set {
                if (SetProperty(ref _templatePath, value)) {
                    IsTemplatePathValid = Uri.TryCreate(value, UriKind.Absolute, out _);
                    UpdateIsValid();
                }
            }
        }

        public bool IsTemplatePathValid {
            get => _isTemplatePathValid;
            private set => SetProperty(ref _isTemplatePathValid, value);
        }

        protected override bool IsValidOverride() => IsTemplatePathValid;

        public void Deconstruct(out string name, out string templatePath, out int port) {
            name = Name;
            templatePath = TemplatePath;
            port = Port;
        }
    }
}