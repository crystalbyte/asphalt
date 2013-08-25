#region Using directives

using System;

#endregion

namespace Crystalbyte.Asphalt.Data {
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class DataExportAttribute : Attribute {
        public DataExportAttribute() : this("{0}") {}

        public DataExportAttribute(string format) {
            Format = format;
        }

        public string Format { get; set; }
        public int Position { get; set; }
    }
}