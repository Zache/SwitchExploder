using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace SwitchExploder
{
	[ExportCodeRefactoringProvider(CodeRefactoringProvider.RefactoringId, LanguageNames.CSharp)]
	internal class CodeRefactoringProvider : ICodeRefactoringProvider
	{
		public const string RefactoringId = "SwitchExploder";

		public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan textSpan, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var node = root.FindNode(textSpan);
			var switchStatementNode = node as SwitchStatementSyntax;
			if (switchStatementNode == null)
				return null;

			var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

			var memberEx = switchStatementNode.Expression as MemberAccessExpressionSyntax;
			if (memberEx == null)
				return null;

			var symbolInfo = semanticModel.GetTypeInfo(memberEx.Name);
			var enumTypeInfo = symbolInfo.Type;
			if (enumTypeInfo.TypeKind != TypeKind.Enum)
				return null;

			var enumName = enumTypeInfo.Name;
			var nameSpace = enumTypeInfo.ContainingNamespace.Name;
			var enumType = Type.GetType(nameSpace + "." + enumName);
			if (enumType == null)
				return null;

			return new[] { CodeAction.Create("Explode Switch", c => ExplodeSwitch(document, root, semanticModel, switchStatementNode, c)) };
		}

		private async Task<Document> ExplodeSwitch(Document document, 
			SyntaxNode root, 
			SemanticModel semanticModel, 
			SwitchStatementSyntax node, 
			CancellationToken cancellationToken)
		{
			var switchRewriter = new SwitchSyntaxRewriter(semanticModel);
			var newNode = switchRewriter.VisitSwitchStatement(node);

			root = root.ReplaceNode(node, newNode);
			document = document.WithSyntaxRoot(root);

			return await Formatter.FormatAsync(document, null, cancellationToken).ConfigureAwait(false);
		}
	}
}
