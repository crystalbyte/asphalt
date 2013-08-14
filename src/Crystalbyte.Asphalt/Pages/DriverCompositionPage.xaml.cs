using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Data;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace Crystalbyte.Asphalt.Pages {
    public partial class DriverCompositionPage {

        private const string DriverStateKey = "driver";
        private bool _isNewPageInstance;
        private readonly PhotoChooserTask _photoChooser;
        private string _chosenPhotoName;

        public DriverCompositionPage() {
            InitializeComponent();
            BindingValidationError += OnBindingValidationError;

            _photoChooser = new PhotoChooserTask { ShowCamera = true, PixelHeight = 200, PixelWidth = 200 };
            _photoChooser.Completed += OnPhotoChooserTaskCompleted;

            _isNewPageInstance = true;
        }

        public DriverSelectionSource DriverSelectionSource {
            get {
                return App.Composition.GetExport<DriverSelectionSource>();
            }
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
            get {
                return App.Composition.GetExport<LocalStorage>();
            }
        }

        /// <summary>
        /// Gets or sets the current datacontext as a vehicle.
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
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New) {
                InitializeDriver();
            }

            if (_isNewPageInstance && Driver == null) {
                Driver = (Driver)State[DriverStateKey];
            }

            if (_chosenPhotoName != null) {
                Driver.ImagePath = _chosenPhotoName;
            }

            if (Driver.Image == null && Driver.ImagePath != null) {
                Driver.RestoreImageFromPath();
            }

            this.UpdateApplicationBar();

            Driver.ValidateAll();

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

            _chosenPhotoName = Guid.NewGuid().ToString();
            await LocalStorage.StoreImageAsync(_chosenPhotoName, data);
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