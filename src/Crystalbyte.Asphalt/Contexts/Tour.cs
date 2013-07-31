using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Runtime.Serialization;

namespace Crystalbyte.Asphalt.Contexts {

    [DataContract, Table]
    public sealed class Tour : BindingModelBase<Tour> {
        private int _id;
        private TourState _state;
        private DateTime _startTime;
        private DateTime? _stopTime;
        private string _reason;
        private string _destination;
        private TourType _type;
        private string _origin;
        private bool _isEditing;

        public Tour() {
            Construct();
        }

        private void Construct() {
            LoadData();
            Destination = "Subway Ltd.";
            Origin = "Indn Route 5068";
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

        public bool IsDataLoaded {
            get { return Positions.Count > 0; }
        }

        public void LoadData() {
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
        public string Origin {
            get { return _origin; }
            set {
                if (_origin == value) {
                    return;
                }

                RaisePropertyChanging(() => Origin);
                _origin = value;
                RaisePropertyChanged(() => Origin);
            }
        }

        public string OriginCoordinates {
            get {
                var begin = Positions.First();
                return string.Format("lat: {0}, lon: {1}", begin.Latitude, begin.Longitude);
            }
        }

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

        public string DestinationCoordinates {
            get {
                var end = Positions.Last();
                return string.Format("lat: {0}, lon: {1}", end.Latitude, end.Longitude);
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

        public IEnumerable<TourType> TourTypeSource {
            get { return Enum.GetValues(typeof(TourType)).OfType<TourType>(); }
        }

        public bool IsEditing {
            get { return _isEditing; }
            set {
                if (_isEditing == value) {
                    return;
                }
                RaisePropertyChanging(() => IsEditing);
                _isEditing = value;
                RaisePropertyChanged(() => IsEditing);
            }
        }
    }
}
