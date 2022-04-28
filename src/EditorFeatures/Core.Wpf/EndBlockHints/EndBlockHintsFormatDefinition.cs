// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.CodeAnalysis.EndBlockHints
{
    [Export(typeof(EditorFormatDefinition))]
    [Name(Name)]
    [UserVisible(true)]
    internal class EndBlockHintsFormatDefinition : EditorFormatDefinition
    {
        public const string Name = nameof(EndBlockHintsFormatDefinition);

        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public EndBlockHintsFormatDefinition()
        {
            DisplayName = EditorFeaturesWpfResources.End_Block_Hints;
            BackgroundCustomizable = false;
            ForegroundBrush = new SolidColorBrush(Colors.DarkGray);
        }

        [Export]
        [Name(Name)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        internal ClassificationTypeDefinition? ClassificationDefinition;
    }
}
