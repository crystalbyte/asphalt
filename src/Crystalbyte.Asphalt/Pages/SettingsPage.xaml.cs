#region Using directives

using System;
using System.Linq;
using System.Windows.Controls;
using Crystalbyte.Asphalt.Contexts;

#endregion

namespace Crystalbyte.Asphalt.Pages {
    public partial class SettingsPage {
        public SettingsPage() {
            InitializeComponent();
            Initialize();
        }

        public AppSettings AppSettings {
            get { return DataContext as AppSettings; }
            set { DataContext = value; }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            this.UpdateApplicationBar();
        }

        private void Initialize() {
            AppSettings = App.Context.AppSettings;
            UnitOfLengthListPicker.ItemsSource = Enum.GetValues(typeof (UnitOfLength));
        }

        private void OnUnitOfLengthListBoxSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count == 0) {
                return;
            }
            var unit = e.AddedItems.OfType<UnitOfLength>().First();
            AppSettings.UnitOfLength = unit;
        }
    }
}