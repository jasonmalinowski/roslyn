using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Composition;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.Mef
{
    public abstract class ExportProviderFixture : IDisposable
    {
        private readonly Lazy<ExportProvider> _exportProvider;

        public ExportProviderFixture()
        {
            _exportProvider = new Lazy<ExportProvider>(
                () => MefUtilities.CreateExportProvider(GetCatalog()));
        }

        public ExportProvider GetExportProvider()
        {
            return _exportProvider.Value;
        }

        protected abstract ComposableCatalog GetCatalog();

        public void Dispose()
        {
            if (_exportProvider.IsValueCreated)
            {
                _exportProvider.Value.Dispose();
            }
        }
    }
}
