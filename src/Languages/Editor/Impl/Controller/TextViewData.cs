// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Controller {
    public class TextViewData {
        private List<ITextView> _allViews;
        public IList<ITextView> AllViews {
            get {
                return _allViews;
            }
        }

        public ITextView LastActiveView { get; private set; }

        public TextViewData() {
            _allViews = new List<ITextView>();
        }

        public void AddView(ITextView textView) {
            textView.GotAggregateFocus += OnTextViewGotAggregateFocus;

            if (LastActiveView == null)
                LastActiveView = textView;

            _allViews.Add(textView);
        }

        public void RemoveView(ITextView textView) {
            textView.GotAggregateFocus -= OnTextViewGotAggregateFocus;
            _allViews.Remove(textView);
            if (LastActiveView == textView) {
                LastActiveView = (_allViews.Count > 0 ? _allViews[_allViews.Count - 1] : null);

                if (LastActiveViewChanged != null)
                    LastActiveViewChanged(this, EventArgs.Empty);
            }
        }

        protected void OnTextViewGotAggregateFocus(object sender, EventArgs eventArgs) {
            LastActiveView = sender as ITextView;
            if (LastActiveViewChanged != null)
                LastActiveViewChanged(this, EventArgs.Empty);
        }

        public event EventHandler<EventArgs> LastActiveViewChanged;
    }
}
