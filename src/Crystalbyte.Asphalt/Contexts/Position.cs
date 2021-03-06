﻿#region Using directives

using System;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Runtime.Serialization;

#endregion

namespace Crystalbyte.Asphalt.Contexts {
    [DataContract, Table]
    [DebuggerDisplay("Id = {Id}, TourId = {TourId}")]
    public sealed class Position : NotificationObject {
        private int _id;
        private int _tourId;
        private double _latitude;
        private double _longitude;
        private DateTime _timeStamp;

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

        [DataMember, Column]
        public int TourId {
            get { return _tourId; }
            set {
                if (_tourId == value) {
                    return;
                }

                RaisePropertyChanging(() => TourId);
                _tourId = value;
                RaisePropertyChanged(() => TourId);
            }
        }

        [DataMember, Column]
        public double Latitude {
            get { return _latitude; }
            set {
                if (Math.Abs(_latitude - value) < double.Epsilon) {
                    return;
                }
                RaisePropertyChanging(() => Latitude);
                _latitude = value;
                RaisePropertyChanged(() => Latitude);
            }
        }

        [DataMember, Column]
        public double Longitude {
            get { return _longitude; }
            set {
                if (Math.Abs(_longitude - value) < double.Epsilon) {
                    return;
                }

                RaisePropertyChanging(() => Longitude);
                _longitude = value;
                RaisePropertyChanged(() => Longitude);
            }
        }

        [DataMember, Column]
        public DateTime TimeStamp {
            get { return _timeStamp; }
            set {
                if (_timeStamp == value) {
                    return;
                }

                RaisePropertyChanging(() => TimeStamp);
                _timeStamp = value;
                RaisePropertyChanged(() => TimeStamp);
            }
        }
    }
}