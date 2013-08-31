#region Using directives

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Data;
using Crystalbyte.Asphalt.UI;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;

#endregion

namespace Crystalbyte.Asphalt.Pages {
    public partial class DriverCompositionPage {
        private const string DriverStateKey = "driver";
        private bool _isNewPageInstance;
        private readonly PhotoChooserTask _photoChooser;

        public DriverCompositionPage() {
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

        public DriverSelectionSource DriverSelectionSource {
            get { return App.Composition.GetExport<DriverSelectionSource>(); }
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
            var button = ApplicationBar.Buttons.OfType<ApplicationBarIconButton>().First();
            button.IsEnabled = !Driver.HasErrors;
        }

        public LocalStorage LocalStorage {
            get { return App.Composition.GetExport<LocalStorage>(); }
        }

        /// <summary>
        ///   Gets or sets the current datacontext as a vehicle.
        /// </summary>
        public Driver Driver {
            get { return (Driver)DataContext; }
            set { DataContext = value; }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            base.OnNavigatedFrom(e);

            if (e.NavigationMode == NavigationMode.New) {
                State[DriverStateKey] = Driver;
            }

            if (e.NavigationMode == NavigationMode.Back) {
                DriverSelectionSource.Selection = null;
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New) {
                InitializeDriver();
            }

            if (_isNewPageInstance && Driver == null) {
                var driver = (Driver)State[DriverStateKey];
                DriverSelectionSource.Selection = driver;
                Driver = driver;
            }

            if (Driver.Image == null && Driver.ImageName != null) {
                await Driver.RestoreImageAsync();
            }

            this.UpdateApplicationBar();

            _isNewPageInstance = false;
        }

        private void InitializeDriver() {
            Driver = DriverSelectionSource.Selection;
            Driver.ValidateAll();
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

            SmartDispatcher.InvokeAsync(() =>
                HandleChosenPhoto(e.OriginalFileName, e.ChosenPhoto));
        }

        private async void HandleChosenPhoto(string name, Stream data) {
            if (string.IsNullOrWhiteSpace(name)) {
                return;
            }

            // Delete obsolete image.
            if (!string.IsNullOrWhiteSpace(Driver.ImageName)) {
                await LocalStorage.DeleteImageAsync(Driver.ImageName);
                Driver.Image = null;
            }

            var image = new BitmapImage();
            image.SetSource(data);
            Driver.Image = image;

            // rewind stream
            data.Position = 0;

            Driver.ImageName = Guid.NewGuid().ToString();
            await LocalStorage.SaveImageAsync(Driver.ImageName, data);
        }

        private void OnTextBoxLostFocus(object sender, RoutedEventArgs e) {
            Driver.CommitChanges();
        }

        private void OnTextBoxGotFocus(object sender, RoutedEventArgs e) {
            var textbox = (TextBox)sender;
            textbox.Select(0, textbox.Text.Length);
        }

        private void OnForenameChanged(object sender, TextChangedEventArgs e) {
            var textbox = (TextBox)sender;
            Driver.Forename = textbox.Text;
        }

        private void OnSurnameChanged(object sender, TextChangedEventArgs e) {
            var textbox = (TextBox)sender;
            Driver.Surname = textbox.Text;
        }
    }
}