using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.VariableWindow;

namespace Microsoft.VisualStudio.R.Package.VariableExplorer
{
    class RVariable : IVariable
    {
        /// <summary>
        /// create new instance of <see cref="RVariable"/>
        /// </summary>
        /// <param name="typeName">variable's type name</param>
        /// <param name="expression">variable's string representation a.k.a. name</param>
        public RVariable(string typeName, string expression)
        {
            this.TypeName = typeName;
            this.Expression = expression;
        }

        #region IVariable Support

        public string Expression
        {
            get;
            private set;
        }

        public bool HasNoChildren
        {
            get
            {
                // TODO: children variables are not supported yet
                return true;
            }
        }

        public Guid ImageMonikerGuid
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int ImageMonikerId
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsValueReadOnly
        {
            get
            {
                // TODO: editable variable later
                return true;
            }
        }

        public string TypeName
        {
            get;
            private set;
        }

        public Task<IImmutableVariableCollection> GetChildrenAsync(CancellationToken cancellationToken)
        {
            // TODO: it doesn't support children variables yet.
            return Task.FromResult(EmptyImmutableVariableCollection.Instance);
        }

        public Task SetValueAsync(string valueExpression)
        {
            throw new NotImplementedException();
        }

        public Task<string> ToPlainTextAsync(int maximumLength, CancellationToken cancellationToken)
        {
            return Task.FromResult("test value");
        }

        #endregion IVariable Support

        public override string ToString()
        {
            // TODO: find better format
            return string.Format("{0} {1}", TypeName, Expression);
        }
    }
}
