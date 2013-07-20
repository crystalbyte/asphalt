using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Crystalbyte.Asphalt.Contexts;
using System.Composition;
using System.Data.Linq;
using System.Windows;
using Windows.Storage;
using System.IO;

namespace Crystalbyte.Asphalt.Data {

    [Export, Shared]
    public sealed class LocalStorage {
        private const string ImagePath = "Images";

        private CarDataContext _carDataContext;
        private TourDataContext _tourDataContext;

        public CarDataContext CarDataContext {
            get { return _carDataContext; }
        }

        //public Table<TourContext> Tours {
        //    get { return _tourDataContext.Tours; }
        //}

        public async Task DeleteImageAsync(string name) {
            var local = ApplicationData.Current.LocalFolder;
            var imageFolder = await local.GetFolderAsync(ImagePath);

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
            var stream = await imageFolder.OpenStreamForReadAsync(name);

            // Image has been deleted
            if (stream.Length == 0) {
                return null;
            }

            var image = new BitmapImage();
            image.SetSource(stream);
            return image;
        }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            var connectionString = (string)Application.Current.Resources["DefaultConnectionString"];

            _carDataContext = new CarDataContext(connectionString);
            if (!_carDataContext.DatabaseExists()) {
                _carDataContext.CreateDatabase();
            }

            _tourDataContext = new TourDataContext(connectionString);
            if (!_tourDataContext.DatabaseExists()) {
                _tourDataContext.CreateDatabase();
            }

            var local = ApplicationData.Current.LocalFolder;
            local.CreateFolderAsync(ImagePath, CreationCollisionOption.OpenIfExists);
        }
    }
}