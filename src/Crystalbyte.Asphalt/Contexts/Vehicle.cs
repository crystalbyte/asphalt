#region Using directives

using System.Collections.ObjectModel;
using System.Data.Linq.Mapping;
using System.Linq;
using Crystalbyte.Asphalt.Data;
using Crystalbyte.Asphalt.Resources;
using System.Runtime.Serialization;
using System.Windows.Media;
using System.Collections.Generic;

#endregion

namespace Crystalbyte.Asphalt.Contexts {

    [DataContract, Table]
    public sealed class Vehicle : BindingModelBase<Vehicle> {

        private int _id;
        private int _initialMileage;
        private string _licencePlate;
        private string _notes;
        private string _imageName;
        private ImageSource _image;

        public Vehicle() {
            Construct();
        }

        private void Construct() {
            AddValidators();
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context) {
            InitializeValidation();
            Construct();
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
                .When(x => x.InitialMileage < 0 || x.InitialMileage > 99999999)
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

        [DataMember, Column(CanBeNull = true)]
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

        [DataMember, Column(CanBeNull = false)]
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

        [DataMember, Column(CanBeNull = false)]
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

        [DataMember, Column(CanBeNull = true)]
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

        public List<TourGroup> GroupedTours {
            get {
                return Tours
                .OrderByDescending(x => x.StartTime)
                .GroupBy(x => x.Month)
                .Select(x => new TourGroup(x.Key, x))
                .ToList();
            }
        }

        public ObservableCollection<Tour> Tours { get; private set; }

        public int? CurrentMileage {
            get { return 12; }
        }

        public bool IsDataLoaded { get; private set; }

        public void InvalidateData() {
            IsDataLoaded = false;
        }

        public void LoadData() {
            var id = Id;
            var storage = App.Composition.GetExport<LocalStorage>();

            var tours = storage.DataContext.Tours
                .Where(x => x.VehicleId.HasValue && x.VehicleId.Value == id)
                .Select(x => x);

            if (Tours == null) {
                Tours = new ObservableCollection<Tour>();
                Tours.CollectionChanged += (sende, e) => RaisePropertyChanged(() => GroupedTours);
            } else {
                Tours.Clear();
            }

            Tours.AddRange(tours);

            IsDataLoaded = true;

            RaisePropertyChanged(() => GroupedTours);
        }
    }
}