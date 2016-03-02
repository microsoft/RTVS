// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.Languages.Editor.Text;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Composition;

namespace Microsoft.Languages.Editor.Controller {
    /// <summary>
    /// Base text view connection listener. Generates norifications to derived classes
    /// when text buffer is created, when secondary buffer is connected or when view
    /// gets aggregate focus.
    /// </summary>
    public abstract class TextViewConnectionListener : IWpfTextViewCreationListener, IWpfTextViewConnectionListener {
        [ImportMany]
        public IEnumerable<Lazy<ITextBufferListener, IOrderedComponentContentTypes>> TextBufferListeners { get; set; }

        [ImportMany]
        public IEnumerable<Lazy<ITextViewListener, IOrderedComponentContentTypes>> TextViewListeners { get; set; }

        // Keep track of how many views are using each text buffer
        protected Dictionary<ITextBuffer, TextViewData> TextBufferToViewData { get; private set; }
        private Action _pendingCheckForViewlessTextBuffers;

        // The editor should only import one of these objects, so cache it
        private static IList<TextViewConnectionListener> _allInstances;

        private Dictionary<ITextBuffer, IContentType> _bufferToOriginalContentType;

        protected TextViewConnectionListener() {
            // WebEditor.CompositionService.SatisfyImportsOnce(this);

            if (_allInstances == null) {
                _allInstances = new List<TextViewConnectionListener>();
            }

            // This class is never disposed, so it always stays in the global list
            _allInstances.Add(this);
        }

        private void EnsureInitialized() {
            if (TextBufferToViewData == null) {
                TextBufferToViewData = new Dictionary<ITextBuffer, TextViewData>();

                EditorShell.Current.Terminating += OnTerminateApp;
                TextBufferListeners = Orderer.Order(TextBufferListeners);
                _bufferToOriginalContentType = new Dictionary<ITextBuffer, IContentType>();
            }
        }

        #region IWpfTextViewCreationListener
        // Called once when view is created
        public void TextViewCreated(IWpfTextView textView) {
            OnTextViewCreated(textView);
        }

        #endregion

        #region IWpfTextViewConnectionListener

        // Called multiple times as subject buffers get connected and disconnected in the buffer graph
        public virtual void SubjectBuffersConnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers) {
            EnsureInitialized();

            foreach (ITextBuffer textBuffer in subjectBuffers) {
                TextViewData viewData = null;
                bool newBuffer = !TextBufferToViewData.ContainsKey(textBuffer);

                if (newBuffer) {
                    viewData = new TextViewData();
                    TextBufferToViewData[textBuffer] = viewData;
                } else {
                    viewData = TextBufferToViewData[textBuffer];
                }

                viewData.AddView(textView);

                if (newBuffer) {
                    OnTextBufferCreated(textBuffer);
                }

                OnTextViewConnected(textView, textBuffer);
            }
        }

        public virtual void SubjectBuffersDisconnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers) {
            if (TextBufferToViewData != null) {
                foreach (ITextBuffer textBuffer in subjectBuffers) {
                    OnTextViewDisconnected(textView, textBuffer);

                    if (TextBufferToViewData.ContainsKey(textBuffer)) {
                        TextViewData viewData = TextBufferToViewData[textBuffer];
                        viewData.RemoveView(textView);

                        if (viewData.AllViews.Count == 0 && _pendingCheckForViewlessTextBuffers == null) {
                            if (reason == ConnectionReason.BufferGraphChange) {
                                // The buffer could be temporarily removed from the view, so don't
                                // immediately check if it's unused - do that after posting a message.
                                _pendingCheckForViewlessTextBuffers = CheckForViewlessTextBuffers;
                                EditorShell.Current.Idle += OnIdle;
                            } else {
                                CheckForViewlessTextBuffers();
                            }
                        }
                    }
                }
            }
        }

        protected void FlushPendingAction() {
            if (_pendingCheckForViewlessTextBuffers != null) {
                _pendingCheckForViewlessTextBuffers();
            }
        }

        private void OnIdle(object sender, EventArgs e) {
            FlushPendingAction();
            EditorShell.Current.Idle -= OnIdle;
        }
        #endregion

        /// <summary>
        /// Called delayed on the main thread to catch text buffers that don't have a view anymore
        /// </summary>
        private void CheckForViewlessTextBuffers() {
            _pendingCheckForViewlessTextBuffers = null;

            if (TextBufferToViewData != null) {
                IList<ITextBuffer> unusedBuffers = new List<ITextBuffer>();

                foreach (KeyValuePair<ITextBuffer, TextViewData> pair in TextBufferToViewData) {
                    if (pair.Value.AllViews.Count == 0) {
                        unusedBuffers.Add(pair.Key);
                    }
                }

                foreach (ITextBuffer textBuffer in unusedBuffers) {
                    TextBufferToViewData.Remove(textBuffer);

                    OnTextBufferDisposing(textBuffer);
                }
            }
        }

        private void OnTerminateApp(object sender, EventArgs eventArgs) {
            // CheckForDisposedTextBuffer won't be called when the app terminates, so
            // dispose of each text buffer now.
            if (TextBufferToViewData != null) {
                foreach (ITextBuffer textBuffer in TextBufferToViewData.Keys) {
                    OnTextBufferDisposing(textBuffer);
                }

                TextBufferToViewData.Clear();
            }
        }

        /// <summary>
        /// Called when text view is connected to the subject buffer
        /// </summary>
        protected virtual void OnTextViewConnected(ITextView textView, ITextBuffer textBuffer) {
            EventHandler gotFocus = null;

            gotFocus = (object sender, EventArgs eventArgs) => {
                this.OnTextViewGotAggregateFocus(textView, textBuffer);
                textView.GotAggregateFocus -= gotFocus;
            };

            textView.GotAggregateFocus += gotFocus;

            IContentType contentType = textBuffer.ContentType;

            foreach (Lazy<ITextViewListener, IOrderedComponentContentTypes> listener in TextViewListeners) {
                if (listener.Metadata.ContentTypes != null) {
                    foreach (string curContentType in listener.Metadata.ContentTypes) {
                        if (contentType.IsOfType(curContentType)) {
                            listener.Value.OnTextViewConnected(textView, textBuffer);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called once for every new text view.
        /// </summary>
        protected virtual void OnTextViewCreated(ITextView textView) {
            string optionName = "ProjectNavBarEnabled";

            // Not defined in unit test scenarios and the SetOptionValue call will throw in that scenario
            if (textView.Options.IsOptionDefined(optionName, false)) {
                // Enable the project navbar in mercury projects
                textView.Options.SetOptionValue(optionName, true);
            }
        }

        /// <summary>
        /// Called when view gets aggregate focus. Typically implemented if derived class
        /// needs to access native VS adapters, like IVsTextView.
        /// </summary>
        protected virtual void OnTextViewGotAggregateFocus(ITextView textView, ITextBuffer textBuffer) {
            IEnumerable<Lazy<ITextViewCreationListener, IComponentContentTypes>> listeners = ComponentLocatorForContentType<ITextViewCreationListener, IComponentContentTypes>.ImportMany(textBuffer.ContentType);

            foreach (var listener in listeners) {
                listener.Value.OnTextViewCreated(textView, textBuffer);
            }
        }

        /// <summary>
        /// Called when text view is disconnected from the subject buffer
        /// </summary>
        protected virtual void OnTextViewDisconnected(ITextView textView, ITextBuffer textBuffer) {
            IContentType contentType = textBuffer.ContentType;

            foreach (Lazy<ITextViewListener, IOrderedComponentContentTypes> listener in TextViewListeners) {
                if (listener.Metadata.ContentTypes != null) {
                    foreach (string curContentType in listener.Metadata.ContentTypes) {
                        if (contentType.IsOfType(curContentType)) {
                            listener.Value.OnTextViewDisconnected(textView, textBuffer);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called once for every new text buffer.
        /// </summary>
        protected virtual void OnTextBufferCreated(ITextBuffer textBuffer) {
            IContentType contentType = textBuffer.ContentType;

            _bufferToOriginalContentType.Add(textBuffer, contentType);

            foreach (var listener in TextBufferListeners) {
                if (listener.Metadata.ContentTypes != null) {
                    foreach (string curContentType in listener.Metadata.ContentTypes) {
                        if (contentType.IsOfType(curContentType)) {
                            listener.Value.OnTextBufferCreated(textBuffer);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called once per buffer, when it detached from its last view
        /// </summary>
        protected virtual void OnTextBufferDisposing(ITextBuffer textBuffer) {
            // These listeners must be called in the reverse order

            List<ITextBufferListener> reverseListeners = new List<ITextBufferListener>();
            IContentType contentType;

            // This has to be obtained from this dictionary, as a content type for this buffer 
            //   could trigger this disposal, and we want to send out notifications based
            //   on the original content type.
            if (_bufferToOriginalContentType.TryGetValue(textBuffer, out contentType)) {
                _bufferToOriginalContentType.Remove(textBuffer);

                foreach (var listener in TextBufferListeners) {
                    if (listener.Metadata.ContentTypes != null) {
                        foreach (string curContentType in listener.Metadata.ContentTypes) {
                            if (contentType.IsOfType(curContentType)) {
                                reverseListeners.Insert(0, listener.Value);
                                break;
                            }
                        }
                    }
                }

                foreach (var listener in reverseListeners) {
                    listener.OnTextBufferDisposed(textBuffer);
                }
            }
        }

        public static IEnumerable<ITextView> GetViewsForBuffer(ITextBuffer textBuffer) {
            TextViewData textViewData = GetTextViewDataForBuffer(textBuffer);

            return textViewData != null ? textViewData.AllViews : new ITextView[0];
        }

        public static ITextView GetFirstViewForBuffer(ITextBuffer textBuffer) {
            foreach (ITextView textView in GetViewsForBuffer(textBuffer)) {
                return textView;
            }

            return null;
        }

        public static TextViewData GetTextViewDataForBuffer(ITextBuffer textBuffer) {
            if (_allInstances != null) {
                foreach (TextViewConnectionListener instance in _allInstances) {
                    TextViewData textViewData = null;

                    if ((instance.TextBufferToViewData != null) && (instance.TextBufferToViewData.TryGetValue(textBuffer, out textViewData))) {
                        if (textViewData.AllViews.Count > 0) {
                            return textViewData;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Can be called from tests when idle doesn't occur
        /// </summary>
        public static void StaticFlushPendingAction() {
            if (_allInstances != null) {
                foreach (TextViewConnectionListener instance in _allInstances) {
                    instance.FlushPendingAction();
                }
            }
        }
    }
}