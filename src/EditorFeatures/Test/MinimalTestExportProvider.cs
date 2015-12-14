// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Editor.Shared.Options;
using Microsoft.CodeAnalysis.Editor.UnitTests.Mef;
using Microsoft.CodeAnalysis.Editor.UnitTests.Utilities;
using Microsoft.CodeAnalysis.Shared.Options;
using Microsoft.CodeAnalysis.SolutionCrawler;
using Microsoft.VisualStudio.Composition;
using Roslyn.Test.Utilities;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Editor.UnitTests
{
    public static class MinimalTestExportProvider
    {
        private static readonly Lazy<ComposableCatalog> s_lazyLanguageNeutralCatalog = new Lazy<ComposableCatalog>(() => MefUtilities.CreateAssemblyCatalog(GetVisualStudioAssemblies()).WithParts(MefUtilities.CreateAssemblyCatalog(GetLanguageNeutralTypes().Select(t => t.Assembly).Distinct())));

        public static ComposableCatalog LanguageNeutralCatalog
        {
            get
            {
                return s_lazyLanguageNeutralCatalog.Value;
            }
        }

        public static Type[] GetLanguageNeutralTypes()
        {
            var types = new[]
            {
                // ROSLYN
                typeof(Microsoft.CodeAnalysis.Editor.Implementation.Workspaces.WorkspaceTaskSchedulerFactoryFactory),
                typeof(Microsoft.CodeAnalysis.Host.WorkspaceTaskSchedulerFactoryFactory),
                typeof(Microsoft.CodeAnalysis.Formatting.Rules.DefaultFormattingRuleFactoryServiceFactory),
                typeof(Microsoft.CodeAnalysis.Host.PersistentStorageServiceFactory),
                typeof(Microsoft.CodeAnalysis.Text.Implementation.TextBufferFactoryService.TextBufferCloneServiceFactory),
                typeof(Microsoft.CodeAnalysis.Host.MetadataServiceFactory),
                typeof(Microsoft.CodeAnalysis.Host.TemporaryStorageServiceFactory),
                typeof(Microsoft.CodeAnalysis.Host.TextFactoryService),
                typeof(Microsoft.CodeAnalysis.Editor.Implementation.Workspaces.ProjectCacheHostServiceFactory),
                typeof(Solution), // ServicesCore
                typeof(Microsoft.CodeAnalysis.Options.OptionService), // Service
                typeof(Microsoft.CodeAnalysis.Options.OptionsServiceFactory),
                typeof(Microsoft.CodeAnalysis.Options.Providers.ExportedOptionProvider),
                typeof(Microsoft.CodeAnalysis.Editor.Implementation.SmartIndent.SmartIndentProvider),
                typeof(Microsoft.CodeAnalysis.Editor.Implementation.ForegroundNotification.ForegroundNotificationService),
                typeof(Microsoft.CodeAnalysis.Editor.UnitTests.TestOptionsServiceFactory),
                typeof(SymbolMapping.SymbolMappingServiceFactory),
                typeof(TestWaitIndicator),
                typeof(TestExtensionErrorHandler),
                typeof(TestExportProvider)
            };

            return types.Concat(TestHelpers.GetAllTypesWithStaticFieldsImplementingType(typeof(InternalSolutionCrawlerOptions).Assembly, typeof(Microsoft.CodeAnalysis.Options.IOption)))
                        .Concat(TestHelpers.GetAllTypesWithStaticFieldsImplementingType(typeof(EditorComponentOnOffOptions).Assembly, typeof(Microsoft.CodeAnalysis.Options.IOption)))
                        .Concat(TestHelpers.GetAllTypesWithStaticFieldsImplementingType(typeof(ServiceComponentOnOffOptions).Assembly, typeof(Microsoft.CodeAnalysis.Options.IOption)))
                        .Concat(TestHelpers.GetAllTypesWithStaticFieldsImplementingType(typeof(Microsoft.CodeAnalysis.Formatting.FormattingOptions).Assembly, typeof(Microsoft.CodeAnalysis.Options.IOption)))
                        .Distinct()
                        .ToArray();
        }

        public static IEnumerable<Assembly> GetVisualStudioAssemblies()
        {
            var assemblies = new[]
            {
                // EDITOR

                // Microsoft.VisualStudio.Platform.VSEditor.dll:
                typeof(Microsoft.VisualStudio.Platform.VSEditor.EventArgsHelper).Assembly,

                // Microsoft.VisualStudio.Text.Logic.dll:
                //   Must include this because several editor options are actually stored as exported information 
                //   on this DLL.  Including most importantly, the tab size information.
                typeof(Microsoft.VisualStudio.Text.Editor.DefaultOptions).Assembly,

                // Microsoft.VisualStudio.Text.UI.dll:
                //   Include this DLL to get several more EditorOptions including WordWrapStyle.
                typeof(Microsoft.VisualStudio.Text.Editor.WordWrapStyle).Assembly,

                // Microsoft.VisualStudio.Text.UI.Wpf.dll:
                //   Include this DLL to get more EditorOptions values.
                typeof(Microsoft.VisualStudio.Text.Editor.HighlightCurrentLineOption).Assembly,

                // BasicUndo.dll:
                //   Include this DLL to satisfy ITextUndoHistoryRegistry
                typeof(BasicUndo.IBasicUndoHistory).Assembly,

                // Microsoft.VisualStudio.Language.StandardClassification.dll:
                typeof(Microsoft.VisualStudio.Language.StandardClassification.PredefinedClassificationTypeNames).Assembly
            };

            return assemblies;
        }
    }
}
