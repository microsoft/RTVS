// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.Editor.Mocks {
    [ExcludeFromCodeCoverage]
    public class EditorOptionsMock : IEditorOptions {
        public IEditorOptions GlobalOptions { get; set; }

        public IEditorOptions Parent { get; set; }

        public IEnumerable<EditorOptionDefinition> SupportedOptions {
            get {
                throw new NotImplementedException();
            }
        }
#pragma warning disable 67
        public event EventHandler<EditorOptionChangedEventArgs> OptionChanged;

        public bool ClearOptionValue(string optionId) {
            throw new NotImplementedException();
        }

        public bool ClearOptionValue<T>(EditorOptionKey<T> key) {
            throw new NotImplementedException();
        }

        public object GetOptionValue(string optionId) {
            throw new NotImplementedException();
        }

        public T GetOptionValue<T>(EditorOptionKey<T> key) {
            throw new NotImplementedException();
        }

        public T GetOptionValue<T>(string optionId) {
            throw new NotImplementedException();
        }

        public bool IsOptionDefined(string optionId, bool localScopeOnly) {
            throw new NotImplementedException();
        }

        public bool IsOptionDefined<T>(EditorOptionKey<T> key, bool localScopeOnly) {
            throw new NotImplementedException();
        }

        public void SetOptionValue(string optionId, object value) {
            throw new NotImplementedException();
        }

        public void SetOptionValue<T>(EditorOptionKey<T> key, T value) {
            throw new NotImplementedException();
        }
    }
}
