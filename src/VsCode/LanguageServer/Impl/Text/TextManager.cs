// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using LanguageServer.VsCode.Contracts;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core.Text;
using Microsoft.R.LanguageServer.Documents;
using Microsoft.R.LanguageServer.Extensions;
using Microsoft.R.LanguageServer.Services;

namespace Microsoft.R.LanguageServer.Text {
    internal sealed class TextManager: ITextManager {
        private readonly IIdleTimeNotification _idleTimeNotification;

        public TextManager(IServiceContainer services) {
            Check.ArgumentNull(nameof(services), services);
            _idleTimeNotification = services.GetService<IIdleTimeNotification>();
        }

        public void ProcessTextChanges(DocumentEntry entry, ICollection<TextDocumentContentChangeEvent> contentChanges) {
            _idleTimeNotification.NotifyUserActivity();

            var eb = entry.EditorBuffer;
            foreach (var change in contentChanges) {
                if (!change.HasRange) {
                    continue;
                }
                var position = eb.ToStreamPosition(change.Range.Start);
                var range = new TextRange(position, change.RangeLength);
                if (!string.IsNullOrEmpty(change.Text)) {
                    // Insert or replace
                    if (change.RangeLength == 0) {
                        eb.Insert(position, change.Text);
                    } else {
                        eb.Replace(range, change.Text);
                    }
                } else {
                    eb.Delete(range);
                }
            }

        }
    }
}
