using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client.Extensions {
    public static class RawEvalResultExtensions {
        public static bool HasRawData(this REvaluationResult result) {
            return result.Raw?.Count > 0;
        }

        public static void SaveRawDataToFile(this REvaluationResult result, string filepath) {
            if (result.HasRawData()) {
                using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(filepath))) {
                    foreach (var data in result.Raw) {
                        writer.Write(data);
                    }
                    writer.Flush();
                }
            }
        }
    }
}
