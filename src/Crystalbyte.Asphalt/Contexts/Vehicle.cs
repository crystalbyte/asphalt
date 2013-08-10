#region Using directives

using System.Data.Linq.Mapping;
using System.Windows.Media.Imaging;
using Crystalbyte.Asphalt.Data;
using Crystalbyte.Asphalt.Resources;
using System.Runtime.Serialization;
using System.Windows.Media;

#endregion

namespace Crystalbyte.Asphalt.Contexts {

    [DataContract, Table]
    public sealed class Vehicle : BindingModelBase<Vehicle> {

        private int _id;
        private int _initialMileage;
        private string _licencePlate;
        private string _notes;
        private string _imagePath;
        private ImageSource _image;
        private bool _hasImage;

        public Vehicle() {
            Construct();
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext e) {
            Construct();
        }

        private async void DeleteCurrentImageAsync() {
            var localStorage = App.Composition.GetExport<LocalStorage>();
            await localStorage.DeleteImageAsync(ImagePath);
        }

        private async void LoadImageFromPath() {
            var localStorage = App.Composition.GetExport<LocalStorage>();
            var stream = await localStorage.GetImageStreamAsync(ImagePath);

            SmartDispatcher.InvokeAsync(() => {
                var image = new BitmapImage();
                image.SetSource(stream);
                Image = image;
            });
        }

        private void Construct() {
            InitializeValidation();
            AddValidationFor(() => LicencePlate)
                .When(x => string.IsNullOrWhiteSpace(x.LicencePlate))
                .Show(AppResources.LicencePlateNotNullOrEmpty);

            AddValidationFor(() => InitialMileage)
                .When(x => x.InitialMileage < 0)
                .Show(AppResources.InitialMileageNotNullOrEmpty);
        }

        public bool HasImage {
            get { return _hasImage; }
            set {
                if (_hasImage == value) {
                    return;
                }
                RaisePropertyChanging(() => HasImage);
                _hasImage = value;
                RaisePropertyChanged(() => HasImage);
            }
        }

        [DataMember]
        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity")]
        public int Id {
            get { return _id; }
            set {
                if (_id == value) {
                    return;
                }

                RaisePropertyChanging(() => Id);
                _id = value;
                RaisePropertyChanged(() => Id);
            }
        }

        public ImageSource Image {
            get {
                return _image;
            }
            set {
                if (_image == value) {
                    return;
                }
                RaisePropertyChanging(() => Image);
                _image = value;
                RaisePropertyChanged(() => Image);
                HasImage = _image != null;
            }
        }

        [Column, DataMember]
        public string ImagePath {
            get { return _imagePath; }
            set {
                if (_imagePath == value) {
                    return;
                }

                if (!string.IsNullOrWhiteSpace(_imagePath)) {
                    DeleteCurrentImageAsync();
                }

                RaisePropertyChanging(() => ImagePath);
                _imagePath = value;
                RaisePropertyChanged(() => ImagePath);

                if (string.IsNullOrWhiteSpace(value)) {
                    Image = null;
                } else {
                    LoadImageFromPath();
                }
            }
        }

        [Column, DataMember]
        public string LicencePlate {
            get { return _licencePlate; }
            set {
                if (_licencePlate == value) {
                    return;
                }
                RaisePropertyChanging(() => LicencePlate);
                _licencePlate = value;
                RaisePropertyChanged(() => LicencePlate);
            }
        }

        [Column, DataMember]
        public int InitialMileage {
            get { return _initialMileage; }
            set {
                if (_initialMileage == value) {
                    return;
                }
                RaisePropertyChanging(() => InitialMileage);
                _initialMileage = value;
                RaisePropertyChanged(() => InitialMileage);
            }
        }

        [Column, DataMember]
        public string Notes {
            get { return _notes; }
            set {
                if (_notes == value) {
                    return;
                }
                RaisePropertyChanging(() => Notes);
                _notes = value;
                RaisePropertyChanged(() => Notes);
            }
        }
    }
}
