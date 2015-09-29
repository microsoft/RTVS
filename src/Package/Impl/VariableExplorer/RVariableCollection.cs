using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.VariableWindow;

namespace Microsoft.VisualStudio.R.Package.VariableExplorer
{
    class RVariableCollection : IImmutableVariableCollection
    {
        List<IVariable> _variables = new List<IVariable>();

        #region IImmutableVariableCollection Support

        public int Count
        {
            get
            {
                return _variables.Count;
            }
        }

        public Task<IVariable> GetAsync(int index, CancellationToken cancellationToken)
        {
            return Task.FromResult(_variables[index]);
        }

        public Task<ICollection<IVariable>> GetManyAsync(int firstIndex, int count, CancellationToken cancellationToken)
        {
            var range = _variables.GetRange(firstIndex, count);
            return Task.FromResult((ICollection<IVariable>)range);
        }

        #endregion

        public static IImmutableVariableCollection Parse(string response)
        {
            if (String.IsNullOrWhiteSpace(response))
            {
                return EmptyImmutableVariableCollection.Instance;
            }

            char[] lineDelimiter = new char[] { '\r', '\n' };
            var lines = response.Split(lineDelimiter, StringSplitOptions.RemoveEmptyEntries);

            var collection = new RVariableCollection();

            for (int i = 0; i < lines.Length; i++)
            {
                RVariable variable = ParseOneLine(lines[i]);
                collection._variables.Add(variable);
            }

            return collection;
        }

        private static RVariable ParseOneLine(string line)
        {
            int nameDelimiterIndex = line.IndexOf(':');
            if (nameDelimiterIndex < 1)
            {
                throw new FormatException("Variable name should be delimited with colon");
            }

            string name = line.Substring(0, nameDelimiterIndex - 1).Trim();
            string value = line.Substring(nameDelimiterIndex + 1).TrimStart();

            return new RVariable(name, value);
        }

        /*
        private static string GetTypeName(string valueExpression)
        {
            if (valueExpression.StartsWith("chr")) return "character";
            else if (valueExpression.StartsWith("num")) return "numeric";
            else if (valueExpression.StartsWith("'data.frame'")) return "data.frame";
            else if (valueExpression.StartsWith(""))
        }*/
    }
}
