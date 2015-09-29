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
                if (variable != null)
                {
                    collection._variables.Add(variable);
                }
            }

            return collection;
        }

        private static RVariable ParseOneLine(string line)
        {
            line = line.TrimStart();    // remove indentation
            if (line[0] == '$') 
            {
                return null;    // TODO: multiple level support
            }

            int nameDelimiterIndex = line.IndexOf(':');
            if (nameDelimiterIndex < 1)
            {
                throw new FormatException("Variable name should be delimited with colon");
            }

            string name = line.Substring(0, nameDelimiterIndex - 1).Trim();
            string value = line.Substring(nameDelimiterIndex + 1).TrimStart();

            string typeName = GetTypeName(value);

            var variable = new RVariable(typeName, name, value);
            return variable;
        }

        private static string GetTypeName(string valueExpression)
        {
            if (valueExpression.StartsWith("chr")) return "character";
            else if (valueExpression.StartsWith("num")) return "numeric";
            else if (valueExpression.StartsWith("'data.frame'")) return "data.frame";
            else if (valueExpression.StartsWith("int")) return "integer";
            else if (valueExpression.StartsWith("function")) return "function";
            else if (valueExpression.StartsWith("Factor")) return "factor";
            else if (valueExpression.StartsWith("Ord.factor")) return "ordered factor";

            // TODO: it throws to detect more format. Later it must fall back onto default value instead of throwing
            throw new FormatException("Can't understand the value expression");
        }
    }
}
