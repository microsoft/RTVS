using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Core.Tokens
{
    /// <summary>
    /// Implements <seealso cref="IToken"/>. Derives from <seealso cref="TextRange"/>
    /// </summary>
    /// <typeparam name="T">Token type (typically enum)</typeparam>
    public class Token<T> : TextRange, IToken<T>
    {
        /// <summary>
        /// Create token based on type and text range
        /// </summary>
        /// <param name="tokenType">Token type</param>
        /// <param name="range">Token range in the text provider</param>
        public Token(T tokenType, ITextRange range)
            : base(range)
        {
            this.TokenType = tokenType;
        }

        /// <summary>
        /// Create token based on token type, start and end of the text range.
        /// </summary>
        /// <param name="tokenType">Token type</param>
        /// <param name="start">Range start</param>
        /// <param name="length">Range length</param>
        public Token(T tokenType, int start, int length)
            : base(start, length)
        {
            this.TokenType = tokenType;
        }

        /// <summary>
        /// Create token based on token type, start and end of the text range.
        /// </summary>
        /// <param name="tokenType">Token type</param>
        /// <param name="start">Range start</param>
        /// <param name="end">Range end</param>
        public static Token<T> FromBounds(T tokenType, int start, int end)
        {
            return new Token<T>(tokenType, start, end - start);
        }

        /// <summary>
        /// Token type
        /// </summary>
        public T TokenType { get; protected set; }

        /// <summary>
        /// Arbitrary data attached to the token
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Determines if token is a comment
        /// </summary>
        public virtual bool IsComment
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Determines if token is a string
        /// </summary>
        public virtual bool IsString
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Token is a number
        /// </summary>
        public virtual bool IsNumber
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Token is a punctuator (comma, semicolon, ...)
        /// </summary>
        public virtual bool IsPunctuation
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Token is a language keyword (if, do, while, for, ...)
        /// </summary>
        public virtual bool IsKeyword
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Token is an operator (plus, minus, multiply, ...)
        /// </summary>
        public virtual bool IsOperator
        {
            get
            {
                return false;
            }
        }
    }
}
