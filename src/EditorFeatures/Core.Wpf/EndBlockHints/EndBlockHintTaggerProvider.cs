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
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.CodeAnalysis.EndBlockHints
{
    [Export(typeof(ITaggerProvider))]
    [ContentType(ContentTypeNames.CSharpContentType)]
    [TagType(typeof(EndBlockHintTag))]
    internal class EndBlockHintTaggerProvider : ITaggerProvider
    {
        private readonly IBufferTagAggregatorFactoryService _tagAggregatorFactoryService;
        private readonly IEditorFormatMap _editorFormatMap;

        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public EndBlockHintTaggerProvider(
            IBufferTagAggregatorFactoryService tagAggregatorFactoryService,
            IEditorFormatMapService editorFormatMapService)
        {
            _tagAggregatorFactoryService = tagAggregatorFactoryService;
            _editorFormatMap = editorFormatMapService.GetEditorFormatMap("text");
        }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            var tagAggregator = _tagAggregatorFactoryService.CreateTagAggregator<IStructureTag>(buffer);
            return (ITagger<T>)new EndBlockHintTagger(buffer, tagAggregator, _editorFormatMap);
        }
    }
}
