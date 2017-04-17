using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    public class LocalClientServices : ILocalClientServices {
        public Assembly GetAssemblyByType(Type type) {
            return type.Assembly;
        }
    }
}
