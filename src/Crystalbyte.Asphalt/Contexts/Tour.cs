using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Linq.Mapping;
using System.Device.Location;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using Crystalbyte.Asphalt.Data;
using Microsoft.Phone.Maps.Services;
using System.Threading;

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
        private double _originLatitude;
        private double _originLongitude;
        private double _destinationLatitude;
        private double _destinationLongitude;
        private bool _isQuerying;
        private double _distance;

        public Tour() {
            Construct();
        }

        public bool IsQuerying {
            get { return _isQuerying; }
            set {
                if (_isQuerying == value) {
                    return;
                }

                RaisePropertyChanging(() => IsQuerying);
                _isQuerying = value;
                RaisePropertyChanged(() => IsQuerying);
            }
        }

        public event EventHandler CivicAddressesResolved;

        public void OnCivicAddressesResolved(EventArgs e) {
            var handler = CivicAddressesResolved;
            if (handler != null)
                handler(this, e);
        }

        private void Construct() {
            LocalStorage = App.Composition.GetExport<LocalStorage>();
            Channels = App.Composition.GetExport<Channels>();
            Positions = new ObservableCollection<Position>();
            Positions.CollectionChanged += OnPositionCollectionChanged;
        }

        private void OnPositionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (Positions.Count == 0) {
                return;
            }

            var first = Positions.First();
            var last = Positions.Last();

            OriginLatitude = first.Latitude;
            OriginLongitude = first.Longitude;

            DestinationLatitude = last.Latitude;
            DestinationLongitude = last.Longitude;
        }

        // [Import]
        public LocalStorage LocalStorage { get; set; }

        // [Import]
        public Channels Channels { get; set; }

        public void ResolveCivicAddresses() {
            // All queries must originate from the dispatcher thread, therefor we need to invoke.
            SmartDispatcher.InvokeAsync(ResolveCivicAddressesInternal);
        }

        private async void ResolveCivicAddressesInternal() {

            IsQuerying = true;

            var first = Positions.First();
            var startQuery = QueryPool.RequestReverseGeocodeQuery(new GeoCoordinate { Latitude = first.Latitude, Longitude = first.Longitude });
            var start = await startQuery.ExecuteAsync();
            QueryPool.Drop(startQuery);
            SetOrigin(start);

            var last = Positions.Last();
            var stopQuery = QueryPool.RequestReverseGeocodeQuery(new GeoCoordinate { Latitude = last.Latitude, Longitude = last.Longitude });
            var stop = await stopQuery.ExecuteAsync();
            QueryPool.Drop(stopQuery);
            SetDestination(stop);

            OnCivicAddressesResolved(EventArgs.Empty);

            IsQuerying = false;
        }

        private void SetDestination(IList<MapLocation> locations) {
            if (locations.Count < 1) {
                Debug.WriteLine("Unable to determine origin, service response was empty.");
                return;
            }
            var address = locations[0].Information.Address;
            Destination = string.Format("{0} {1}, {2} {3}", address.Street, address.HouseNumber, address.PostalCode,
                                        address.State);
        }

        private void SetOrigin(IList<MapLocation> locations) {
            if (locations.Count < 1) {
                Debug.WriteLine("Unable to determine origin, service response was empty.");
                return;
            }
            var address = locations[0].Information.Address;
            Origin = string.Format("{0} {1}, {2} {3}", address.Street, address.HouseNumber, address.PostalCode,
                                        address.State);
        }

        [OnDeserialized]
        public void OnDeserialized() {
            Construct();
        }

        public DateTime Month {
            get { return new DateTime(StartTime.Year, StartTime.Month, 1); }
        }

        public double DestinationLongitude {
            get { return _destinationLongitude; }
            set {
                if (Math.Abs(_destinationLongitude - value) < double.Epsilon) {
                    return;
                }

                RaisePropertyChanging(() => DestinationLongitude);
                _destinationLongitude = value;
                RaisePropertyChanged(() => DestinationLongitude);
            }
        }

        [DataMember]
        public double DestinationLatitude {
            get { return _destinationLatitude; }
            set {
                if (Math.Abs(_destinationLatitude - value) < double.Epsilon) {
                    return;
                }

                RaisePropertyChanging(() => DestinationLatitude);
                _destinationLatitude = value;
                RaisePropertyChanged(() => DestinationLatitude);
            }
        }

        [DataMember]
        public double OriginLatitude {
            get { return _originLatitude; }
            set {
                if (Math.Abs(_originLatitude - value) < double.Epsilon) {
                    return;
                }

                RaisePropertyChanging(() => OriginLatitude);
                _originLatitude = value;
                RaisePropertyChanged(() => OriginLatitude);
            }
        }

        [DataMember]
        public double OriginLongitude {
            get { return _originLongitude; }
            set {
                if (Math.Abs(_originLongitude - value) < double.Epsilon) {
                    return;
                }
                RaisePropertyChanging(() => OriginLongitude);
                _originLongitude = value;
                RaisePropertyChanged(() => OriginLongitude);
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

        public async void LoadData() {
            var id = Id;

            var positions = await Channels.Database.Enqueue(
                () => LocalStorage.DataContext.Positions
                    .Where(x => x.TourId == id)
                    .Select(x => x)
                    .OrderBy(x => x.TimeStamp)
                    .ToArray());

            Positions.AddRange(positions);
        }

        public ObservableCollection<Position> Positions { get; private set; }

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

        /// <summary>
        /// Gets or sets the accumulated distance for all recorded waypoints.
        /// </summary>
        [DataMember, Column(CanBeNull = false)]
        public double Distance {
            get { return _distance; }
            set {
                if (Math.Abs(_distance - value) < double.Epsilon) {
                    return;
                }

                RaisePropertyChanging(() => Distance);
                _distance = value;
                RaisePropertyChanged(() => Distance);
            }
        }

        public void CalculateDistance() {
            if (Positions.Count < 2) {
                Distance = 0.0d;
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
            Distance = distance;
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
