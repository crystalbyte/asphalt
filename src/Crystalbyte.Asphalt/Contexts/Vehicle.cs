﻿#region Using directives

using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Crystalbyte.Asphalt.Data;
using Crystalbyte.Asphalt.Resources;
using System.Threading.Tasks;

#endregion

namespace Crystalbyte.Asphalt.Contexts {
    [DataContract, Table]
    public sealed class Vehicle : BindingModelBase<Vehicle> {
        private int _id;
        private double _mileage;
        private string _licensePlate;
        private string _notes;
        private string _imageName;
        private ImageSource _image;
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

        public async Task RestoreImageAsync() {
            if (string.IsNullOrWhiteSpace(ImageName)) {
                return;
            }
            var stream = await LocalStorage.GetImageStreamAsync(ImageName);
            var image = new BitmapImage();
            image.SetSource(stream);
            Image = image;
        }

        public void CommitChanges() {
            Channels.Database.Enqueue(() => {
                var context = LocalStorage.DataContext;
                try {
                    context.SubmitChanges(ConflictMode.ContinueOnConflict);
                }
                catch (ChangeConflictException) {
                    context.ChangeConflicts.ResolveAll(RefreshMode.KeepCurrentValues);
                }
            });
        }

        private void Construct() {
            InitializeValidation();
            AddValidationFor(() => LicensePlate)
                .When(x => string.IsNullOrWhiteSpace(x.LicensePlate))
                .Show(AppResources.LicensePlateNotNullOrEmpty);

            AddValidationFor(() => Mileage)
                .When(x => x.Mileage < 0)
                .Show(AppResources.InitialMileageNotNullOrEmpty);
        }

        public bool HasImage {
            get { return _image != null; }
        }

        public ImageSource Image {
            get { return _image; }
            set {
                if (_image == value) {
                    return;
                }

                RaisePropertyChanging(() => Image);
                RaisePropertyChanging(() => HasImage);
                _image = value;
                RaisePropertyChanged(() => Image);
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

        [Column(UpdateCheck = UpdateCheck.Never), DataMember]
        public string ImageName {
            get { return _imageName; }
            set {
                if (_imageName == value) {
                    return;
                }

                RaisePropertyChanging(() => ImageName);
                _imageName = value;
                RaisePropertyChanged(() => ImageName);
            }
        }

        [Column, DataMember]
        public string LicensePlate {
            get { return _licensePlate; }
            set {
                if (_licensePlate == value) {
                    return;
                }
                RaisePropertyChanging(() => LicensePlate);
                _licensePlate = value;
                RaisePropertyChanged(() => LicensePlate);
                //if (!IsNew) {
                //    CommitChanges();
                //}
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
                //if (!IsNew) {
                //    CommitChanges();
                //}
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
                //if (!IsNew) {
                //    CommitChanges();
                //}
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
                //if (!IsNew) {
                //    CommitChanges();    
                //}
            }
        }

        public void InvalidateSelection() {
            RaisePropertyChanged(() => IsSelected);
        }

        public bool IsSelected {
            get { return AppContext.Vehicles.Aggregate((c, n) => c.SelectionTime > n.SelectionTime ? c : n) == this; }
        }

        public bool IsNew {
            get { return Id == 0; }
        }

        public string PageHeaderText {
            get { return Id == 0 ? AppResources.AddVehiclePageTitle : LicensePlate; }
        }
    }
}