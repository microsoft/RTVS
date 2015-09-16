using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Languages.Core.Classification;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Support.Markdown.Tokens
{
    /// <summary>
    /// Composite token that represents R code inside a markdown document
    /// </summary>
    [DebuggerDisplay("[{TokenType} : {Start}...{End}), Length = {Length}")]
    public class MdRCodeToken : MdToken, ICompositeToken
    {
        private ITextProvider _textProvider;
        private ReadOnlyCollection<object> _tokens;

        public MdRCodeToken(int start, int length, ITextProvider textProvider): 
            base(MdTokenType.Code, new TextRange(start, length))
        {
            _textProvider = textProvider;
        }

        public ReadOnlyCollection<object> TokenList
        {
            get
            {
                if (_tokens == null)
                {
                    var rTokenizer = new RTokenizer();
                    var tokens = rTokenizer.Tokenize(_textProvider, Start, Length);
                    var list = new List<object>(tokens);
                    _tokens = new ReadOnlyCollection<object>(list);
                }

                return _tokens;
            }
        }

        public string ContentType
        {
            get { return "R"; }
        }
    }
}
