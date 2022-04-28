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
        protected override string FeatureAttributeName => throw new NotImplementedException();

        protected override string AdornmentLayerName => nameof(EndBlockHints);

        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public EndBlockHintsAdornmentManagerProvider(
            IThreadingContext threadingContext,
            IViewTagAggregatorFactoryService tagAggregatorFactoryService,
            IClassificationFormatMapService classificationFormatMapService,
            IClassificationTypeRegistryService classificationTypeRegistryService,
            IGlobalOptionService globalOptions,
            IAsynchronousOperationListenerProvider listenerProvider)
            : base(threadingContext, tagAggregatorFactoryService, globalOptions, listenerProvider)
        {
            //_classificationFormatMapService = classificationFormatMapService;
            //_classificationTypeRegistryService = classificationTypeRegistryService;
        }

        protected override void CreateAdornmentManager(IWpfTextView textView)
        {
            // the manager keeps itself alive by listening to text view events.
            _ = new EndBlockHintsAdornmentManager(
                ThreadingContext,
                textView,
                TagAggregatorFactoryService,
                AsyncListener,
                AdornmentLayerName);
        }
    }
}
