#region Using directives

using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Windows.Media.Imaging;
using Crystalbyte.Asphalt.Data;
using Crystalbyte.Asphalt.Resources;
using System.Runtime.Serialization;
using System.Windows.Media;
using System.Windows.Input;

#endregion

namespace Crystalbyte.Asphalt.Contexts {

    [DataContract, Table]
    public sealed class Vehicle : BindingModelBase<Vehicle> {

        private int _id;
        private double _mileage;
        private string _licencePlate;
        private string _notes;
        private string _imagePath;
        private ImageSource _image;
        private bool _hasImage;
        private DateTime _selectionTime;

        public Vehicle() {
            Construct();

            // Must be set to an SqlCe compatible range.
            SelectionTime = DateTime.Now;
        }

        public Channels Channels {
            get { return App.Composition.GetExport<Channels>(); }
        }

        public LocalStorage LocalStorage {
            get { return App.Composition.GetExport<LocalStorage>(); }
        }

        public AppContext AppContext {
            get { return App.Composition.GetExport<AppContext>(); }
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext e) {
            Construct();
        }

        private async void DeleteCurrentImageAsync() {
            await LocalStorage.DeleteImageAsync(ImagePath);
        }

        public async void RestoreImageFromPath() {
            var stream = await LocalStorage.GetImageStreamAsync(ImagePath);
            if (stream == null || stream.Length == 0) {
                return;
            }

            SmartDispatcher.InvokeAsync(() => {
                var image = new BitmapImage();
                image.SetSource(stream);
                Image = image;
            });
        }

        public void CommitChanges() {
            Channels.Database.Enqueue(() =>
                LocalStorage.DataContext.SubmitChanges(ConflictMode.FailOnFirstConflict));
        }

        private void Construct() {
            InitializeValidation();
            AddValidationFor(() => LicencePlate)
                .When(x => string.IsNullOrWhiteSpace(x.LicencePlate))
                .Show(AppResources.LicencePlateNotNullOrEmpty);

            AddValidationFor(() => Mileage)
                .When(x => x.Mileage < 0)
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

        [Column(UpdateCheck = UpdateCheck.Never), DataMember]
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
                    RestoreImageFromPath();
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
        public double Mileage {
            get { return _mileage; }
            set {
                if (Math.Abs(_mileage - value) < double.Epsilon) {
                    return;
                }
                RaisePropertyChanging(() => Mileage);
                _mileage = value;
                RaisePropertyChanged(() => Mileage);
                CommitChanges();
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

        [Column, DataMember]
        public DateTime SelectionTime {
            get { return _selectionTime; }
            set {
                if (_selectionTime == value) {
                    return;
                }

                RaisePropertyChanging(() => SelectionTime);
                _selectionTime = value;
                RaisePropertyChanged(() => SelectionTime);
                CommitChanges();
            }
        }

        public void InvalidateSelection() {
            RaisePropertyChanged(() => IsSelected);
        }

        public bool IsSelected {
            get {
                return AppContext.Vehicles
                    .Aggregate((c, n) => c.SelectionTime > n.SelectionTime ? c : n) == this;
            }
        }

        public bool IsNew {
            get { return Id == 0; }
        }

        public string PageHeaderText {
            get {
                return Id == 0 ? AppResources.AddVehiclePageTitle : LicencePlate;
            }
        }
    }
}
