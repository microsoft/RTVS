// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.SuggestedActions {
    /// <summary>
    /// Base class for suggested action implementations.  
    /// Actions can derive from this type and specialize as necessary 
    /// for their language context (e.g. R, Markdown, JSON, etc).
    /// </summary>
    public abstract class SuggestedActionBase : ISuggestedAction {
        protected SuggestedActionBase(ITextView view, ITextBuffer buffer, int position, string displayText) {
            TextBuffer = buffer;
            TextView = view;
            Position = position;
            DisplayText = displayText;
        }

        protected SuggestedActionBase(ITextView view, ITextBuffer buffer, int position, string displayText, ImageMoniker moniker) :
            this(view, buffer, position, displayText) {
            IconMoniker = moniker;
        }

        public ITextBuffer TextBuffer { get; }
        public ITextView TextView { get; }
        public int Position { get; }

        #region ISuggestedAction members
        /// <summary>
        /// By default, nested actions are not supported.
        /// </summary>
        public virtual bool HasActionSets => false;

        /// <summary>
        /// By default, nested actions are not supported.
        /// </summary>
        public virtual Task<IEnumerable<SuggestedActionSet>> GetActionSetsAsync(CancellationToken cancellationToken)
            => Task.FromResult(Enumerable.Empty<SuggestedActionSet>());

        public string DisplayText { get; }
        public string IconAutomationText { get; }
        public ImageMoniker IconMoniker { get; }
        public string InputGestureText { get; protected set; }

        /// <summary>
        /// By default, Preview is not supported.
        /// </summary>
        public virtual bool HasPreview => false;

        public virtual Task<object> GetPreviewAsync(CancellationToken cancellationToken) => Task.FromResult<object>(null);

        public abstract void Invoke(CancellationToken cancellationToken);

        public abstract bool TryGetTelemetryId(out Guid telemetryId);

        #region IDisposable
        protected virtual void Dispose(bool disposing) { }

        public void Dispose() {
            Dispose(true);
        }
        #endregion
        #endregion
    }
}
