// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Controller.Command {
    /// <summary>
    /// This command works on a buffer of a certain content type within a view.
    ///
    /// The buffer cannot be cached since multiple buffers of the same type can
    /// appear in the same view. The buffer is figured out each time it's needed
    /// from the GetBufferFromView delegate.
    ///
    /// Useful delegates are implemented in ProjectionBufferHelper. They will
    /// get the correct buffer based on where the caret is in the text view.
    /// </summary>
    public class ViewAndBufferCommand : ViewCommand {
        public ViewAndBufferCommand(ITextView textView, CommandId[] ids, bool needCheckout)
            : base(textView, ids, needCheckout) {
        }

        public ITextBuffer TextBuffer {
            get {
                return TextView.TextBuffer;
            }
        }
    }
}
