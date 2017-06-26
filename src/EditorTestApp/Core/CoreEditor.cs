// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Application.Composition;
using Microsoft.Languages.Editor.Application.Controller;
using Microsoft.Languages.Editor.Application.Host;
using Microsoft.Languages.Editor.Text;
using Microsoft.Languages.Editor.ViewModel;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Application.Core {
    [ExcludeFromCodeCoverage]
    public sealed class CoreEditor {
        public ICommandTarget BaseController { get; private set; }

        private readonly ITextEditorFactoryService _textEditorFactoryService;
        private readonly ITextDocumentFactoryService _textDocumentFactoryService;
        private readonly ITextBufferFactoryService _textBufferFactoryService;
        private readonly IClassificationFormatMapService _classificationFormatMapService;
        private readonly ITextBufferUndoManagerProvider _textBufferUndoManagerProvider;
        private readonly IEditorOperationsFactoryService _editorOperationsFactoryService;
        private readonly IEditorOptionsFactoryService _editorOptionsFactoryService;

        private readonly IServiceContainer _services;
        private readonly string _filePath;

        private IWpfTextViewHost _wpftextViewHost;
        private IContentType _contentType;
        private ITextBufferUndoManager _undoManager;
        private IEditorOperations _editorOperations;
        private IEditorViewModel _editorViewModel;

        public CoreEditor(IServiceContainer services, string text, string filePath, string contentTypeName) {
            _textEditorFactoryService = services.GetService<ITextEditorFactoryService>();
            _textDocumentFactoryService = services.GetService<ITextDocumentFactoryService>();
            _textBufferFactoryService = services.GetService<ITextBufferFactoryService>();
            _classificationFormatMapService = services.GetService<IClassificationFormatMapService>();
            _textBufferUndoManagerProvider = services.GetService<ITextBufferUndoManagerProvider>();
            _editorOperationsFactoryService = services.GetService<IEditorOperationsFactoryService>();
            _editorOptionsFactoryService = services.GetService<IEditorOptionsFactoryService>();

            _services = services;
            _filePath = filePath;

            if (string.IsNullOrEmpty(_filePath) || Path.GetExtension(_filePath).Length == 0) {
                Check.ArgumentNull(nameof(contentTypeName), contentTypeName);
                var contentTypeRegistryService = services.GetService<IContentTypeRegistryService>();
                _contentType = contentTypeRegistryService.GetContentType(contentTypeName);
            }

            CreateTextViewHost(text, filePath);
        }

        #region Properties
        public IWpfTextView View  => _wpftextViewHost?.TextView;

        public ITextBufferUndoManager UndoManager {
            get {
                _undoManager = _undoManager ?? _textBufferUndoManagerProvider.GetTextBufferUndoManager(TextBuffer);
                return _undoManager;
            }
        }

        public IEditorOperations EditorOperations {
            get {
                _editorOperations = _editorOperations ?? _editorOperationsFactoryService.GetEditorOperations(View);
                return _editorOperations;
            }
        }

        private IEditorOptions GlobalOptions => _editorOptionsFactoryService.GlobalOptions;

        /// <summary>
        /// Calling TextBuffer is only valid after instantiating the View
        /// </summary>
        private ITextBuffer TextBuffer {
            get {
                Debug.Assert(_wpftextViewHost != null, "View was not created yet");
                return _wpftextViewHost?.TextView?.TextBuffer;
            }
        }

        private IContentType ContentType {
            get {
                if (_contentType == null) {
                    var ctl = new ContentTypeLocator(_services.GetService<ICompositionService>());
                    _contentType = ctl.FindContentType(_filePath);
                }
                return _contentType;
            }
        }

        #endregion

        private void CreateTextViewHost(string text, string filePath) {
            ITextDataModel textDataModel;
            text = text ?? string.Empty;

            var diskBuffer = _textBufferFactoryService.CreateTextBuffer(text, ContentType);
            var cs = _services.GetService<ICompositionService>();
            _editorViewModel = EditorViewModelFactory.CreateEditorViewModel(diskBuffer, _services);

            if (_editorViewModel != null) {
                textDataModel = new TextDataModel(diskBuffer, _editorViewModel.ViewBuffer.As<ITextBuffer>());
            } else {
                textDataModel = new TextDataModel(diskBuffer, diskBuffer);
            }

            var textBuffer = textDataModel.DocumentBuffer;
            TextDocument = _textDocumentFactoryService.CreateTextDocument(textBuffer, filePath);

            SetGlobalEditorOptions();
            var textView = _textEditorFactoryService.CreateTextView(textDataModel,
                                                                   new DefaultTextViewRoleSet(),
                                                                   GlobalOptions);
            _wpftextViewHost = _textEditorFactoryService.CreateTextViewHost(textView, true);

            ApplyDefaultSettings();
            Control.Content = _wpftextViewHost.HostControl;

            var baseController = new BaseController();
            BaseController = baseController;

            if (_editorViewModel != null) {
                CommandTarget = _editorViewModel.GetCommandTarget(textView.ToEditorView());
                var controller = CommandTarget as Common.Core.UI.Commands.Controller;
                Debug.Assert(controller != null);
                controller.ChainedController = baseController;
            } else {
                CommandTarget = baseController;
            }

            baseController.Initialize(textView, EditorOperations, UndoManager, _services);
        }

        public void Close() {
            if (_wpftextViewHost != null) {
                _wpftextViewHost.Close();
                _wpftextViewHost = null;
            }

            if (_editorViewModel != null) {
                _editorViewModel.Dispose();
                _editorViewModel = null;
            }
        }

        private void SetGlobalEditorOptions() {
            var options = _editorOptionsFactoryService.GlobalOptions;

            options.SetOptionValue("IsCodeLensEnabled", false);

            options.SetOptionValue<bool>(DefaultTextViewOptions.UseVisibleWhitespaceId, true);
            options.SetOptionValue<bool>(DefaultTextViewOptions.BraceCompletionEnabledOptionId, true);

            options.SetOptionValue<bool>(DefaultTextViewHostOptions.LineNumberMarginId, true);
            options.SetOptionValue<bool>(DefaultTextViewHostOptions.OutliningMarginId, true);

            options.SetOptionValue<bool>(DefaultTextViewHostOptions.HorizontalScrollBarId, true);
            options.SetOptionValue<bool>(DefaultTextViewHostOptions.VerticalScrollBarId, true);
        }

        private void ApplyDefaultSettings() {
            var textFormatMap = _classificationFormatMapService.GetClassificationFormatMap("text");
            textFormatMap.DefaultTextProperties = textFormatMap.DefaultTextProperties.SetFontRenderingEmSize(11);
            textFormatMap.DefaultTextProperties = textFormatMap.DefaultTextProperties.SetTypeface(new Typeface("Consolas"));
        }

        public bool HasFocus {
            get {
                if (View != null && View.VisualElement != null && Control != null) {
                    return View.VisualElement.IsKeyboardFocusWithin;
                }
                return false;
            }
        }

        public ContentControl Control { get; } = new ContentControl();

        public ICommandTarget CommandTarget { get; private set; }

        public void Focus() {
            if (View != null && View.VisualElement != null) {
                if (!View.VisualElement.Focus()) {
                    Dispatcher.CurrentDispatcher.BeginInvoke(
                        (Action)(() => View?.VisualElement?.Focus())
                        , DispatcherPriority.ApplicationIdle,
                        null);
                }
            }
        }

        public ITextDocument TextDocument { get; private set; }

        public string Text {
            get => TextBuffer.CurrentSnapshot.GetText();
            set => TextBuffer.Replace(new Span(0, TextBuffer.CurrentSnapshot.Length), value ?? string.Empty);
        }

        public int CurrentColumn {
            get {
                var caretViewLine = View.Caret.ContainingTextViewLine;
                var columnWidth = View.FormattedLineSource.ColumnWidth;
                return (int)Math.Round((View.Caret.Left - caretViewLine.Left) / columnWidth);
            }
        }

        public int CurrentLine {
            get {
                int caretIndex = View.Caret.Position.VirtualBufferPosition.Position;
                return View.TextSnapshot.GetLineNumberFromPosition(caretIndex);
            }
        }

        public void GoTo(int line, int offsetFromStart, int length) {
            var snapshot = View.TextSnapshot;
            if (line >= snapshot.LineCount || line < 0) {
                Debug.Fail("Line number must be between 0 and linecount");
                return;
            }

            var snapshotLine = View.TextSnapshot.GetLineFromLineNumber(line);
            if (offsetFromStart > 0 && (offsetFromStart > snapshotLine.LengthIncludingLineBreak || offsetFromStart < 0)) {
                Debug.Fail("Offset must be between 0 and chars in line");
                return;
            }

            var bufferIntPosition = snapshotLine.Start.Position + offsetFromStart;
            var bufferPosition = new SnapshotPoint(snapshot, bufferIntPosition);

            View.Caret.MoveTo(bufferPosition);
            Debug.Assert(length >= 0, "Length must be 0 or more");
            if (length > 0) {
                var selectionSpan = new SnapshotSpan(bufferPosition, length);
                View.Selection.Select(selectionSpan, false);
            }
            Focus();
        }
    }
}
