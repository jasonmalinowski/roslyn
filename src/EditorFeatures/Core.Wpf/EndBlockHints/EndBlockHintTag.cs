// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows.Media;
using Microsoft.CodeAnalysis.Editor.Implementation.Adornments;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.CodeAnalysis.EndBlockHints
{
    internal sealed class EndBlockHintTag : BrushTag
    {
        public EndBlockHintTag(string text, IEditorFormatMap editorFormatMap) : base(editorFormatMap)
        {
            Text = text;
        }

        public string Text { get; }

        protected override Color? GetColor(IWpfTextView view, IEditorFormatMap editorFormatMap)
        {
            return Colors.Gray;
        }
    }
}
