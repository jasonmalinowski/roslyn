// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.CodeAnalysis.EndBlockHints
{
    internal class EndBlockHintTagger : ITagger<EndBlockHintTag>
    {
        private readonly ITagAggregator<IStructureTag> _structureTagAggregator;

        public EndBlockHintTagger(ITagAggregator<IStructureTag> structureTagAggregator)
        {
            _structureTagAggregator = structureTagAggregator;
        }

        event EventHandler<SnapshotSpanEventArgs> ITagger<EndBlockHintTag>.TagsChanged
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        IEnumerable<ITagSpan<EndBlockHintTag>> ITagger<EndBlockHintTag>.GetTags(NormalizedSnapshotSpanCollection spans)
        {
            // We're going to look through our structure tags, and produce hints for ones that meet certain criteria
            foreach (var structureTag in _structureTagAggregator.GetTags(spans))
            {
                if (structureTag.Tag.Type is PredefinedStructureTagTypes.Type or
                                             PredefinedStructureTagTypes.Member or
                                             PredefinedStructureTagTypes.Conditional or
                                             PredefinedStructureTagTypes.Namespace or
                                             PredefinedStructureTagTypes.Loop)
                {
                    var snapshot = spans[0].Snapshot;

                    var tagSpans = structureTag.Span.GetSpans(snapshot);
                    if (tagSpans.Count == 1)
                    {
                        // We only want to produce a tag if there's nothing else after the closing }
                        var tagLocation = tagSpans[0].End;

                        if (tagLocation.GetContainingLine().End == tagLocation)
                        {
                            yield return new TagSpan(new SnapshotSpan(snapshot, tagLocation, 0), new EndBlockHintTag(structureTag.Tag.Snapshot.GetText(structureTag.Tag.HeaderSpan));
                        }
                    }
                }
            }
        }
    }
}
