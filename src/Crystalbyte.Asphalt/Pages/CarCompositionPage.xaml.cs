using System;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Data;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace Crystalbyte.Asphalt.Pages {
    public partial class CarCompositionPage {

        private const string CarStateKey = "car";
        private bool _isNewPageInstance;
        private readonly PhotoChooserTask _photoChooser;
        private string _chosenPhotoName;

        public CarCompositionPage() {
            InitializeComponent();
            LocalStorage = App.Composition.GetExport<LocalStorage>();
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

        public Vehicle Car {
            get { return (Vehicle)DataContext; }
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
                LocalStorage = App.Composition.GetExport<LocalStorage>();
                Car = (Vehicle)State[CarStateKey];
                Car.OnRevive();
            }

            Car.ImagePath = _chosenPhotoName;

            _isNewPageInstance = false;
        }

        private void InitializeCar() {
            Car = (Vehicle)NavigationState.Pop();
            Car.Commit();
            Car.ValidateAll();
        }

        private void OnCheckButtonClicked(object sender, EventArgs e) {
            LocalStorage.CarDataContext.Cars.InsertOnSubmit(Car);
            LocalStorage.CarDataContext.SubmitChanges(ConflictMode.FailOnFirstConflict);

            App.Context.InvalidateData();
            NavigationService.GoBack();
        }

        private void OnCancelButtonClicked(object sender, EventArgs e) {
            Car.Revert();
            NavigationService.GoBack();
        }

        private void OnStackPanelTap(object sender, System.Windows.Input.GestureEventArgs e) {
            SelectImage();
        }

        private void SelectImage() {
            _photoChooser.Show();
        }

        private void OnPhotoChooserTaskCompleted(object sender, PhotoResult e) {
            // User canceled photo selection
            if (e.ChosenPhoto == null) {
                return;
            }

            HandleChosenPhoto(e.OriginalFileName, e.ChosenPhoto);
        }

        private void HandleChosenPhoto(string name, Stream data) {
            if (string.IsNullOrWhiteSpace(name)) {
                return;
            }

            _chosenPhotoName = Guid.NewGuid().ToString();
            LocalStorage.StoreImageAsync(_chosenPhotoName, data);
        }

        private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e) {
            var textbox = (TextBox)sender;
            Car.Notes = textbox.Text;
        }
    }
}