// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Controller.Command {
    [ExcludeFromCodeCoverage]
    public class ViewCommand : Command, IDisposable {
        public ITextView TextView { get; set; }

        public ViewCommand(ITextView textView, Guid group, int id, bool needCheckout)
            : base(group, id, needCheckout) {
            TextView = textView;
        }

        public ViewCommand(ITextView textView, int id, bool needCheckout)
            : base(Guid.Empty, id, needCheckout) {
            TextView = textView;
        }

        public ViewCommand(ITextView textView, CommandId id, bool needCheckout)
            : base(id, needCheckout) {
            TextView = textView;
        }

        public ViewCommand(ITextView textView, CommandId[] ids, bool needCheckout)
            : base(ids, needCheckout) {
            TextView = textView;
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing) {
            TextView = null;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
