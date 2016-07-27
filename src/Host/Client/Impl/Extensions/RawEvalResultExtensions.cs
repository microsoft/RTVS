using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client.Extensions {
    public static class RawEvalResultExtensions {
        public static void SaveRawDataToFile(this REvaluationResult result, string filePath) {
            if (result.RawResult != null) {
                File.WriteAllBytes(filePath, result.RawResult);
            }
        }
    }
}
