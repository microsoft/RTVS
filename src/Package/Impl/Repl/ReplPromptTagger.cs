using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Repl {
    [Export(typeof(IViewTaggerProvider))]
    [ContentType(PredefinedInteractiveContentTypes.InteractiveContentTypeName)]
    [TagType(typeof(ClassificationTag))]
    class ReplPromptTagger : IViewTaggerProvider {
        private readonly ClassificationTag _tag;

        [ImportingConstructor]
        public ReplPromptTagger(IClassificationTypeRegistryService classificationTypeRegistryService) {
            var classificationType = classificationTypeRegistryService.GetClassificationType(ReplClassificationTypes.ReplPromptClassification);
            _tag = new ClassificationTag(classificationType);
        }


        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag {
            return new Tagger(textView, _tag) as ITagger<T>;
        }

        class Tagger : ITagger<ClassificationTag> {
            private readonly ITextView _view;
            private readonly ClassificationTag _tag;
            private static readonly char[] _trimChars = new[] { ' ' };

            public Tagger(ITextView view, ClassificationTag tag) {
                _view = view;
                _tag = tag;
            }

            public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans) {
                var firstSpan = spans.FirstOrDefault();
                if (firstSpan.Snapshot != null) {
                    var window = firstSpan.Snapshot.TextBuffer.GetInteractiveWindow();
                    if (window == null || !(window.Evaluator is RInteractiveEvaluator)) {
                        yield break;
                    }
                }

                foreach (var span in spans) {
                    // prompts are inserted directly into the input buffer as inert text
                    var matches = _view.BufferGraph.MapDownToFirstMatch(span, SpanTrackingMode.EdgeInclusive, x => x.TextBuffer.ContentType.IsOfType("inert"));
                    foreach (var match in matches) {
                        var trimmedLength = match.GetText().TrimEnd(_trimChars).Length;
                        var targetSpan = new SnapshotSpan(match.Snapshot, match.Start, trimmedLength);

                        foreach (var upperMatch in _view.BufferGraph.MapUpToBuffer(targetSpan, SpanTrackingMode.EdgeInclusive, span.Snapshot.TextBuffer)) {
                            yield return new TagSpan<ClassificationTag>(
                                upperMatch,
                                _tag
                            );
                        }
                    }
                }
            }

            public event EventHandler<SnapshotSpanEventArgs> TagsChanged {
                add { }
                remove { }
            }
        }
    }
}
