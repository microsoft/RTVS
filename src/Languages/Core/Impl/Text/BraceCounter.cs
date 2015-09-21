using System;
using System.Collections.Generic;

namespace Microsoft.Languages.Core.Text
{
    public class BraceCounter<T> where T : IComparable<T>
    {
        T _openBrace1;
        T _closeBrace1;
        T _openBrace2;
        T _closeBrace2;

        private Stack<T> _braces1 = new Stack<T>();
        private Stack<T> _braces2;

        public BraceCounter(T openCurlyBrace, T closeCurlyBrace) : 
            this(openCurlyBrace, closeCurlyBrace, default(T), default(T))
        {
        }

        public BraceCounter(T openBrace1, T closeBrace1, T openBrace2, T closeBrace2)
        {
            _openBrace1 = openBrace1;
            _closeBrace1 = closeBrace1;

            if (openBrace2 != null)
            {
                _braces2 = new Stack<T>();
                _openBrace2 = openBrace2;
                _closeBrace2 = closeBrace2;
            }
        }

        public int Count
        {
            get { return _braces1.Count; }
        }

        public bool CountBrace(T brace)
        {
            if (0 == brace.CompareTo(_openBrace1))
            {
                _braces1.Push(brace);
            }
            else if (_braces2 != null && 0 == brace.CompareTo(_openBrace2))
            {
                _braces2.Push(brace);
            }
            else if (0 == brace.CompareTo(_closeBrace1))
            {
                if (_braces1.Count > 0)
                    _braces1.Pop();
            }
            else if (_braces2 != null && 0 == brace.CompareTo(_closeBrace2))
            {
                if (_braces2.Count > 0)
                    _braces2.Pop();
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}
