
namespace Microsoft.Languages.Core.Text
{
    /// <summary>
    /// Text range that allows attaching of simple properties
    /// </summary>
    public class TextRange<T> : TextRange, ITextRange<T>
    {
        /// <summary>
        /// Creates text range starting at given position and length of zero.
        /// </summary>
        /// <param name="position">Start position</param>
        public TextRange(int position)
            : base(position)
        {
        }

        /// <summary>
        /// Creates text range based on start and end positions.
        /// End is exclusive, Length = End - Start
        /// <param name="start">Range start</param>
        /// <param name="length">Range length</param>
        /// </summary>
        public TextRange(int start, int length)
            : base(start, length)
        {
        }

        /// <summary>
        /// Creates text range based on another text range
        /// </summary>
        /// <param name="range">Text range to use as position source</param>
        public TextRange(ITextRange range)
            : this(range.Start, range.Length)
        {
        }
        
        // ITextRange<T>
        public T Tag { get; set; }
    }
}
