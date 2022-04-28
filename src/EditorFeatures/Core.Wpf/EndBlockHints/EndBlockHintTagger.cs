// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text.Shared.Extensions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.CodeAnalysis.EndBlockHints
{
    internal class EndBlockHintTagger : ITagger<EndBlockHintTag>, IDisposable
    {
        private readonly ITextBuffer _textBuffer;
        private readonly ITagAggregator<IStructureTag> _structureTagAggregator;
        private readonly IEditorFormatMap _editorFormatMap;

        public EndBlockHintTagger(ITextBuffer textBuffer, ITagAggregator<IStructureTag> structureTagAggregator, IEditorFormatMap editorFormatMap)
        {
            _textBuffer = textBuffer;
            _structureTagAggregator = structureTagAggregator;
            _structureTagAggregator.TagsChanged += StructureTagAggregator_TagsChanged;
            _editorFormatMap = editorFormatMap;
        }

        public void Dispose()
        {
            _structureTagAggregator.TagsChanged -= StructureTagAggregator_TagsChanged;
        }

        private void StructureTagAggregator_TagsChanged(object sender, TagsChangedEventArgs e)
        {
            foreach (var span in e.Span.GetSpans(_textBuffer))
            {
                TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(span));
            }
        }

        public event EventHandler<SnapshotSpanEventArgs>? TagsChanged;

        IEnumerable<ITagSpan<EndBlockHintTag>> ITagger<EndBlockHintTag>.GetTags(NormalizedSnapshotSpanCollection spans)
        {
            // We're going to look through our structure tags, and produce hints for ones that meet certain criteria
            foreach (var structureTag in _structureTagAggregator.GetTags(spans))
            {
                if (structureTag.Tag.HeaderSpan.HasValue &&
                    structureTag.Tag.Type is PredefinedStructureTagTypes.Type or
                                             PredefinedStructureTagTypes.Member or
                                             PredefinedStructureTagTypes.Conditional or
                                             PredefinedStructureTagTypes.Namespace or
                                             PredefinedStructureTagTypes.Loop)
                {
                    var snapshot = spans[0].Snapshot;

                    var tagSpans = structureTag.Span.GetSpans(snapshot);

                    // Filter out zero-width spans, since that means we deleted the block
                    if (tagSpans.Count == 1 && tagSpans[0].Length > 0)
                    {
                        // We only want to produce a tag if there's nothing else after the closing }
                        var tagLocation = tagSpans[0].End;
                        if (tagLocation.GetContainingLine().End == tagLocation)
                        {
                            // The tag data for the header can be coming from an older snapshot if we haven't recomputed; migrate it forward so
                            // we're on the current snapshot.
                            var headerSnapshotSpan = structureTag.Tag.Snapshot
                                .GetSpan(structureTag.Tag.HeaderSpan.Value)
                                .TranslateTo(snapshot, SpanTrackingMode.EdgeInclusive);

                            if (headerSnapshotSpan.Length > 0)
                            {
                                yield return new TagSpan<EndBlockHintTag>(
                                    new SnapshotSpan(snapshot, tagLocation, 0),
                                    new EndBlockHintTag(headerSnapshotSpan.GetText(), _editorFormatMap));
                            }
                        }
                    }
                }
            }
        }
    }
}
