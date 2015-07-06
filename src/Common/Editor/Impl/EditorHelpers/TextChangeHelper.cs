using System.Diagnostics;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.EditorHelpers
{
    /// <summary>
    /// A single range of difference between two strings
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    public struct SingleTextChange
    {
        public int Position { get; set; }
        public int DeletedLength { get; set; }
        public int InsertedLength { get; set; }
    }

    public static class TextChangeHelper
    {
        /// <summary>
        /// Updates the text in a text buffer by making a single minimal change
        /// </summary>
        public static bool ApplyNewText(ITextBuffer textBuffer, string fullNewBufferText)
        {
            SingleTextChange change = TextChangeHelper.FindSingleTextChange(textBuffer.CurrentSnapshot.GetText(), fullNewBufferText);

            return ApplySingleChange(textBuffer, fullNewBufferText, change);
        }

        public static bool ApplySingleChange(ITextBuffer textBuffer, string fullNewBufferText, SingleTextChange change)
        {
            if (change.DeletedLength > 0 || change.InsertedLength > 0)
            {
                textBuffer.Replace(
                    new Span(change.Position, change.DeletedLength),
                    fullNewBufferText.Substring(change.Position, change.InsertedLength));

                return true;
            }

            return false;
        }

        /// <summary>
        /// Finds a single range of difference between two strings
        /// </summary>
        public static SingleTextChange FindSingleTextChange(string oldText, string newText)
        {
            return FindSingleTextChange(oldText, 0, oldText.Length, newText, 0, newText.Length);
        }

        /// <summary>
        /// Finds a single range of difference between two substrings. The returned change position
        /// is relative to the start of each substring.
        /// </summary>
        public static SingleTextChange FindSingleTextChange(
            string oldText, int oldSubstringStart, int oldSubstringLength,
            string newText, int newSubstringStart, int newSubstringLength)
        {
            Debug.Assert(oldSubstringStart >= 0 && oldSubstringLength >= 0 && oldSubstringStart + oldSubstringLength <= oldText.Length);
            Debug.Assert(newSubstringStart >= 0 && newSubstringLength >= 0 && newSubstringStart + newSubstringLength <= newText.Length);

            SingleTextChange change = new SingleTextChange();
            int sameAtStart = 0;
            int sameAtEnd = 0;

            // Find out how many unchanged chars are at the start of the text

            for (int i = oldSubstringStart, h = newSubstringStart;
                i < oldSubstringStart + oldSubstringLength && h < newSubstringStart + newSubstringLength;
                i++, h++, sameAtStart++)
            {
                if (oldText[i] != newText[h])
                {
                    break;
                }
            }

            if (sameAtStart != oldSubstringLength || sameAtStart != newSubstringLength)
            {
                // Find out how many unchanged chars are at the end of the text

                for (int i = oldSubstringStart + oldSubstringLength - 1, h = newSubstringStart + newSubstringLength - 1;
                    i >= oldSubstringStart + sameAtStart && h >= newSubstringStart + sameAtStart;
                    i--, h--, sameAtEnd++)
                {
                    if (oldText[i] != newText[h])
                    {
                        break;
                    }
                }

                Debug.Assert(sameAtStart + sameAtEnd <= oldSubstringLength);
                Debug.Assert(sameAtStart + sameAtEnd <= newSubstringLength);

                change.Position = sameAtStart;
                change.DeletedLength = oldSubstringLength - sameAtStart - sameAtEnd;
                change.InsertedLength = newSubstringLength - sameAtStart - sameAtEnd;
            }

            return change;
        }

        /// <summary>
        /// This is used during ITextBuffer change events to convert multiple changes
        /// into a single change.
        /// </summary>
        public static SingleTextChange ConvertToSingleTextChange(INormalizedTextChangeCollection changes)
        {
            SingleTextChange singleChange = new SingleTextChange();

            if (changes.Count > 0)
            {
                ITextChange firstChange = changes[0];
                ITextChange lastChange = changes[changes.Count - 1];

                Debug.Assert(firstChange.OldPosition == firstChange.NewPosition);

                singleChange.Position = firstChange.OldPosition;
                singleChange.DeletedLength = lastChange.OldEnd - firstChange.OldPosition;
                singleChange.InsertedLength = lastChange.NewEnd - firstChange.OldPosition;
            }

            return singleChange;
        }
    }
}
