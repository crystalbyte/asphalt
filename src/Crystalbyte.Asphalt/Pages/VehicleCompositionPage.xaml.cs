#region Using directives

using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Data;
using Crystalbyte.Asphalt.UI;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

#endregion

namespace Crystalbyte.Asphalt.Pages {
    public partial class VehicleCompositionPage {
        private const string VehicleStateKey = "vehicle";
        private readonly PhotoChooserTask _photoChooser;
        private bool _isNewPageInstance;

        public VehicleCompositionPage() {
            InitializeComponent();
            BindingValidationError += OnBindingValidationError;

            if (DesignerProperties.IsInDesignTool) {
                return;
            }

            _photoChooser = new PhotoChooserTask { ShowCamera = true, PixelHeight = 200, PixelWidth = 200 };
            _photoChooser.Completed += OnPhotoChooserTaskCompleted;

            BackgroundImageSource.ImageSource = ThemedResourceProvider.SatellitePageBackgroundSource;

            _isNewPageInstance = true;
        }

        public ThemedResourceProvider ThemedResourceProvider {
            get { return App.Composition.GetExport<ThemedResourceProvider>(); }
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
            var button = ApplicationBar.Buttons.OfType<ApplicationBarIconButton>().FirstOrDefault();
            if (button != null) {
                button.IsEnabled = !Vehicle.HasErrors;
            }
        }

        public LocalStorage LocalStorage {
            get { return App.Composition.GetExport<LocalStorage>(); }
        }

        /// <summary>
        ///   Gets the VehicleSelectionSource.
        /// </summary>
        public VehicleSelectionSource VehicleSelectionSource {
            get { return App.Composition.GetExport<VehicleSelectionSource>(); }
        }

        /// <summary>
        ///   Gets or sets the current Vehicle.
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

            if (e.NavigationMode == NavigationMode.Back) {
                VehicleSelectionSource.Selection = null;
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New) {
                InitializeVehicle();
            }

            if (_isNewPageInstance && Vehicle == null) {
                var vehicle = (Vehicle)State[VehicleStateKey];
                VehicleSelectionSource.Selection = Vehicle;
                Vehicle = vehicle;
            }

            if (Vehicle.Image == null && Vehicle.ImageName != null) {
                await Vehicle.RestoreImageAsync();
            }

            this.UpdateApplicationBar();

            _isNewPageInstance = false;
        }

        private void InitializeVehicle() {
            Vehicle = VehicleSelectionSource.Selection;
            Vehicle.ValidateAll();
        }

        private void OnStackPanelTap(object sender, GestureEventArgs e) {
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

            SmartDispatcher.InvokeAsync(() => HandleChosenPhoto(e.OriginalFileName, e.ChosenPhoto));
        }

        private async void HandleChosenPhoto(string name, Stream data) {
            if (string.IsNullOrWhiteSpace(name)) {
                return;
            }

            // Delete obsolete image.
            if (!string.IsNullOrWhiteSpace(Vehicle.ImageName)) {
                await LocalStorage.DeleteImageAsync(Vehicle.ImageName);
                Vehicle.Image = null;
            }


            var image = new BitmapImage();
            image.SetSource(data);
            Vehicle.Image = image;

            // rewind stream
            data.Position = 0;

            Vehicle.ImageName = Guid.NewGuid().ToString();
            await LocalStorage.SaveImageAsync(Vehicle.ImageName, data);
        }

        private void OnTextBoxLostFocus(object sender, RoutedEventArgs e) {
            Vehicle.CommitChanges();
        }

        private void OnNotesTextChanged(object sender, TextChangedEventArgs e) {
            var textbox = (TextBox)sender;
            Vehicle.Notes = textbox.Text;
        }

        private void OnLicencePlateTextChanged(object sender, TextChangedEventArgs e) {
            var textbox = (TextBox)sender;
            Vehicle.LicensePlate = textbox.Text;
        }

        private void OnTextBoxGotFocus(object sender, RoutedEventArgs e) {
            var textbox = (TextBox)sender;
            textbox.Select(0, textbox.Text.Length);
        }
    }
}