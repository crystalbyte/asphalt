using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Crystalbyte.Asphalt {
    public sealed class ImageStore {

        private static readonly ImageStore Instance = new ImageStore();

        private readonly Dictionary<string, ImageSource> _imageSources = 
            new Dictionary<string, ImageSource>();

        private ImageStore() {
            // singleton
        }

        public static ImageStore Current { get { return Instance; } }

        public void StoreImage(string path, Stream stream) {
            if (!_imageSources.ContainsKey(path)) {
                _imageSources.Remove(path);
            }

            var image = new BitmapImage();
            image.SetSource(stream);
            _imageSources.Add(path, image);
        }

        public ImageSource GetImage(string path) {
            ImageSource image;
            if (!_imageSources.ContainsKey(path)) {
                image = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
                _imageSources.Add(path, image);
            } else {
                image = _imageSources[path];
            }
            return image;
        }
    }
}
