using System.Collections.Generic;

namespace Microsoft.Languages.Core.Composition {
    public interface IContentTypeMetadata {
        IEnumerable<string> ContentTypes { get; }
    }
}
