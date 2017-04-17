using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Microsoft.R.Host.Client {
    public interface ILocalClientServices {
        Assembly GetAssemblyByType(Type type);
    }
}
