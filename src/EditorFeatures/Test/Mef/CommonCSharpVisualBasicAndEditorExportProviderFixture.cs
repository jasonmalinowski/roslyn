using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Composition;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.Mef
{
    /// <summary>
    /// A test fixture that composes the entire C# and VB workspace and editor assemblies along
    /// with the Visual Studio editor.
    /// </summary>
    public sealed class CommonCSharpVisualBasicAndEditorExportProviderFixture : ExportProviderFixture
    {
        protected override ComposableCatalog GetCatalog()
        {
            return TestExportProvider.CreateAssemblyCatalogWithCSharpAndVisualBasic();
        }
    }
}
