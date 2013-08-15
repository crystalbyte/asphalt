#region Using directives

using Crystalbyte.Asphalt.Resources;
using System;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Windows.Storage;

#endregion

namespace Crystalbyte.Asphalt.Data {

    [Export, Shared]
    public sealed class LocalStorage {
        private const string ImagePath = "Images";

        [Import]
        public Channels Channels { get; set; }

        public AsphaltDataContext DataContext { get; private set; }

        public async Task DeleteImageAsync(string name) {

            try {
                var local = ApplicationData.Current.LocalFolder;
                var imageFolder = await local.GetFolderAsync(ImagePath);

                var store = IsolatedStorageFile.GetUserStoreForApplication();
                if (!store.FileExists(Path.Combine(local.Path, ImagePath, name))) {
                    return;
                }
                var file = await imageFolder.GetFileAsync(name);
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (Exception ex) {
                var caption = AppResources.ErrorDeletingImageCaption;
                MessageBox.Show(ex.ToString(), caption, MessageBoxButton.OK);
            }
            
        }

        public async Task StoreImageAsync(string name, Stream stream) {
            try {
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
            catch (Exception ex) {
                var caption = AppResources.ErrorStoringImageCaption;
                MessageBox.Show(ex.ToString(), caption, MessageBoxButton.OK);
            }
            
        }

        public async Task<Stream> GetImageStreamAsync(string name) {
            try {
                var local = ApplicationData.Current.LocalFolder;
                var imageFolder = await local.GetFolderAsync(ImagePath);

                var store = IsolatedStorageFile.GetUserStoreForApplication();
                if (!store.FileExists(Path.Combine(local.Path, ImagePath, name))) {
                    return null;
                }

                var stream = await imageFolder.OpenStreamForReadAsync(name);
                return stream;
            }
            catch (Exception ex) {
                var caption = AppResources.ErrorLoadingImageCaption;
                MessageBox.Show(ex.ToString(), caption, MessageBoxButton.OK);
                return null;
            }
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