// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Editor;
using Microsoft.CodeAnalysis.Editor.Implementation.Adornments;
using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Shared.TestHooks;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.CodeAnalysis.EndBlockHints
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType(ContentTypeNames.RoslynContentType)]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal class EndBlockHintsAdornmentManagerProvider : AbstractAdornmentManagerProvider<EndBlockHintTag>
    {
        private const string LayerName = nameof(EndBlockHints);

        [Export]
        [Name(LayerName)]
        [ContentType(ContentTypeNames.RoslynContentType)]
        [Order(After = PredefinedAdornmentLayers.Selection, Before = PredefinedAdornmentLayers.Squiggle)]
        internal readonly AdornmentLayerDefinition? AdornmentLayer;

        protected override string FeatureAttributeName => FeatureAttribute.EndBlockHints;

        protected override string AdornmentLayerName => LayerName;

        private readonly IClassificationTypeRegistryService _classificationTypeRegistryService;
        private readonly IClassificationFormatMapService _classificationFormatMapService;

        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public EndBlockHintsAdornmentManagerProvider(
            IThreadingContext threadingContext,
            IViewTagAggregatorFactoryService tagAggregatorFactoryService,
            IClassificationTypeRegistryService classificationTypeRegistryService,
            IClassificationFormatMapService classificationFormatMapService,
            IGlobalOptionService globalOptions,
            IAsynchronousOperationListenerProvider listenerProvider)
            : base(threadingContext, tagAggregatorFactoryService, globalOptions, listenerProvider)
        {
            _classificationTypeRegistryService = classificationTypeRegistryService;
            _classificationFormatMapService = classificationFormatMapService;
        }

        protected override void CreateAdornmentManager(IWpfTextView textView)
        {
            // the manager keeps itself alive by listening to text view events.
            _ = new EndBlockHintsAdornmentManager(
                _classificationTypeRegistryService,
                _classificationFormatMapService,
                ThreadingContext,
                textView,
                TagAggregatorFactoryService,
                AsyncListener,
                AdornmentLayerName);
        }
    }
}
