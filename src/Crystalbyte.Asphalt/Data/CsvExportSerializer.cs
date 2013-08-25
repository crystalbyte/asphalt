#region Using directives

using System;
using System.Composition;
using System.Linq;
using System.Text;
using Crystalbyte.Asphalt.Contexts;

#endregion

namespace Crystalbyte.Asphalt.Data {

    [Shared]
    [Export(typeof (IExportSerializer))]
    public sealed class CsvExportSerializer : IExportSerializer {
        [Import]
        public ExportContext ExportContext { get; set; }

        private bool _headerWritten;

        #region Implementation of IExportSerializer

        public string FriendlyName {
            get { return "CSV (Comma-separated values)"; }
        }

        public string FileExtension {
            get { return "csv"; }
        }

        public void Reset() {
            _headerWritten = false;
        }

        public void Serialize(object data, StringBuilder builder) {
            var exports = ExportContext.CollectDataExports(data);

            if (!_headerWritten) {
                foreach (var key in exports.Keys) {
                    builder.Append(key);
                    builder.Append(",");
                }

                builder.AppendLine();
                _headerWritten = true;
            }

            foreach (var value in exports.Select(export => MakeValueCsvFriendly(export.Value))) {
                builder.Append(value);
                builder.Append(";");
            }

            builder.Remove(builder.Length - 1, 1);
            builder.AppendLine();
        }

        #endregion

        /// <summary>
        ///   Formats the value so it can be read by common csv parsers.
        ///   Taken from: http://www.saramgsilva.com/index.php/2012/export-to-csv-windows-8-store-apps/
        /// </summary>
        /// <param name="value"> The value to be formatted. </param>
        /// <returns> The formatted value. </returns>
        private static string MakeValueCsvFriendly(object value) {
            if (value == null)
                return string.Empty;

            if (value is DateTime) {
                return ((DateTime) value).ToString(
                    Math.Abs(((DateTime) value).TimeOfDay.TotalSeconds) < double.Epsilon
                        ? "yyyy-MM-dd"
                        : "yyyy-MM-dd HH:mm:ss");
            }

            var output = value.ToString();
            if (output.Contains(",") || output.Contains("\""))
                output = '"' + output.Replace("\"", "\"\"") + '"';
            return output;
        }
    }
}