using System;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Data;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace Crystalbyte.Asphalt.Pages {
    public partial class VehicleCompositionPage {

        private const string VehicleStateKey = "vehicle";
        private bool _isNewPageInstance;
        private readonly PhotoChooserTask _photoChooser;
        private string _chosenPhotoName;

        public VehicleCompositionPage() {
            InitializeComponent();
            BindingValidationError += OnBindingValidationError;

            _photoChooser = new PhotoChooserTask { ShowCamera = true, PixelHeight = 200, PixelWidth = 200 };
            _photoChooser.Completed += OnPhotoChooserTaskCompleted;

            _isNewPageInstance = true;
        }

        protected override void OnKeyUp(KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                HandleEnterKey();
                e.Handled = true;
            }

            base.OnKeyUp(e);
        }

        private void HandleEnterKey() {
            Focus();
        }

        private void OnBindingValidationError(object sender, ValidationErrorEventArgs e) {
            var button = ApplicationBar.Buttons.OfType<ApplicationBarIconButton>().First();
            button.IsEnabled = !Vehicle.HasErrors;
        }

        public LocalStorage LocalStorage {
            get {
                return App.Composition.GetExport<LocalStorage>();
            }
        }

        /// <summary>
        /// Gets the VehicleSelectionSource.
        /// </summary>
        public VehicleSelectionSource VehicleSelectionSource {
            get { return App.Composition.GetExport<VehicleSelectionSource>(); }
        }

        /// <summary>
        /// Gets or sets the current datacontext as a vehicle.
        /// </summary>
        public Vehicle Vehicle {
            get { return (Vehicle)DataContext; }
            set { DataContext = value; }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            base.OnNavigatedFrom(e);

            if (e.NavigationMode == NavigationMode.New) {
                State[VehicleStateKey] = Vehicle;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New) {
                InitializeVehicle();
            }

            if (_isNewPageInstance && Vehicle == null) {
                Vehicle = (Vehicle)State[VehicleStateKey];
            }

            Vehicle.ImagePath = _chosenPhotoName;
            Vehicle.ValidateAll();

            _isNewPageInstance = false;
        }

        private void InitializeVehicle() {
            Vehicle = VehicleSelectionSource.Selection;
            Vehicle.ValidateAll();
        }

        private void OnCheckButtonClicked(object sender, EventArgs e) {
            LocalStorage.DataContext.Vehicles.InsertOnSubmit(Vehicle);
            LocalStorage.DataContext.SubmitChanges(ConflictMode.FailOnFirstConflict);

            App.Context.InvalidateData();
            NavigationService.GoBack();
        }

        private void OnCancelButtonClicked(object sender, EventArgs e) {
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

            SmartDispatcher.InvokeAsync(() => 
                HandleChosenPhoto(e.OriginalFileName, e.ChosenPhoto));
        }

        private async void HandleChosenPhoto(string name, Stream data) {
            if (string.IsNullOrWhiteSpace(name)) {
                return;
            }
            
            _chosenPhotoName = Guid.NewGuid().ToString();
            await LocalStorage.StoreImageAsync(_chosenPhotoName, data);
        }

        private void OnNotesTextChanged(object sender, TextChangedEventArgs e) {
            var textbox = (TextBox)sender;
            Vehicle.Notes = textbox.Text;
        }
    }
}
