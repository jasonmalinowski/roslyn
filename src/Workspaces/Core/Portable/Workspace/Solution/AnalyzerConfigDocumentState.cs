using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis
{
    internal sealed class AnalyzerConfigDocumentState : TextDocumentState
    {
        private AnalyzerConfigDocumentState(
            SolutionServices solutionServices,
            IDocumentServiceProvider documentServiceProvider,
            DocumentInfo.DocumentAttributes attributes,
            SourceText sourceTextOpt,
            ValueSource<TextAndVersion> textAndVersionSource)
            : base(solutionServices, documentServiceProvider, attributes, sourceTextOpt, textAndVersionSource)
        {
        }

        public AnalyzerConfigDocumentState(
            DocumentInfo documentInfo,
            SolutionServices solutionServices)
            : base(documentInfo, solutionServices)
        {
        }
    }
}
