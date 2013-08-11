using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Crystalbyte.Asphalt.Data;
using Crystalbyte.Asphalt.Resources;

namespace Crystalbyte.Asphalt.Contexts {
    [DataContract, Table]
    public sealed class Driver : BindingModelBase<Driver> {

        private int _id;
        private string _imagePath;
        private ImageSource _image;
        private bool _hasImage;
        private string _forename;
        private string _surname;
        private string _isActive;
        private string _isDefault;

        public Driver() {
            Construct();
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext e) {
            Construct();
        }

        private void Construct() {
            InitializeValidation();
            AddValidationFor(() => Surname)
                .When(x => string.IsNullOrWhiteSpace(x.Surname))
                .Show(AppResources.LicencePlateNotNullOrEmpty);

            AddValidationFor(() => Forename)
                .When(x => string.IsNullOrWhiteSpace(Forename))
                .Show(AppResources.InitialMileageNotNullOrEmpty);
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
        public string Forename {
            get { return _forename; }
            set {
                if (_forename == value) {
                    return;
                }
                RaisePropertyChanging(() => Forename);
                _forename = value;
                RaisePropertyChanged(() => Forename);
            }
        }

        [Column, DataMember]
        public string Surname {
            get { return _surname; }
            set {
                if (_surname == value) {
                    return;
                }
                RaisePropertyChanging(() => Surname);
                _surname = value;
                RaisePropertyChanged(() => Surname);
            }
        }

        [Column, DataMember]
        public string IsActive {
            get { return _isActive; }
            set {
                if (_isActive == value) {
                    return;
                }
                RaisePropertyChanging(() => IsActive);
                _isActive = value;
                RaisePropertyChanged(() => IsActive);
            }
        }

        [Column, DataMember]
        public string IsDefault {
            get { return _isDefault; }
            set {
                if (_isDefault == value) {
                    return;
                }
                RaisePropertyChanging(() => IsDefault);
                _isDefault = value;
                RaisePropertyChanged(() => IsDefault);
            }
        }
    }
}
