using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.AST.Statements;
using Microsoft.R.Core.AST.Statements.Definitions;
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
        private IStatement _statement;
        private string _terminatingKeyword;

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
        public IReadOnlyTextRangeCollection<IStatement> Statements
        {
            get { return new TextRangeCollection<IStatement>() { _statement }; }
        }
        #endregion

        public SimpleScope(string terminatingKeyword)
        {
            _terminatingKeyword = terminatingKeyword;
        }

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            _statement = Statement.Create(context, this, _terminatingKeyword);
            if (_statement != null)
            {
                if(_statement.Parse(context, this))
                {
                    return base.Parse(context, parent);
                }
            }

            return false;
        }
    }
}
