using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Composition;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.Mef
{
    public static class MefUtilities
    {
        public static ExportProvider CreateExportProvider(ComposableCatalog catalog)
        {
            var configuration = CompositionConfiguration.Create(catalog.WithDesktopSupport().WithCompositionService());
            var runtimeComposition = RuntimeComposition.CreateRuntimeComposition(configuration);
            return runtimeComposition.CreateExportProviderFactory().CreateExportProvider();
        }

        public static ComposableCatalog CreateAssemblyCatalog(Assembly assembly)
        {
            return CreateAssemblyCatalog(SpecializedCollections.SingletonEnumerable(assembly));
        }

        public static ComposableCatalog CreateAssemblyCatalog(IEnumerable<Assembly> assemblies)
        {
            var discovery = CreatePartDiscovery();

            // If we run CreatePartsAsync on the test thread we may deadlock since it'll schedule stuff back
            // on the thread.
            var parts = Task.Run(async () => await discovery.CreatePartsAsync(assemblies).ConfigureAwait(false)).Result;

            return ComposableCatalog.Create(Resolver.DefaultInstance).AddParts(parts);
        }

        public static ComposableCatalog CreateTypeCatalog(IEnumerable<Type> types)
        {
            var discovery = CreatePartDiscovery();

            // If we run CreatePartsAsync on the test thread we may deadlock since it'll schedule stuff back
            // on the thread.
            var parts = Task.Run(async () => await discovery.CreatePartsAsync(types).ConfigureAwait(false)).Result;

            return ComposableCatalog.Create(Resolver.DefaultInstance).AddParts(parts);
        }

        public static PartDiscovery CreatePartDiscovery()
        {
            return PartDiscovery.Combine(new AttributedPartDiscoveryV1(Resolver.DefaultInstance), new AttributedPartDiscovery(Resolver.DefaultInstance, isNonPublicSupported: true));
        }

        public static ComposableCatalog WithParts(this ComposableCatalog @this, ComposableCatalog catalog)
        {
            return @this.AddParts(catalog.DiscoveredParts);
        }

        public static ComposableCatalog WithParts(this ComposableCatalog catalog, IEnumerable<Type> types)
        {
            return catalog.WithParts(CreateTypeCatalog(types));
        }

        public static ComposableCatalog WithParts(this ComposableCatalog catalog, params Type[] types)
        {
            return WithParts(catalog, (IEnumerable<Type>)types);
        }

        public static ComposableCatalog WithPart(this ComposableCatalog catalog, Type t)
        {
            return catalog.WithParts(CreateTypeCatalog(SpecializedCollections.SingletonEnumerable(t)));
        }
    }
}
