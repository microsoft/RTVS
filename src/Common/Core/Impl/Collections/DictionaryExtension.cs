using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Common.Core {
    public static class DictionaryExtension {
        public static IDictionary<string, object> FromAnonymousObject(object data) {
            IDictionary<string, object> dict;
            if (data != null) {
                dict = data as IDictionary<string, object>;
                if (dict == null) {
                    var attr = BindingFlags.Public | BindingFlags.Instance;
                    dict = new Dictionary<string, object>();
                    foreach (var property in data.GetType().GetProperties(attr)) {
                        if (property.CanRead) {
                            dict.Add(property.Name, property.GetValue(data, null));
                        }
                    }
                }
            }
            else {
                dict = new Dictionary<string, object>();
            }
            return dict;
        }
    }
}
