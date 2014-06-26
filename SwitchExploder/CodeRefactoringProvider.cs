using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Rename;
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

			// Only offer a refactoring if the selected node is a type declaration node.
			var switchStatement = node as SwitchStatementSyntax;
			if (switchStatement == null)
				return null;
			
			// For any type declaration node, create a code action to reverse the identifier text.
			var action = CodeAction.Create("Explode Switch", c => ExplodeSwitch(document, switchStatement, c));

			// Return this code action.
			return new[] { action };
		}

		private async Task<Document> ExplodeSwitch(Document document, SwitchStatementSyntax node, CancellationToken cancellationToken)
		{
			var tree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
			var root = await tree.GetRootAsync(cancellationToken).ConfigureAwait(false);
			var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

			var switchRewriter = new SwitchSyntaxRewriter(semanticModel);
			var newRoot = switchRewriter.Visit(root);

			var newDocument = document.WithSyntaxRoot(newRoot);
            return await Formatter.FormatAsync(newDocument, null, cancellationToken);
		}
	}
}