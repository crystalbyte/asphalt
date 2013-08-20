namespace Crystalbyte.Asphalt {
    public enum TourType : byte {
        /// <summary>
        ///   Business denotes a work related trip apart from the commute.
        /// </summary>
        Business = 0,

        /// <summary>
        ///   Commute denotes the trip from home to work and vice versa.
        /// </summary>
        Commute,

        /// <summary>
        ///   Private denotes a non work releated trip.
        /// </summary>
        Private
    }
}