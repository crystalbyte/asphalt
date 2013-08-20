#region Using directives

using System;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Crystalbyte.Asphalt.Resources;
using Windows.Storage;

#endregion

namespace Crystalbyte.Asphalt.Data {
    [Export, Shared]
    public sealed class LocalStorage {
        public const string ImagePath = "images";

        [Import]
        public Channels Channels { get; set; }

        public AsphaltDataContext DataContext { get; private set; }

        public async Task DeleteImageAsync(string name) {
            var local = ApplicationData.Current.LocalFolder;
            var imageFolder = await local.GetFolderAsync(ImagePath);

            var store = IsolatedStorageFile.GetUserStoreForApplication();
            if (!store.FileExists(Path.Combine(local.Path, ImagePath, name))) {
                return;
            }
            var file = await imageFolder.GetFileAsync(name);
            await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
        }

        public async Task DeleteExportAsync(string name) {
            var local = ApplicationData.Current.LocalFolder;
            var shared = await local.GetFolderAsync("shared");
            var transfers = await shared.GetFolderAsync("transfers");
            var file = await transfers.GetFileAsync(name);
            await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
        }

        public async Task SaveExportAsync(string name, string text) {
            var local = ApplicationData.Current.LocalFolder;
            var shared = await local.GetFolderAsync("shared");
            var transfers = await shared.GetFolderAsync("transfers");
            var file = await transfers.CreateFileAsync(name);

            var bytes = Encoding.UTF8.GetBytes(text);
            using (var sw = await file.OpenStreamForWriteAsync()) {
                await sw.WriteAsync(bytes, 0, bytes.Length);
                await sw.FlushAsync();
            }
        }

        public async Task SaveImageAsync(string name, Stream stream) {
            var local = ApplicationData.Current.LocalFolder;
            var imageFolder = await local.GetFolderAsync(ImagePath);

            var file = await imageFolder.CreateFileAsync(name,
                                                         CreationCollisionOption.FailIfExists);

            using (var sr = new BinaryReader(stream)) {
                var bytes = sr.ReadBytes(Convert.ToInt32(stream.Length));
                using (var sw = await file.OpenStreamForWriteAsync()) {
                    await sw.WriteAsync(bytes, 0, bytes.Length);
                    await sw.FlushAsync();
                }
            }
        }

        public async Task<Stream> GetImageStreamAsync(string name) {
            var local = ApplicationData.Current.LocalFolder;
            var imageFolder = await local.GetFolderAsync(ImagePath);
            var stream = await imageFolder.OpenStreamForReadAsync(name);
            return stream;
        }

        [OnImportsSatisfied]
        public async void OnImportsSatisfied() {
            await InitializeStorage();
        }

        private async Task InitializeStorage() {
            var connectionString = (string)Application.Current.Resources["DefaultConnectionString"];
            await IntializeDatabaseAsync(connectionString);

            var local = ApplicationData.Current.LocalFolder;
            await local.CreateFolderAsync(ImagePath, CreationCollisionOption.OpenIfExists);
        }

        private Task IntializeDatabaseAsync(string connectionString) {
            return Channels.Database.Enqueue(() => {
                Debug.WriteLine("ThreadId: {0}", Thread.CurrentThread.ManagedThreadId);

                DataContext = new AsphaltDataContext(connectionString);
                if (!DataContext.DatabaseExists()) {
                    DataContext.CreateDatabase();
                }
            });
        }
    }
}