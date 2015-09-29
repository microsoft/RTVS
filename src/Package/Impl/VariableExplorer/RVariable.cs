using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.VariableWindow;

namespace Microsoft.VisualStudio.R.Package.VariableExplorer
{
    class RVariable : IVariable
    {
        private readonly string _representation;
        private Lazy<List<RVariable>> _children = new Lazy<List<RVariable>>();

        /// <summary>
        /// create new instance of <see cref="RVariable"/>
        /// </summary>
        /// <param name="typeName">variable's type name</param>
        /// <param name="expression">variable's string representation a.k.a. name</param>
        /// <param name="represenstation">variable's representation in one line</param>
        public RVariable(string typeName, string expression, string represenstation)
        {
            this.TypeName = typeName;
            this.Expression = expression;
            _representation = represenstation;
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
            if (_representation != null)
            {
                if (_representation.Length > maximumLength)
                {
                    return Task.FromResult(_representation.Substring(maximumLength));
                }
                return Task.FromResult(_representation);
            }

            Debug.Fail("You should not reach here");
            return Task.FromResult(string.Empty);
        }

        #endregion IVariable Support

        public void Add(RVariable child)
        {
            _children.Value.Add(child);
        }

        public override string ToString()
        {
            // TODO: find better format
            return string.Format("{0} {1}", TypeName, Expression);
        }
    }
}
