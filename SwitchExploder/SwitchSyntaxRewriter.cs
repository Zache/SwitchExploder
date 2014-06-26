using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SwitchExploder
{
	public class SwitchSyntaxRewriter : CSharpSyntaxRewriter
	{
		readonly SemanticModel _semanticModel;

		public SwitchSyntaxRewriter(SemanticModel semanticModel)
		{
			_semanticModel = semanticModel;
		}

		public override SyntaxNode VisitSwitchStatement(SwitchStatementSyntax node)
		{
			var memberEx = node.Expression as MemberAccessExpressionSyntax;
			if (memberEx == null)
				return node;

			var symbolInfo = _semanticModel.GetTypeInfo(memberEx.Name);
			var enumTypeInfo = symbolInfo.Type;
			if (enumTypeInfo.TypeKind != TypeKind.Enum)
				return node;

			var enumName = enumTypeInfo.Name;
			var nameSpace = enumTypeInfo.ContainingNamespace.Name;
			var enumType = Type.GetType(nameSpace + "." + enumName);
			if (enumType == null)
				return node;

			var possibleNames = Enum.GetNames(enumType);
			var sections = new List<SwitchSectionSyntax>();
			foreach (var name in possibleNames)
			{
				var section = SyntaxFactory.SwitchSection()
					.WithLabels(SyntaxFactory.List(new[]
					{
						SyntaxFactory.SwitchLabel(SyntaxKind.CaseSwitchLabel)
						.WithCaseOrDefaultKeyword(SyntaxFactory.Token(SyntaxKind.CaseKeyword))
						.WithValue(SyntaxFactory.MemberAccessExpression
						(
							SyntaxKind.SimpleMemberAccessExpression,
							SyntaxFactory.IdentifierName(enumName),
							SyntaxFactory.Token(SyntaxKind.DotToken),
							SyntaxFactory.IdentifierName(name)
						))
						.WithColonToken(SyntaxFactory.Token(SyntaxKind.ColonToken))
					}))
					.WithStatements(SyntaxFactory.List<StatementSyntax>(new[]
					{
						SyntaxFactory.BreakStatement()
						.WithBreakKeyword(SyntaxFactory.Token(SyntaxKind.BreakKeyword))
						.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
					}));
				sections.Add(section);
			}

			sections.Add(SyntaxFactory.SwitchSection()
				.WithLabels(SyntaxFactory.List(
					new []
					{
						SyntaxFactory.SwitchLabel(SyntaxKind.DefaultSwitchLabel)
						.WithCaseOrDefaultKeyword(SyntaxFactory.Token(SyntaxKind.DefaultKeyword))
						.WithColonToken(SyntaxFactory.Token(SyntaxKind.ColonToken))
					}))
				.WithStatements(SyntaxFactory.List<StatementSyntax>(
					new []
					{
						SyntaxFactory.ThrowStatement()
						.WithThrowKeyword(SyntaxFactory.Token(SyntaxKind.ThrowKeyword))
						.WithExpression(SyntaxFactory.ObjectCreationExpression(
							SyntaxFactory.ParseTypeName(typeof(ArgumentOutOfRangeException).Name)).WithArgumentList(SyntaxFactory.ArgumentList()))
						.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
					})));

			return node.WithSections(SyntaxFactory.List(sections));
		}
	}
}
