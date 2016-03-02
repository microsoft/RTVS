// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.Languages.Editor.Undo {
    public class TextUndoPrimitiveBase : ITextUndoPrimitive {
        protected ITextBuffer TextBuffer { get; private set; }

        public TextUndoPrimitiveBase(ITextBuffer textBuffer) {
            TextBuffer = textBuffer;
        }

        public bool CanMerge(ITextUndoPrimitive older) {
            return false;
        }

        public bool CanRedo {
            get { return true; }
        }

        public bool CanUndo {
            get { return true; }
        }

        public ITextUndoPrimitive Merge(ITextUndoPrimitive older) {
            throw new NotImplementedException();
        }

        public ITextUndoTransaction Parent { get; set; }

        public virtual void Do() { }
        public virtual void Undo() { }
    }
}
