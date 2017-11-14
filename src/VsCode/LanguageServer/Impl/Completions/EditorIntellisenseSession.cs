// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.LanguageServer.Services.Editor;

namespace Microsoft.R.LanguageServer.Completions {
    internal sealed class EditorIntellisenseSession : PropertyHolder, IEditorIntellisenseSession {
        public EditorIntellisenseSession(IEditorView view, IServiceContainer services) {
            View = view;
            Services = services;
        }

        public T As<T>() where T : class => throw new NotSupportedException();
        public IServiceContainer Services { get; }
        public IEditorView View { get; }
        public bool IsDismissed => false;

#pragma warning disable 67
        public event EventHandler Dismissed;
    }
}
