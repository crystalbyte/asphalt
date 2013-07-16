using System;
using System.Data.Linq;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Data;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace Crystalbyte.Asphalt.Pages {
    public partial class CarCompositionPage {

        private const string CarStateKey = "car";
        private bool _isNewPageInstance;
        private readonly PhotoChooserTask _photoChooser;
        private string _chosenPhotoPath;

        public CarCompositionPage() {
            InitializeComponent();
            BindingValidationError += OnBindingValidationError;

            _photoChooser = new PhotoChooserTask { ShowCamera = true };
            _photoChooser.Completed += OnPhotoChooserTaskCompleted;

            _isNewPageInstance = true;
        }

        private void OnBindingValidationError(object sender, ValidationErrorEventArgs e) {
            var button = ApplicationBar.Buttons.OfType<ApplicationBarIconButton>().First();
            button.IsEnabled = !Car.HasErrors;
        }

        public LocalStorage LocalStorage { get; private set; }

        public Car Car {
            get { return (Car)DataContext; }
            set { DataContext = value; }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            base.OnNavigatedFrom(e);

            if (e.NavigationMode == NavigationMode.New) {
                State[CarStateKey] = Car;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New) {
                InitializeCar();
            }

            if (_isNewPageInstance && Car == null) {
                Car = (Car)State[CarStateKey];
                Car.OnRevive();
                Car.Image = _chosenPhotoPath;
            }

            _isNewPageInstance = false;
        }

        private void InitializeCar() {
            Car = (Car)NavigationState.Pop();
            Car.Commit();
            Car.ValidateAll();
        }

        private void OnCheckButtonClicked(object sender, EventArgs e) {
            LocalStorage.CarDataContext.Cars.InsertOnSubmit(Car);
            LocalStorage.CarDataContext.SubmitChanges(ConflictMode.FailOnFirstConflict);
            NavigationService.GoBack();
        }

        private void OnCancelButtonClicked(object sender, EventArgs e) {
            Car.Revert();
            NavigationService.GoBack();
        }

        private void OnLabelTextBoxTextChanged(object sender, TextChangedEventArgs e) {
            Car.ValidateProperty(() => Car.Label);
        }

        private void OnLicencePlateTextChanged(object sender, TextChangedEventArgs e) {
            Car.ValidateProperty(() => Car.LicencePlate);
        }

        private void OnInitialMileageChanged(object sender, TextChangedEventArgs e) {
            Car.ValidateProperty(() => Car.InitialMileage);
        }

        private void OnStackPanelTap(object sender, System.Windows.Input.GestureEventArgs e) {
            SelectImage();
        }

        private void SelectImage() {
            _photoChooser.Show();
        }

        private void OnPhotoChooserTaskCompleted(object sender, PhotoResult e) {
            ImageStore.Current.StoreImage(e.OriginalFileName, e.ChosenPhoto);
            _chosenPhotoPath = e.OriginalFileName;
        }
    }
}