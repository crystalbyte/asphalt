#region Using directives

using System.Data.Linq.Mapping;
using Crystalbyte.Asphalt.Data;
using Crystalbyte.Asphalt.Resources;
using System.Runtime.Serialization;
using System.Windows.Media;

#endregion

namespace Crystalbyte.Asphalt.Contexts {

    [DataContract, Table]
    public sealed class Vehicle : BindingModelBase<Vehicle> {

        private int _id;
        private int? _initialMileage;
        private string _licencePlate;
        private string _notes;
        private string _imageName;
        private ImageSource _image;

        public Vehicle() {
            AddValidators();
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context) {
            InitializeValidation();
            AddValidators();
        }

        private async void DeleteCurrentImageAsync() {
            var localStorage = App.Composition.GetExport<LocalStorage>();
            await localStorage.DeleteImageAsync(ImageName);
        }

        private async void LoadImageFromPathAsync() {
            var localStorage = App.Composition.GetExport<LocalStorage>();
            Image = await localStorage.GetImageAsync(ImageName);
        }

        private void AddValidators() {
            AddValidationFor(() => LicencePlate)
                .When(x => string.IsNullOrWhiteSpace(x.LicencePlate))
                .Show(AppResources.LicencePlateNotNullOrEmpty);

            AddValidationFor(() => InitialMileage)
                .When(x => !x.InitialMileage.HasValue || x.InitialMileage.Value < 0 || x.InitialMileage > 99999999)
                .Show(AppResources.InitialMileageNotNegative);
        }

        public bool HasImage {
            get { return Image != null; }
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
                RaisePropertyChanging(() => HasImage);
                RaisePropertyChanging(() => Image);
                _image = value;
                RaisePropertyChanged(() => Image);
                RaisePropertyChanged(() => HasImage);
            }
        }

        [Column, DataMember]
        public string ImageName {
            get { return _imageName; }
            set {
                if (_imageName == value) {
                    return;
                }

                if (!string.IsNullOrWhiteSpace(_imageName)) {
                    DeleteCurrentImageAsync();
                }

                RaisePropertyChanging(() => ImageName);
                _imageName = value;
                RaisePropertyChanged(() => ImageName);

                if (string.IsNullOrWhiteSpace(value)) {
                    Image = null;
                } else {
                    LoadImageFromPathAsync();
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
        public int? InitialMileage {
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