using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Crystalbyte.Asphalt.Contexts;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

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

        private void Initialize() {
            AppSettings = App.Context.AppSettings;
            UnitOfLengthListPicker.ItemsSource = Enum.GetValues(typeof(UnitOfLength));
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