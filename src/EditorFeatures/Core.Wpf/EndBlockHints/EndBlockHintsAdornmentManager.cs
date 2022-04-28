// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.CodeAnalysis.Editor.Implementation.Adornments;
using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.CodeAnalysis.Shared.TestHooks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.EndBlockHints
{
    internal sealed class EndBlockHintsAdornmentManager : AbstractAdornmentManager<EndBlockHintTag>
    {
        private readonly IClassificationType _classificationType;
        private readonly IClassificationFormatMap _classificationFormatMap;

        public EndBlockHintsAdornmentManager(
            IClassificationTypeRegistryService classificationTypeRegistryService,
            IClassificationFormatMapService classificationFormatMapService,
            IThreadingContext threadingContext,
            IWpfTextView textView,
            IViewTagAggregatorFactoryService tagAggregatorFactoryService,
            IAsynchronousOperationListener asyncListener,
            string adornmentLayerName)
            : base(threadingContext, textView, tagAggregatorFactoryService, asyncListener, adornmentLayerName)
        {
            _classificationType = classificationTypeRegistryService.GetClassificationType(EndBlockHintsFormatDefinition.Name);  // TODO
            _classificationFormatMap = classificationFormatMapService.GetClassificationFormatMap("text");
        }

        protected override void AddAdornmentsToAdornmentLayer_CallOnlyOnUIThread(NormalizedSnapshotSpanCollection changedSpanCollection)
        {
            // this method should only run on UI thread as we do WPF here.
            Contract.ThrowIfFalse(TextView.VisualElement.Dispatcher.CheckAccess());

            var viewSnapshot = TextView.TextSnapshot;
            var viewLines = TextView.TextViewLines;

            foreach (var changedSpan in changedSpanCollection)
            {
                if (!viewLines.IntersectsBufferSpan(changedSpan))
                    continue;

                var tagSpans = TagAggregator.GetTags(changedSpan);
                foreach (var tagMappingSpan in tagSpans)
                {
                    if (!ShouldDrawTag(changedSpan, tagMappingSpan, out _))
                        continue;

                    if (!TryMapToSingleSnapshotSpan(tagMappingSpan.Span, TextView.TextSnapshot, out var span))
                        continue;

                    var line = TextView.GetTextViewLineContainingBufferPosition(span.End);
                    var bounds = line.GetCharacterBounds(span.End);

                    var format = _classificationFormatMap.GetTextProperties(_classificationType);

                    var textBlock = new TextBlock
                    {
                        FontFamily = format.Typeface.FontFamily,
                        FontSize = 0.75 * format.FontRenderingEmSize,
                        FontStyle = FontStyles.Normal,
                        Foreground = format.ForegroundBrush,
                        Text = tagMappingSpan.Tag.Text
                    };

                    textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                    Canvas.SetTop(textBlock, bounds.TextTop + (bounds.TextHeight - textBlock.DesiredSize.Height) / 2);
                    Canvas.SetLeft(textBlock, bounds.Left + 3); // The + 3 is to add some padding

                    AdornmentLayer.AddAdornment(
                        behavior: AdornmentPositioningBehavior.TextRelative,
                        visualSpan: span,
                        tag: null,
                        adornment: textBlock,
                        removedCallback: delegate { });
                }
            }
        }
    }
}
