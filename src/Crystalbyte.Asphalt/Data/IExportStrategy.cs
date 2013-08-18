#region Using directives

using System.Threading.Tasks;

#endregion

namespace Crystalbyte.Asphalt.Data {
    public interface IExportStrategy {
        string FriendlyName { get; }
        Task ExportAsync(string data, string extension, IProgressAware observer);
    }
}