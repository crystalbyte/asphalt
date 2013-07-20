#region Using directives

using System.Data.Linq.Mapping;
using Crystalbyte.Asphalt.Data;
using Crystalbyte.Asphalt.Resources;
using System.Runtime.Serialization;
using System.Windows.Media;
using System.Threading.Tasks;

#endregion

namespace Crystalbyte.Asphalt.Contexts {

    [DataContract, Table]
    public sealed class Vehicle : BindingModelBase<Vehicle> {

        private int _id;
        private int? _initialMileage;
        private string _licencePlate;
        private string _notes;
        private string _imagePath;
        private ImageSource _image;

        public Vehicle() {
            InitializeValidation();
        }

        public override void OnRevive() {
            base.OnRevive();
            InitializeValidation();
        }

        private async void DeleteCurrentImageAsync() {
            var localStorage = App.Composition.GetExport<LocalStorage>();
            await localStorage.DeleteImageAsync(ImagePath);
        }

        private async void LoadImageFromPath() {
            var localStorage = App.Composition.GetExport<LocalStorage>();
            Image = await localStorage.GetImageAsync(ImagePath);
        }

        private void InitializeValidation() {
            AddValidationFor(() => LicencePlate)
                .When(x => string.IsNullOrWhiteSpace(x.LicencePlate))
                .Show(AppResources.LicencePlateNotNullOrEmpty);

            AddValidationFor(() => InitialMileage)
                .When(x => !x.InitialMileage.HasValue || x.InitialMileage.Value < 0)
                .Show(AppResources.InitialMileageNotNegative);
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

        [Memorize]
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
            }
        }

        [Column, Memorize, DataMember]
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

        [Column, Memorize, DataMember]
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

        [Column, Memorize, DataMember]
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

        [Column, Memorize, DataMember]
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