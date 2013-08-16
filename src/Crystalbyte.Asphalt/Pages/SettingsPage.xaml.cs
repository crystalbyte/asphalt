#region Using directives

using System;
using System.Linq;
using System.Windows.Controls;
using Crystalbyte.Asphalt.Contexts;

#endregion

namespace Crystalbyte.Asphalt.Pages {
    public partial class SettingsPage {
        private double _entryChecksum;
        private double _exitChecksum;

        public SettingsPage() {
            InitializeComponent();

            AppSettings = App.Composition.GetExport<AppSettings>();
            AppSettings.IsEditingChanged += (sender, e) => this.UpdateApplicationBar();
        }

        public AppSettings AppSettings {
            get { return DataContext as AppSettings; }
            set { DataContext = value; }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            CalculateEntryChecksum();

            this.UpdateApplicationBar();
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e) {
            base.OnNavigatedFrom(e);

            CalculateExitChecksum();
            if (Math.Abs(_entryChecksum - _exitChecksum) < double.Epsilon) {
                return;
            }

            AppSettings.OnSettingsChanged(EventArgs.Empty);
        }

        private void CalculateEntryChecksum() {
            _entryChecksum = AppSettings.ReportInterval + AppSettings.RecordingTimeout * 2 + AppSettings.SpeedThreshold * 3
                + AppSettings.RequiredAccuracy * 4 + AppSettings.SpeedExceedances * 5;
        }

        private void CalculateExitChecksum() {
            _exitChecksum = AppSettings.ReportInterval + AppSettings.RecordingTimeout * 2 + AppSettings.SpeedThreshold * 3
                + AppSettings.RequiredAccuracy * 4 + AppSettings.SpeedExceedances * 5;
        }

        private void OnUnitOfLengthSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count == 0) {
                return;
            }
            var unit = e.AddedItems.Cast<UnitOfLength>().FirstOrDefault();
            AppSettings.UnitOfLength = unit;    
        }
    }
}