using Crystalbyte.Asphalt.Contexts;
using System.Composition;
using System.Data.Linq;
using System.Windows;

namespace Crystalbyte.Asphalt.Data {

    [Export, Shared]
    public sealed class LocalStorage {

        private CarDataContext _carDataContext;
        private TourDataContext _tourDataContext;

        public CarDataContext CarDataContext {
            get { return _carDataContext; }
        }

        //public Table<TourContext> Tours {
        //    get { return _tourDataContext.Tours; }
        //}

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            var connectionString = (string) Application.Current.Resources["DefaultConnectionString"];

            _carDataContext = new CarDataContext(connectionString);
            if (!_carDataContext.DatabaseExists()) {
                _carDataContext.CreateDatabase();
            }

            _tourDataContext = new TourDataContext(connectionString);
            if (!_tourDataContext.DatabaseExists()) {
                _tourDataContext.CreateDatabase();
            }

        }
    }
}