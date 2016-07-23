using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client.Extensions {
    public static class RawEvalResultExtensions {
        public static void SaveRawDataToFile(this REvaluationResult result, string filepath) {
            if (result.RawResult != null) {
                using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(filepath))) {
                    writer.Write(result.RawResult);
                    writer.Flush();
                }
            }
        }
    }
}
