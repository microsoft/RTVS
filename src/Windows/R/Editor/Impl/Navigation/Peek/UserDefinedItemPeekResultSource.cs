// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Threading;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Navigation.Peek {
    internal sealed class UserDefinedItemPeekResultSource : IPeekResultSource {
        private readonly UserDefinedPeekItem _peekItem;

        public UserDefinedItemPeekResultSource(UserDefinedPeekItem peekItem) {
            _peekItem = peekItem;
        }

        public void FindResults(string relationshipName,
                                IPeekResultCollection resultCollection,
                                CancellationToken cancellationToken,
                                IFindPeekResultsCallback callback) {
            if (relationshipName == PredefinedPeekRelationships.Definitions.Name) {

                using (var displayInfo = new PeekResultDisplayInfo(
                    label: _peekItem.DisplayName,
                    labelTooltip: _peekItem.FileName,
                    title: Path.GetFileName(_peekItem.FileName),
                    titleTooltip: _peekItem.FileName)) {
                    var result = _peekItem.PeekResultFactory.Create
                    (
                        displayInfo,
                        _peekItem.FileName,
                        new Span(_peekItem.DefinitionNode.Start, _peekItem.DefinitionNode.Length),
                        _peekItem.DefinitionNode.Start,
                        false
                    );

                    resultCollection.Add(result);
                    callback.ReportProgress(1);
                }
            }
        }
    }
}
