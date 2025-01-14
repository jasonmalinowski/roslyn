﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.CodeFixes.Suppression;

internal sealed class WrapperCodeFixProvider(IConfigurationFixProvider suppressionFixProvider, IEnumerable<string> diagnosticIds) : CodeFixProvider
{
    private readonly ImmutableArray<string> _originalDiagnosticIds = [.. diagnosticIds.Distinct()];

    public IConfigurationFixProvider SuppressionFixProvider { get; } = suppressionFixProvider;
    public override ImmutableArray<string> FixableDiagnosticIds => _originalDiagnosticIds;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostics = context.Diagnostics.Where(SuppressionFixProvider.IsFixableDiagnostic);

        var documentDiagnostics = diagnostics.Where(d => d.Location.IsInSource).ToImmutableArray();
        if (!documentDiagnostics.IsEmpty)
        {
            var suppressionFixes = await SuppressionFixProvider.GetFixesAsync(context.Document, context.Span, documentDiagnostics, context.CancellationToken).ConfigureAwait(false);
            RegisterSuppressionFixes(context, suppressionFixes);
        }

        var projectDiagnostics = diagnostics.Where(d => !d.Location.IsInSource).ToImmutableArray();
        if (!projectDiagnostics.IsEmpty)
        {
            var suppressionFixes = await SuppressionFixProvider.GetFixesAsync(context.Document.Project, projectDiagnostics, context.CancellationToken).ConfigureAwait(false);
            RegisterSuppressionFixes(context, suppressionFixes);
        }
    }

    private static void RegisterSuppressionFixes(CodeFixContext context, ImmutableArray<CodeFix> suppressionFixes)
    {
        if (!suppressionFixes.IsDefault)
        {
            foreach (var suppressionFix in suppressionFixes)
            {
                context.RegisterCodeFix(suppressionFix.Action, suppressionFix.Diagnostics);
            }
        }
    }

    public override FixAllProvider GetFixAllProvider()
        => SuppressionFixProvider.GetFixAllProvider();
}
