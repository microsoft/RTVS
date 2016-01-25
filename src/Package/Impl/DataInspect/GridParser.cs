using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Parses grid data from R
    /// </summary>
    internal class GridParser {
        private const int VectorIndex = 0;
        private readonly static char[] ValueStart = new char[] { 'c', '\"' };
        private const int ClosingIndex = 1;
        private readonly static char[] ValueDelimiter = new char[] { ',', ')' };

        /// <summary>
        /// Parse grid data from R host and return <see cref="GridData"/>
        /// </summary>
        /// <param name="input">serialized string from R host</param>
        /// <returns>parsed data</returns>
        public static GridData Parse(string input) {
            input = CleanEscape(input);

            //
            // the implementation here is virtually hard-coded
            // R routine uses dput of list that contains four children in order; dimnames, row.names, col.names, data
            // row.names is character vector, col.names is character vector, and data contains is list of columns, which is in turn a named character vector
            // This is for performance, as generic formatting such as json is too expensive
            //
            int current = 0;
            current = input.IndexOf("structure", current);
            current = input.IndexOf('(', current);
            current = input.IndexOf("list", current);
            current = input.IndexOf('(', current);

            current = input.IndexOf("dimnames", current);
            current = input.IndexOf('=', current);
            string dimnamesValue;
            current = FirstQuotedString(input, current, out dimnamesValue);
            var validHeaderNames = (GridData.HeaderNames) Enum.Parse(typeof(GridData.HeaderNames), dimnamesValue);

            List<string> rowNames = new List<string>();
            current = NamedValue(input, "row.names", rowNames, current, true);
            current = input.IndexOf(',', current);

            List<string> columnNames = new List<string>();
            current = NamedValue(input, "col.names", columnNames, current, true);
            current = input.IndexOf(',', current);

            current = input.IndexOf("data", current);
            current = input.IndexOf('=', current);

            current = input.IndexOf("structure", current);
            current = input.IndexOf('(', current);


            List<string> values = new List<string>();
            current = Vector(input, values, current);

            GridData data = new GridData(rowNames, columnNames, values);
            data.ValidHeaderNames = validHeaderNames;

            return data;
        }

        public static string CleanEscape(string input) {
            StringBuilder sb = new StringBuilder();
            char prev = ' ';
            foreach (char ch in input) {
                if (ch == '\\') {
                    if (prev == '\\') {
                        sb.Append(ch);
                        prev = ' ';
                    }
                } else {
                    sb.Append(ch);
                    prev = ch;
                }
            }
            return sb.ToString();
        }

        private static int NamedValue(string input, string name, List<string> names, int current, bool optional = false) {
            int nameIndex = input.IndexOf(name, current);
            if (optional && nameIndex == -1) {
                return current;
            }
            current = input.IndexOf('=', current);

            return Vector(input, names, current);
        }

        private static int Vector(string input, List<string> names, int current) {
            current = input.IndexOfAny(ValueStart, current);
            if (input[current] == ValueStart[VectorIndex]) {
                // vector
                current = input.IndexOf('(', current);

                while (true) {
                    string value;
                    current = FirstQuotedString(input, current, out value);
                    names.Add(value);

                    current = input.IndexOfAny(ValueDelimiter, current);
                    if (input[current] == ValueDelimiter[ClosingIndex]) {
                        current += 1;
                        break;
                    }
                }
            } else {
                // constant
                string value;
                current = FirstQuotedString(input, current, out value);
                names.Add(value);
            }

            return current;
        }

        private static int FirstQuotedString(string input, int startIndex, out string value) {
            int start = input.IndexOf('"', startIndex);

            int end = start + 1;
            while (true) {
                end = input.IndexOf('"', end);
                if (input[end - 1] == '\\') {
                    end++;
                    continue;
                }
                break;
            }

            value = input.Substring(start + 1, end - start - 1);
            return end;
        }
    }
}
