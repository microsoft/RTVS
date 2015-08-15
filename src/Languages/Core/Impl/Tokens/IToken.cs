using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Core.Tokens
{
    /// <summary>
    /// Describes a parse token. Parse token is a text range
    /// with a type that describes nature of the range.
    /// Derives from <seealso cref="ITextRange"/>
    /// </summary>
    /// <typeparam name="T">Token type (typically enum)</typeparam>
    public interface IToken<T>: ITextRange
    {
        /// <summary>
        /// Type of the token
        /// </summary>
        T TokenType { get; }

        /// <summary>
        /// Arbitrary data attached to the token
        /// </summary>
        object Tag { get; set; }
    }
}
