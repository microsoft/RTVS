// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Controllers.Views {
    public class TextViewData {
        private List<ITextView> _allViews;
        public IList<ITextView> AllViews => _allViews;
        public ITextView LastActiveView { get; private set; }

        public TextViewData() {
            _allViews = new List<ITextView>();
        }

        public void AddView(ITextView textView) {
            textView.GotAggregateFocus += OnTextViewGotAggregateFocus;
            LastActiveView = LastActiveView ?? textView;
            _allViews.Add(textView);
        }

        public void RemoveView(ITextView textView) {
            textView.GotAggregateFocus -= OnTextViewGotAggregateFocus;
            _allViews.Remove(textView);
            if (LastActiveView == textView) {
                LastActiveView = (_allViews.Count > 0 ? _allViews[_allViews.Count - 1] : null);
                LastActiveViewChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        protected void OnTextViewGotAggregateFocus(object sender, EventArgs eventArgs) {
            LastActiveView = sender as ITextView;
            LastActiveViewChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<EventArgs> LastActiveViewChanged;
    }
}
