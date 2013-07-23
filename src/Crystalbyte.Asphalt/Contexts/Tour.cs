using System.Collections.ObjectModel;
using System.Data.Linq;
using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Runtime.Serialization;

namespace Crystalbyte.Asphalt.Contexts {

    [DataContract, Table]
    public sealed class Tour : BindingModelBase<Tour> {
        private TourState _state;
        private int _id;
        private DateTime _startTime;
        private DateTime? _stopTime;
        private string _reason;
        private string _destination;
        private int? _vehicleId;
        private TourType _type;

        public Tour() {
            Construct();
        }

        private void Construct() {
            LoadPositions();
            Destination = "Indn Route 5068";
            Reason = "Debugging Asphalt application";
        }

        [OnDeserialized]
        public void OnDeserialized() {
            Construct();
        }

        public DateTime Month {
            get { return new DateTime(StartTime.Year, StartTime.Month, 1); }
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

        [DataMember, Column(CanBeNull = true)]
        public int? VehicleId {
            get { return _vehicleId; }
            set {
                if (_vehicleId == value) {
                    return;
                }

                RaisePropertyChanging(() => VehicleId);
                _vehicleId = value;
                RaisePropertyChanged(() => VehicleId);
            }
        }

        [DataMember, Column(CanBeNull = false)]
        public DateTime StartTime {
            get { return _startTime; }
            set {
                if (_startTime == value) {
                    return;
                }

                RaisePropertyChanging(() => StartTime);
                _startTime = value;
                RaisePropertyChanged(() => StartTime);
            }
        }

        [DataMember, Column(CanBeNull = true)]
        public DateTime? StopTime {
            get { return _stopTime; }
            set {
                if (_stopTime == value) {
                    return;
                }

                RaisePropertyChanging(() => StopTime);
                _stopTime = value;
                RaisePropertyChanged(() => StopTime);
            }
        }

        private void LoadPositions() {
            var id = Id;
            var storage = App.Context.LocalStorage;
            var positions = storage.DataContext.Positions
                .Where(x => x.TourId == id)
                .Select(x => x);

            if (Positions == null) {
                Positions = new ObservableCollection<Position>();
            }

            Positions.AddRange(positions);
        }

        public IList<Position> Positions { get; private set; }

        [DataMember, Column(CanBeNull = true)]
        public string Reason {
            get { return _reason; }
            set {
                if (_reason == value) {
                    return;
                }

                RaisePropertyChanging(() => Reason);
                _reason = value;
                RaisePropertyChanged(() => Reason);
            }
        }

        [DataMember, Column(CanBeNull = true)]
        public string Destination {
            get { return _destination; }
            set {
                if (_destination == value) {
                    return;
                }

                RaisePropertyChanging(() => Destination);
                _destination = value;
                RaisePropertyChanged(() => Destination);
            }
        }

        [DataMember, Column(DbType = "TINYINT NOT NULL")]
        public TourState State {
            get { return _state; }
            set {
                if (_state == value) {
                    return;
                }
                RaisePropertyChanging(() => State);
                _state = value;
                RaisePropertyChanged(() => State);
            }
        }

        [DataMember, Column(DbType = "TINYINT NOT NULL")]
        public TourType Type {
            get { return _type; }
            set {
                if (_type == value) {
                    return;
                }
                RaisePropertyChanging(() => Type);
                _type = value;
                RaisePropertyChanged(() => Type);
            }
        }
    }
}
