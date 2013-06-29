using Crystalbyte.Asphalt.Models;

namespace Crystalbyte.Asphalt.Contexts {
    public class CarContext : NotificationObject {
        private readonly Car _car;
        public CarContext(Car car) {
            _car = car;
        }

        public string Label {
            get { return _car.Label; }
            set {
                if (_car.Label == value) {
                    return;
                }
                _car.Label = value;
                RaisePropertyChanged(() => Label);
            }
        }

        public string LicencePlate {
            get { return _car.LicencePlate; }
            set {
                if (_car.LicencePlate == value) {
                    return;
                }
                _car.LicencePlate = value;
                RaisePropertyChanged(() => LicencePlate);
            }
        }

        public int InitialMileage {
            get { return _car.InitialMileage; }
            set {
                if (_car.InitialMileage == value) {
                    return;
                }
                _car.InitialMileage = value;
                RaisePropertyChanged(() => InitialMileage);
            }
        }

        public string Description {
            get { return _car.Description; }
            set {
                if (_car.Description == value) {
                    return;
                }
                _car.Description = value;
                RaisePropertyChanged(() => Description);
            }
        }
    }
}