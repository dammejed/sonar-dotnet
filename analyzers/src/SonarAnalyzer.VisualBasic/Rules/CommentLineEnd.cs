﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class CommentLineEnd : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S139";
        private const string MessageFormat = "Move this trailing comment on the previous empty line.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private const string DefaultPattern = @"^'\s*\S+\s*$";

        [RuleParameter("legalCommentPattern", PropertyType.String,
            "Pattern for text of trailing comments that are allowed.", DefaultPattern)]
        public string LegalCommentPattern { get; set; } = DefaultPattern;

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxTreeActionInNonGenerated(
                c =>
                {
                    foreach (var token in c.Tree.GetRoot().DescendantTokens())
                    {
                        CheckTokenComments(token, c);
                    }
                });
        }

        private void CheckTokenComments(SyntaxToken token, SyntaxTreeAnalysisContext context)
        {
            var tokenLine = token.GetLocation().GetLineSpan().StartLinePosition.Line;

            var comments = token.TrailingTrivia
                .Where(tr => tr.IsKind(SyntaxKind.CommentTrivia));

            foreach (var comment in comments)
            {
                var location = comment.GetLocation();
                if (location.GetLineSpan().StartLinePosition.Line == tokenLine &&
                    !Regex.IsMatch(comment.ToString(), LegalCommentPattern))
                {
                    context.ReportIssue(Diagnostic.Create(rule, location));
                }
            }
        }
    }
}
