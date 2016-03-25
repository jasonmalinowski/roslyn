using System.ComponentModel.Composition;

namespace Microsoft.VisualStudio.LanguageServices.Packaging
{
    /// <summary>
    /// A handshake contract to let the ASP.NET tooling know that we now support giving NuGet suggestions and their code
    /// doesn't need to run anymore. They simply check for this being exported (by the contract name), and if it's there,
    /// it turns their stuff off. Once they've deleted their code entirely, we can delete this.
    /// </summary>
    [Export("RoslynNuGetSuggestions")]
    class RoslynHandshakeContract
    {
    }
}
