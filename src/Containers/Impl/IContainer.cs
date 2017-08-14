using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.R.Containers {
    public interface IContainer {
        string Id { get; }
        string Name { get; }
    }
}
