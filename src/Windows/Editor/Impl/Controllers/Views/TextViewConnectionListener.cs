// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Controllers.Views {
    /// <summary>
    /// Base text view connection listener. Generates norifications to derived classes
    /// when text buffer is created, when secondary buffer is connected or when view
    /// gets aggregate focus.
    /// </summary>
    public abstract class TextViewConnectionListener : IWpfTextViewCreationListener, IWpfTextViewConnectionListener {
        private readonly IEnumerable<Lazy<ITextBufferListener, IOrderedComponentContentTypes>> _textBufferListeners;
        private readonly IEnumerable<Lazy<ITextViewListener, IOrderedComponentContentTypes>> _textViewListeners;
        private readonly Dictionary<ITextBuffer, IContentType> _bufferToOriginalContentType = new Dictionary<ITextBuffer, IContentType>();
        private readonly ITextDocumentFactoryService _tdfs;
        private readonly IIdleTimeService _idleTime;
        private readonly IApplication _application;
        private Action _pendingCheckForViewlessTextBuffers;

        protected IServiceContainer Services { get; }

        // Keep track of how many views are using each text buffer
        protected Dictionary<ITextBuffer, TextViewData> TextBufferToViewData { get; } = new Dictionary<ITextBuffer, TextViewData>();

        // The editor should only import one of these objects, so cache it
        private static IList<TextViewConnectionListener> _allInstances;

        protected TextViewConnectionListener(IServiceContainer services) {
            Services = services;

            var ep = services.GetService<ExportProvider>();
            _textBufferListeners = Orderer.Order(ep.GetExports<ITextBufferListener, IOrderedComponentContentTypes>());
            _textViewListeners = ep.GetExports<ITextViewListener, IOrderedComponentContentTypes>();
            _textViewListeners = ep.GetExports<ITextViewListener, IOrderedComponentContentTypes>();
            _tdfs = services.GetService<ITextDocumentFactoryService>();

            _allInstances = _allInstances ?? new List<TextViewConnectionListener>();
            // This class is never disposed, so it always stays in the global list
            _allInstances.Add(this);
            _idleTime = services.GetService<IIdleTimeService>();
            _application = services.GetService<IApplication>();

            _application.Terminating += OnTerminateApp;
        }

        #region IWpfTextViewCreationListener
        /// <summary>
        /// Called once when view is created
        /// </summary>
        public void TextViewCreated(IWpfTextView textView) {
            EditorView.Create(textView);
            OnTextViewCreated(textView);
        }
        #endregion

        #region IWpfTextViewConnectionListener
        /// <summary>
        /// Called multiple times as subject buffers get connected and disconnected in the buffer graph
        /// </summary>
        public void SubjectBuffersConnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers) {
            EditorView.Create(textView);

            foreach (var textBuffer in subjectBuffers) {
                TextViewData viewData;
                var newBuffer = !TextBufferToViewData.ContainsKey(textBuffer);
                if (newBuffer) {
                    viewData = new TextViewData();
                    TextBufferToViewData[textBuffer] = viewData;
                } else {
                    viewData = TextBufferToViewData[textBuffer];
                }

                viewData.AddView(textView);
                if (newBuffer) {
                    EditorBuffer.Create(textBuffer, _tdfs);
                    OnTextBufferCreated(textView, textBuffer);
                }

                OnTextViewConnected(textView, textBuffer);
            }
        }

        public virtual void SubjectBuffersDisconnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers) {
            if (TextBufferToViewData != null) {
                foreach (var textBuffer in subjectBuffers) {
                    OnTextViewDisconnected(textView, textBuffer);

                    if (TextBufferToViewData.ContainsKey(textBuffer)) {
                        TextViewData viewData = TextBufferToViewData[textBuffer];
                        viewData.RemoveView(textView);

                        if (viewData.AllViews.Count == 0 && _pendingCheckForViewlessTextBuffers == null) {
                            if (reason == ConnectionReason.BufferGraphChange) {
                                // The buffer could be temporarily removed from the view, so don't
                                // immediately check if it's unused - do that after posting a message.
                                _pendingCheckForViewlessTextBuffers = CheckForViewlessTextBuffers;
                                _idleTime.Idle += OnIdle;
                            } else {
                                CheckForViewlessTextBuffers();
                            }
                        }
                    }
                }
            }
        }

        protected void FlushPendingAction() {
            _pendingCheckForViewlessTextBuffers?.Invoke();
        }

        private void OnIdle(object sender, EventArgs e) {
            FlushPendingAction();
            _idleTime.Idle -= OnIdle;
        }
        #endregion

        /// <summary>
        /// Called delayed on the main thread to catch text buffers that don't have a view anymore
        /// </summary>
        private void CheckForViewlessTextBuffers() {
            _pendingCheckForViewlessTextBuffers = null;

            if (TextBufferToViewData != null) {
                var unusedBuffers = (from pair in TextBufferToViewData where pair.Value.AllViews.Count == 0 select pair.Key).ToList();
                foreach (var textBuffer in unusedBuffers) {
                    TextBufferToViewData.Remove(textBuffer);
                    OnTextBufferDisposing(textBuffer);
                }
            }
        }

        private void OnTerminateApp(object sender, EventArgs eventArgs) {
            // CheckForDisposedTextBuffer won't be called when the app terminates, so
            // dispose of each text buffer now.
            if (TextBufferToViewData != null) {
                foreach (var textBuffer in TextBufferToViewData.Keys) {
                    OnTextBufferDisposing(textBuffer);
                }
                TextBufferToViewData.Clear();
            }
            _application.Terminating -= OnTerminateApp;
        }

        /// <summary>
        /// Called when text view is connected to the subject buffer
        /// </summary>
        protected virtual void OnTextViewConnected(ITextView textView, ITextBuffer textBuffer) {
            void GotFocus(object sender, EventArgs eventArgs) {
                OnTextViewGotAggregateFocus(textView, textBuffer);
                textView.GotAggregateFocus -= GotFocus;
            }

            textView.GotAggregateFocus += GotFocus;
            var contentType = textBuffer.ContentType;

            foreach (var listener in _textViewListeners) {
                if (listener.Metadata.ContentTypes != null) {
                    if (listener.Metadata.ContentTypes.Any(curContentType => contentType.IsOfType(curContentType))) {
                        listener.Value.OnTextViewConnected(textView, textBuffer);
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
            var cs = Services.GetService<ICompositionService>();
            var listeners = ComponentLocatorForContentType<ITextViewCreationListener, IComponentContentTypes>.ImportMany(cs, textBuffer.ContentType);

            foreach (var listener in listeners) {
                listener.Value.OnTextViewCreated(textView, textBuffer);
            }
        }

        /// <summary>
        /// Called when text view is disconnected from the subject buffer
        /// </summary>
        protected virtual void OnTextViewDisconnected(ITextView textView, ITextBuffer textBuffer) {
            var contentType = textBuffer.ContentType;
            foreach (var listener in _textViewListeners) {
                if (listener.Metadata.ContentTypes != null) {
                    if (listener.Metadata.ContentTypes.Any(curContentType => contentType.IsOfType(curContentType))) {
                        listener.Value.OnTextViewDisconnected(textView, textBuffer);
                    }
                }
            }
        }

        /// <summary>
        /// Called once for every new text buffer.
        /// </summary>
        protected virtual void OnTextBufferCreated(ITextView textView, ITextBuffer textBuffer) {
            var contentType = textBuffer.ContentType;
            _bufferToOriginalContentType.Add(textBuffer, contentType);

            foreach (var listener in _textBufferListeners) {
                if (listener.Metadata.ContentTypes != null) {
                    if (listener.Metadata.ContentTypes.Any(curContentType => contentType.IsOfType(curContentType))) {
                        listener.Value.OnTextBufferCreated(textBuffer);
                    }
                }
            }
        }

        /// <summary>
        /// Called once per buffer, when it detached from its last view
        /// </summary>
        protected virtual void OnTextBufferDisposing(ITextBuffer textBuffer) {
            // These listeners must be called in the reverse order
            var reverseListeners = new List<ITextBufferListener>();
            IContentType contentType;

            // This has to be obtained from this dictionary, as a content type for this buffer 
            //   could trigger this disposal, and we want to send out notifications based
            //   on the original content type.
            if (_bufferToOriginalContentType.TryGetValue(textBuffer, out contentType)) {
                _bufferToOriginalContentType.Remove(textBuffer);

                foreach (var listener in _textBufferListeners) {
                    if (listener.Metadata.ContentTypes != null) {
                        if (listener.Metadata.ContentTypes.Any(curContentType => contentType.IsOfType(curContentType))) {
                            reverseListeners.Insert(0, listener.Value);
                        }
                    }
                }

                foreach (var listener in reverseListeners) {
                    listener.OnTextBufferDisposed(textBuffer);
                }
            }
        }

        public static IEnumerable<ITextView> GetViewsForBuffer(ITextBuffer textBuffer) {
            var textViewData = GetTextViewDataForBuffer(textBuffer);
            return textViewData != null ? textViewData.AllViews : new ITextView[0];
        }

        public static ITextView GetFirstViewForBuffer(ITextBuffer textBuffer) => GetViewsForBuffer(textBuffer).FirstOrDefault();

        public static TextViewData GetTextViewDataForBuffer(ITextBuffer textBuffer) {
            if (_allInstances != null) {
                foreach (var instance in _allInstances) {
                    if (instance.TextBufferToViewData != null && instance.TextBufferToViewData.TryGetValue(textBuffer, out TextViewData textViewData)) {
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
                foreach (var instance in _allInstances) {
                    instance.FlushPendingAction();
                }
            }
        }
    }
}