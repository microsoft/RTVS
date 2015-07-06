using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.AST.Statements;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Core.AST.Scopes
{
    /// <summary>
    /// Represents scope that only holds a single statement. The node may
    /// actually have multiple children since single line statement
    /// can be followed by a comment as in 'for(...) statement # comment'
    /// </summary>
    public sealed class SimpleScope : AstNode, IScope
    {
        private Statement statement;
        private string terminatingKeyword;

        #region IScope
        public string Name
        {
            get { return string.Empty; }
        }

        public TokenNode OpenCurlyBrace
        {
            get { return null; }
        }

        public TokenNode CloseCurlyBrace
        {
            get { return null; }
        }

        public IReadOnlyDictionary<string, int> Functions
        {
            get { return StaticDictionary<string, int>.Empty; }
        }

        public IReadOnlyDictionary<string, int> Variables
        {
            get { return StaticDictionary<string, int>.Empty; }
        }
        public IReadOnlyTextRangeCollection<Statement> Statements
        {
            get { return new TextRangeCollection<Statement>() { this.statement }; }
        }
        #endregion

        public SimpleScope(string terminatingKeyword)
        {
            this.terminatingKeyword = terminatingKeyword;
        }

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            this.statement = Statement.Create(context, this);
            if (this.statement != null)
            {
                if(this.statement.Parse(context, this))
                {
                    return base.Parse(context, parent);
                }
            }

            return false;
        }
    }
}
