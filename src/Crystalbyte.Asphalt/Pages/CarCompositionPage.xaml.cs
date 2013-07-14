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
        public CarCompositionPage() {
            InitializeComponent();
            BindingValidationError += OnBindingValidationError;
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

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.Back) {
                return;
            }

            Car = (Car)NavigationState.Pop();
            Car.Commit();
            Car.ValidateAll();

            LocalStorage = (LocalStorage)NavigationState.Pop();
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
            var task = new PhotoChooserTask { ShowCamera = true };
            task.Completed += OnPhotoChooserTaskCompleted;
            task.Show();
        }

        private void OnPhotoChooserTaskCompleted(object sender, PhotoResult e) {
            var task = (PhotoChooserTask)sender;
            task.Completed -= OnPhotoChooserTaskCompleted;
            Car.Image = e.OriginalFileName;
        }
    }
}