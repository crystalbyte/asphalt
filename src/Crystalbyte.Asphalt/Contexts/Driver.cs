#region Using directives

using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Crystalbyte.Asphalt.Data;
using Crystalbyte.Asphalt.Resources;

#endregion

namespace Crystalbyte.Asphalt.Contexts {
    [DataContract, Table]
    public sealed class Driver : BindingModelBase<Driver> {
        private int _id;
        private string _imagePath;
        private ImageSource _image;
        private bool _hasImage;
        private string _forename;
        private string _surname;
        private string _isDefault;
        private DateTime _selectionTime;

        public Driver() {
            Construct();

            // Must be set to an SqlCe compatible range.
            SelectionTime = DateTime.Now;
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext e) {
            Construct();
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

        private void Construct() {
            InitializeValidation();
            AddValidationFor(() => Surname)
                .When(x => string.IsNullOrWhiteSpace(x.Surname))
                .Show(AppResources.SurnameNotNullOrEmpty);

            AddValidationFor(() => Forename)
                .When(x => string.IsNullOrWhiteSpace(x.Forename))
                .Show(AppResources.ForenameNotNullOrEmpty);
        }

        private async void DeleteCurrentImageAsync() {
            await LocalStorage.DeleteImageAsync(ImagePath);
        }

        public async void RestoreImageFromPath() {
            var stream = await LocalStorage.GetImageStreamAsync(ImagePath);

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
            get { return _image; }
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
                }
                else {
                    RestoreImageFromPath();
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

        public void CommitChanges() {
            Channels.Database.Enqueue(() =>
                                      LocalStorage.DataContext.SubmitChanges(ConflictMode.FailOnFirstConflict));
        }

        public void InvalidateSelection() {
            RaisePropertyChanged(() => IsSelected);
        }

        public bool IsSelected {
            get {
                return AppContext.Drivers
                           .Aggregate((c, n) => c.SelectionTime > n.SelectionTime ? c : n) == this;
            }
        }

        public string PageHeaderText {
            get { return Id == 0 ? AppResources.AddDriverPageTitle : Forename; }
        }
    }
}