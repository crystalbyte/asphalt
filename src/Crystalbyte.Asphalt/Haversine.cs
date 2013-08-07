#region Using directives

using System;
using Crystalbyte.Asphalt.Contexts;
using Windows.Devices.Geolocation;

#endregion

namespace Crystalbyte.Asphalt {
    public static class Haversine {
        /// <summary>
        ///   Radius of the Earth in Kilometers.
        /// </summary>
        private const double EarthRadiusKm = 6371;

        /// <summary>
        ///   Converts an angle to a radian.
        /// </summary>
        /// <param name="input"> The angle that is to be converted. </param>
        /// <returns> The angle in radians. </returns>
        private static double ToRad(double input) {
            return input*(Math.PI/180);
        }

        /// <summary>
        ///   Calculates the distance between two geo-points in kilometers using the Haversine algorithm.
        /// </summary>
        /// <param name="point1"> The first point. </param>
        /// <param name="point2"> The second point. </param>
        /// <returns> A double indicating the distance between the points in KM. </returns>
        public static double Delta(Geocoordinate point1, Geocoordinate point2) {
            var dLat = ToRad(point2.Latitude - point1.Latitude);
            var dLon = ToRad(point2.Longitude - point1.Longitude);

            var a = Math.Pow(Math.Sin(dLat/2), 2) +
                    Math.Cos(ToRad(point1.Latitude))*Math.Cos(ToRad(point2.Latitude))*
                    Math.Pow(Math.Sin(dLon/2), 2);

            var c = 2*Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            var distance = EarthRadiusKm*c;
            return distance;
        }

        /// <summary>
        ///   Calculates the distance between two geo-points in kilometers using the Haversine algorithm.
        /// </summary>
        /// <param name="point1"> The first point. </param>
        /// <param name="point2"> The second point. </param>
        /// <returns> A double indicating the distance between the points in KM. </returns>
        public static double Delta(Position point1, Position point2) {
            var dLat = ToRad(point2.Latitude - point1.Latitude);
            var dLon = ToRad(point2.Longitude - point1.Longitude);

            var a = Math.Pow(Math.Sin(dLat/2), 2) +
                    Math.Cos(ToRad(point1.Latitude))*Math.Cos(ToRad(point2.Latitude))*
                    Math.Pow(Math.Sin(dLon/2), 2);

            var c = 2*Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            var distance = EarthRadiusKm*c;
            return distance;
        }
    }
}