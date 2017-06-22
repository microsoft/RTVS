// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.SuggestedActions;
using Microsoft.Languages.Editor.Text;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.Markdown.Editor.Settings;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.SuggestedActions {
    internal sealed class RmdSuggestedActionsSource : SuggestedActionsSourceBase, ISuggestedActionsSource {
        private readonly IRMarkdownEditorSettings _settings;
        private bool _caretInRCode;

        private RmdSuggestedActionsSource(ITextView textView, ITextBuffer textBuffer,
            IEnumerable<ISuggestedActionProvider> suggestedActionProviders, IServiceContainer services) :
            base(textView, textBuffer, suggestedActionProviders, services) {
            _settings = services.GetService<IRMarkdownEditorSettings>();
        }


        public static ISuggestedActionsSource Create(ITextView textView, ITextBuffer textBuffer, IServiceContainer services) {
            // Check for detached documents in the interactive window projected buffers
            var cs = services.GetService<ICompositionService>();
            var ctrs = services.GetService<IContentTypeRegistryService>();
            var suggestedActionProviders =
                ComponentLocatorForContentType<ISuggestedActionProvider, IComponentContentTypes>
                    .ImportMany(cs, ctrs.GetContentType(MdContentTypeDefinition.ContentType)).Select(p => p.Value);
            return new RmdSuggestedActionsSource(textView, textBuffer, suggestedActionProviders, services);
        }

        protected override void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e) {
            if (_settings.AutomaticSync) {
                return;
            }
            var caretInRCode = TextView.IsCaretInRCode();
            if (_caretInRCode ^ caretInRCode) {
                _caretInRCode = caretInRCode;
                SuggestedActionsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        #region ISuggestedActionsSource
        public event EventHandler<EventArgs> SuggestedActionsChanged;

        public IEnumerable<SuggestedActionSet> GetSuggestedActions(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken) {
            if (IsDisposed || cancellationToken.IsCancellationRequested || _settings.AutomaticSync) {
                return Enumerable.Empty<SuggestedActionSet>();
            }

            if (!range.Snapshot.TextBuffer.ContentType.TypeName.EqualsOrdinal(MdContentTypeDefinition.ContentType)) {
                return Enumerable.Empty<SuggestedActionSet>();
            }

            var codeBlock = TextView.GetCurrentRCodeBlock();
            if (codeBlock == null) {
                return Enumerable.Empty<SuggestedActionSet>();
            }

            var caretPosition = TextView.Caret.Position.BufferPosition;
            var applicableSpan = codeBlock.ToSpan();

            return SuggestedActionProviders
                .Where(ap => ap.HasSuggestedActions(TextView, TextBuffer, caretPosition))
                .Select(ap => new SuggestedActionSet(ap.GetSuggestedActions(TextView, TextBuffer, caretPosition), applicableToSpan: applicableSpan));
        }

        public Task<bool> HasSuggestedActionsAsync(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
            => (IsDisposed || _settings.AutomaticSync || TextView.GetCurrentRCodeBlock() == null) ? Task.FromResult(false) : Task.FromResult(true);

        public bool TryGetTelemetryId(out Guid telemetryId) {
            telemetryId = MdPackageCommandId.MdCmdSetGuid;
            return true;
        }
        #endregion
    }
}
