using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SwitchExploder
{
	public static class SwitchSyntaxExtensions
	{
		public static SwitchLabelSyntax WithCaseKeyword(this SwitchLabelSyntax switchLabelSyntax)
		{
			return switchLabelSyntax.WithCaseOrDefaultKeyword(SyntaxFactory.Token(SyntaxKind.CaseKeyword));
		}

		public static SwitchLabelSyntax WithColonToken(this SwitchLabelSyntax switchLabelSyntax)
		{
			return switchLabelSyntax.WithColonToken(SyntaxFactory.Token(SyntaxKind.ColonToken));
		}

		public static SwitchSectionSyntax WithBreakStatement(this SwitchSectionSyntax switchSectionSyntax)
		{
			return switchSectionSyntax.WithStatements(SyntaxFactory.List<StatementSyntax>(new[]
			{
				SyntaxFactory.BreakStatement()
				.WithBreakKeyword(SyntaxFactory.Token(SyntaxKind.BreakKeyword))
				.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
			}));
		}

		public static SwitchSectionSyntax WithThrowStatement<TException>(this SwitchSectionSyntax switchSectionSyntax) where TException : Exception
		{
			return switchSectionSyntax.WithStatements(SyntaxFactory.List<StatementSyntax>(
				new[]
				{
					SyntaxFactory.ThrowStatement()
					.WithThrowKeyword(SyntaxFactory.Token(SyntaxKind.ThrowKeyword))
					.WithExpression(
						SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName(typeof(TException).Name))
						.WithArgumentList(SyntaxFactory.ArgumentList()))
						.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
				}));
		}

		public static SwitchSectionSyntax WithDefaultLabel(this SwitchSectionSyntax switchSectionSyntax)
		{
			return switchSectionSyntax.WithLabels(SyntaxFactory.List(
				new[]
				{
					SyntaxFactory.SwitchLabel(SyntaxKind.DefaultSwitchLabel)
					.WithCaseOrDefaultKeyword(SyntaxFactory.Token(SyntaxKind.DefaultKeyword))
					.WithColonToken()
				}));
		}
	}
}
