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
using System.Threading.Tasks;

#endregion

namespace Crystalbyte.Asphalt.Contexts {
    [DataContract, Table]
    public sealed class Driver : BindingModelBase<Driver> {
        private int _id;
        private string _imageName;
        private ImageSource _image;
        private string _forename;
        private string _surname;
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

        public async Task RestoreImageAsync() {
            if (string.IsNullOrWhiteSpace(ImageName)) {
                return;
            }
            var stream = await LocalStorage.GetImageStreamAsync(ImageName);
            var image = new BitmapImage();
            image.SetSource(stream);
            Image = image;
        }

        public bool HasImage {
            get { return _image != null; }
        }

        public string Fullname {
            get { return string.Format("{0} {1}", Forename, Surname); }
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
                RaisePropertyChanging(() => HasImage);
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

                RaisePropertyChanging(() => ImageName);
                _imageName = value;
                RaisePropertyChanged(() => ImageName);
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

        public async void CommitChanges() {
            await Channels.Database.Enqueue(() => LocalStorage.DataContext.SubmitChanges(ConflictMode.FailOnFirstConflict));
        }

        public void InvalidateSelection() {
            RaisePropertyChanged(() => IsSelected);
        }

        public bool IsSelected {
            get {
                return AppContext.Drivers.Aggregate((c, n) => c.SelectionTime > n.SelectionTime ? c : n) == this;
            }
        }

        public string PageHeaderText {
            get { return Id == 0 ? AppResources.AddDriverPageTitle : Forename; }
        }
    }
}