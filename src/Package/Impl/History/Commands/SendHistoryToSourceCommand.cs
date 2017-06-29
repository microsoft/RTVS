// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.History.Commands {
    internal class SendHistoryToSourceCommand : ViewCommand {
        private readonly IRInteractiveWorkflowVisual _interactiveWorkflow;
        private readonly IActiveWpfTextViewTracker _textViewTracker;
        private readonly List<IContentType> _contentTypes = new List<IContentType>();
        private readonly IRHistoryVisual _history;

        public SendHistoryToSourceCommand(ITextView textView, IRHistoryProvider historyProvider, IRInteractiveWorkflowVisual interactiveWorkflow, IContentTypeRegistryService contentTypeRegistry, IActiveWpfTextViewTracker textViewTracker)
            : base(textView, RGuidList.RCmdSetGuid, RPackageCommandId.icmdSendHistoryToSource, false) {

            _textViewTracker = textViewTracker;
            _interactiveWorkflow = interactiveWorkflow;
            _history = historyProvider.GetAssociatedRHistory(textView);

            _contentTypes.Add(contentTypeRegistry.GetContentType(RContentTypeDefinition.ContentType));
            _contentTypes.Add(contentTypeRegistry.GetContentType(MdProjectionContentTypeDefinition.ContentType));
        }

        public override CommandStatus Status(Guid guid, int id) {
            return _interactiveWorkflow.ActiveWindow != null && (_history.HasSelectedEntries || !TextView.Selection.IsEmpty) && GetLastActiveRTextView() != null
                ? CommandStatus.SupportedAndEnabled
                : CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            var textView = GetLastActiveRTextView();
            if (textView != null) {
                _history.SendSelectedToTextView(textView);
            }

            return CommandResult.Executed;
        }

        private IWpfTextView GetLastActiveRTextView() {
            foreach (var c in _contentTypes) {
                var textView = _textViewTracker.GetLastActiveTextView(c);
                if (textView != null && !textView.IsClosed && textView.VisualElement.IsVisible) {
                    return textView;
                }
            }
            return null;
        }
    }
}