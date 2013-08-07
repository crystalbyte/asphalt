#region Using directives

using System.Collections.Generic;
using System.Device.Location;
using Microsoft.Phone.Maps.Services;

#endregion

namespace Crystalbyte.Asphalt {
    public static class QueryPool {
        private static readonly HashSet<object> Pool = new HashSet<object>();

        public static ReverseGeocodeQuery RequestReverseGeocodeQuery(GeoCoordinate coordinate) {
            var query = new ReverseGeocodeQuery {GeoCoordinate = coordinate};
            Pool.Add(query);
            return query;
        }

        public static RouteQuery RequestRouteQuery(IEnumerable<GeoCoordinate> waypoints) {
            var query = new RouteQuery {Waypoints = waypoints};
            Pool.Add(query);
            return query;
        }

        public static void Drop(object query) {
            Pool.Remove(query);
        }
    }
}