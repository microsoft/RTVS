// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.Languages.Editor.Completions {
    /// <summary>
    /// Wraps VS editor completion session in a portable way.
    /// </summary>
    public sealed class EditorIntellisenseSession : IEditorIntellisenseSession {
        public const string SessionKey = "XIntellisenseSession";

        private readonly IIntellisenseSession _session;
        private readonly Lazy<PropertyDictionary> _properties = Lazy.Create(() => new PropertyDictionary());

        public T As<T>() where T: class => _session as T;
        public PropertyDictionary Properties => _properties.Value;
        public IServiceContainer Services { get; }
        public IEditorView View => _session.TextView.ToEditorView();
        public bool IsDismissed => _session.IsDismissed;

        public event EventHandler Dismissed;

        public EditorIntellisenseSession(IIntellisenseSession session, IServiceContainer services) {
            Check.InvalidOperation(() => !session.IsDismissed, "Session is already dismissed");

            _session = session;
            _session.Dismissed += OnSessionDismissed;
            session.Properties[SessionKey] = this;

            Services = services;
        }

        private void OnSessionDismissed(object sender, EventArgs e) {
            _session.Dismissed -= OnSessionDismissed;
            Dismissed?.Invoke(this, EventArgs.Empty);
        }
    }
}
