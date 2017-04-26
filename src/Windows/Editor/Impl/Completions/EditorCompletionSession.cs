// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.Languages.Editor.Completions {
    /// <summary>
    /// Wraps VS editor completion session in a portable way.
    /// </summary>
    public sealed class EditorCompletionSession: IEditorCompletionSession {
        private readonly ICompletionSession _session;
        private readonly Lazy<PropertyDictionary> _properties = Lazy.Create(() => new PropertyDictionary());

        public T As<T>() where T: class {

        }

        public PropertyDictionary Properties => _properties.Value;
        public IEditorView View => _session.TextView.ToEditorView();
        public bool IsDismissed => _session.IsDismissed;

        public event EventHandler Dismissed {
            add => _session.Dismissed += value;
            remove => _session.Dismissed -= value;
        }

        public EditorCompletionSession(ICompletionSession session) {
            _session = session;
        }
    }
}
