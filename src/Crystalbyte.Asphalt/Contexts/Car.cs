#region Using directives

using System.Data.Linq.Mapping;
using System.Windows.Media;
using Crystalbyte.Asphalt.Resources;

#endregion

namespace Crystalbyte.Asphalt.Contexts {

    [Table]
    public sealed class Car : BindingModelBase<Car> {

        private int _id;
        private string _label;
        private int? _initialMileage;
        private string _licencePlate;
        private string _notes;
        private string _image;

        public Car() {
            InitializeValidation();
        }

        private void InitializeValidation() {
            AddValidationFor(() => Label)
                .When(x => string.IsNullOrWhiteSpace(x.Label))
                .Show(AppResources.LabelNotNullOrEmpty);

            AddValidationFor(() => LicencePlate)
                .When(x => string.IsNullOrWhiteSpace(x.LicencePlate))
                .Show(AppResources.LicencePlateNotNullOrEmpty);

            AddValidationFor(() => InitialMileage)
                .When(x => !x.InitialMileage.HasValue || x.InitialMileage.Value < 0)
                .Show(AppResources.InitialMileageNotNegative);
        }

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

        [Column]
        [Memorize]
        public string Label {
            get { return _label; }
            set {
                if (_label == value) {
                    return;
                }
                RaisePropertyChanging(() => Label);
                _label = value;
                RaisePropertyChanged(() => Label);
            }
        }

        [Column]
        [Memorize]
        public string Image {
            get { return _image; }
            set {
                if (_image == value) {
                    return;
                }

                RaisePropertyChanging(() => Image);
                _image = value;
                RaisePropertyChanged(() => Image);
            }
        }


        [Column]
        [Memorize]
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



        [Column]
        [Memorize]
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



        [Column]
        [Memorize]
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