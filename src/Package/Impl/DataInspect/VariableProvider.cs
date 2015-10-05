using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Controls
{
    public class VariableProvideContext
    {
        public string Environment { get; set; }
    }

    public class VariableProvider
    {
        public IList<Variable> Get(VariableProvideContext context)
        {
            List<Variable> variables = new List<Variable>();

            Populate(variables);

            return variables;
        }

        private void Populate(IList<Variable> collection)
        {
            AddRootLevel(collection, 5);

            Add(collection[0], 3);

            Add(collection[4], 2);
            Add(collection[4].Children[1], 100);
        }

        void AddRootLevel(IList<Variable> collection, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var v = new Variable()
                {
                    VariableName = "ch" + i.ToString(),
                    VariableValue = "value" + i.ToString(),
                    TypeName = "type" + i.ToString(),
                };
                collection.Add(v);
            }
        }

        void Add(Variable parent, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var v = new Variable(parent)
                {
                    VariableName = "ch" + i.ToString(),
                    VariableValue = "value" + i.ToString(),
                    TypeName = "type" + i.ToString(),
                };
                parent.Children.Add(v);
            }
        }
    }
}
