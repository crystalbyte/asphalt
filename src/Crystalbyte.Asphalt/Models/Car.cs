using Crystalbyte.Asphalt.Contexts;

namespace Crystalbyte.Asphalt.Models {
    public sealed class Car : NotificationObject {
        public string Label { get; set; }
        public string LicencePlate { get; set; }
        public int InitialMileage { get; set; }
        public string Description { get; set; }
    }
}
