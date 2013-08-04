using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Device.Location;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.Phone.Maps.Services;
using Windows.Devices.Geolocation;

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
        private bool _isExported;

        public Tour() {
            Construct();
        }

        public event EventHandler OriginCivicAddressRequestCompleted;

        public void OnOriginCivicAddressRequestCompleted(EventArgs e) {
            var handler = OriginCivicAddressRequestCompleted;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler DestinationCivicAddressRequestCompleted;

        public void OnDestinationCivicAddressRequestCompleted(EventArgs e) {
            var handler = DestinationCivicAddressRequestCompleted;
            if (handler != null)
                handler(this, e);
        }

        private void Construct() {
            LoadData();
        }

        public void RequestOriginCivicAddressAsync(Geocoordinate coordinate) {
            SmartDispatcher.InvokeAsync(() => {
                var reverseGeocode = new ReverseGeocodeQuery { GeoCoordinate = new GeoCoordinate(coordinate.Latitude, coordinate.Longitude) };
                reverseGeocode.QueryCompleted += OnOriginReverseGeocodeQueryCompleted;
                reverseGeocode.QueryAsync();
            });
        }

        public void RequestDestinationCivicAddressAsync(Position position) {
            SmartDispatcher.InvokeAsync(() => {
                var reverseGeocode = new ReverseGeocodeQuery { GeoCoordinate = new GeoCoordinate(position.Latitude, position.Longitude) };
                reverseGeocode.QueryCompleted += OnDestinationReverseGeocodeQueryCompleted;
                reverseGeocode.QueryAsync();
            });
        }

        private void OnDestinationReverseGeocodeQueryCompleted(object sender, QueryCompletedEventArgs<IList<MapLocation>> e) {
            var query = (ReverseGeocodeQuery)sender;
            query.QueryCompleted -= OnDestinationReverseGeocodeQueryCompleted;

            if (e.Result.Count < 1) {
                Debug.WriteLine("ReverseGeocodeQuery returned no results.");
                return;
            }

            var address = e.Result[0].Information.Address;
            Destination = string.Format("{0} {1}, {2} {3}", address.Street, address.HouseNumber, address.PostalCode,
                                        address.State);

            OnDestinationCivicAddressRequestCompleted(EventArgs.Empty);
        }

        private void OnOriginReverseGeocodeQueryCompleted(object sender, QueryCompletedEventArgs<IList<MapLocation>> e) {
            var query = (ReverseGeocodeQuery)sender;
            query.QueryCompleted -= OnOriginReverseGeocodeQueryCompleted;

            if (e.Result.Count < 1) {
                Debug.WriteLine("ReverseGeocodeQuery returned no results.");
                return;
            }

            var address = e.Result[0].Information.Address;
            Origin = string.Format("{0} {1}, {2} {3}", address.Street, address.HouseNumber, address.PostalCode,
                                        address.State);

            OnOriginCivicAddressRequestCompleted(EventArgs.Empty);
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

        [DataMember, Column(CanBeNull = false)]
        public bool IsExported {
            get { return _isExported; }
            set {
                if (_isExported == value) {
                    return;
                }

                RaisePropertyChanging(() => IsExported);
                _isExported = value;
                RaisePropertyChanged(() => IsExported);
            }
        }

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

        public double Distance {
            get {
                if (Positions.Count < 2) {
                    return 0.0d;
                }
                var distance = 0.0d;
                for (var i = 0; i < Positions.Count; i++) {
                    if (Positions.Count == i + 1) {
                        break;
                    }
                    var current = Positions[i];
                    var next = Positions[i + 1];

                    distance += Haversine.Delta(current, next);
                }
                return distance;
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
