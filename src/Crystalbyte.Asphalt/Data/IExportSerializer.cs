#region Using directives

using System.Text;

#endregion

namespace Crystalbyte.Asphalt.Data {
    public interface IExportSerializer {
        string FriendlyName { get; }
        string FileExtension { get; }
        void Reset();
        void Serialize(object data, StringBuilder builder);
    }
}