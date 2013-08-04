using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Crystalbyte.Asphalt.Contexts;
using System.Composition;
using System.Data.Linq;
using System.Windows;
using Windows.Storage;
using System.IO;
using System.IO.IsolatedStorage;

namespace Crystalbyte.Asphalt.Data {

    [Export, Shared]
    public sealed class LocalStorage {

        private const string ImagePath = "Images";

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

        public async Task StoreImageAsync(string name, Stream stream) {
            var local = ApplicationData.Current.LocalFolder;
            var imageFolder = await local.GetFolderAsync(ImagePath);

            var file = await imageFolder.CreateFileAsync(name,
            CreationCollisionOption.FailIfExists);

            using (var sr = new BinaryReader(stream)) {
                var bytes = sr.ReadBytes(Convert.ToInt32(stream.Length));
                using (var sw = await file.OpenStreamForWriteAsync()) {
                    await sw.WriteAsync(bytes, 0, bytes.Length);

                    // Manually dispose stream, due to possible framework issue.
                    // http://social.msdn.microsoft.com/Forums/windowsapps/en-US/de83d7b1-2ef2-4e45-8e0c-a7334ecf1ee8/unauthorizedaccessexception-when-usig-storagefoldercreatefileasync-in-windows-8
                    sw.Dispose();
                }
            }

            stream.Dispose();
        }

        public async Task<ImageSource> GetImageAsync(string name) {
            var local = ApplicationData.Current.LocalFolder;
            var imageFolder = await local.GetFolderAsync(ImagePath);

            var store = IsolatedStorageFile.GetUserStoreForApplication();
            if (!store.FileExists(Path.Combine(local.Path, ImagePath, name))) {
                // Image has been deleted
                // TODO: replace for default image
                return null;
            }

            var stream = await imageFolder.OpenStreamForReadAsync(name);
            // Image is corrupt
            if (stream.Length == 0) {
                // TODO: replace for default image
                return null;
            }

            var image = new BitmapImage();
            image.SetSource(stream);
            return image;
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
                Debug.WriteLine("ThreadId: {0}", System.Threading.Thread.CurrentThread.ManagedThreadId);

                DataContext = new AsphaltDataContext(connectionString);
                if (!DataContext.DatabaseExists()) {
                    DataContext.CreateDatabase();
                }
            });
        }
    }
}