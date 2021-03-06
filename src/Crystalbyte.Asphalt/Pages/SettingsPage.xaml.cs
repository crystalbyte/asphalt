﻿#region Using directives

using System;
using System.ComponentModel;
using System.Windows.Navigation;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.UI;

#endregion

namespace Crystalbyte.Asphalt.Pages {
    public partial class SettingsPage {
        private double _entryChecksum;
        private double _exitChecksum;

        public SettingsPage() {
            InitializeComponent();

            if (DesignerProperties.IsInDesignTool) {
                return;
            }

            AppSettings = App.Composition.GetExport<AppSettings>();
            AppSettings.IsEditingChanged += (sender, e) => this.UpdateApplicationBar();
            BackgroundImageSource.ImageSource = ThemedResourceProvider.SatellitePageBackgroundSource;
        }

        public ThemedResourceProvider ThemedResourceProvider {
            get { return App.Composition.GetExport<ThemedResourceProvider>(); }
        }

        public AppSettings AppSettings {
            get { return DataContext as AppSettings; }
            set { DataContext = value; }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            CalculateEntryChecksum();
            this.UpdateApplicationBar();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            base.OnNavigatedFrom(e);

            CalculateExitChecksum();
            if (Math.Abs(_entryChecksum - _exitChecksum) < double.Epsilon) {
                return;
            }

            AppSettings.OnSettingsChanged(EventArgs.Empty);
        }

        private void CalculateEntryChecksum() {
            _entryChecksum = AppSettings.ReportInterval + AppSettings.RecordingTimeout*2 + AppSettings.SpeedThreshold*3
                             + AppSettings.RequiredAccuracy*4 + AppSettings.SpeedExceedances*5;
        }

        private void CalculateExitChecksum() {
            _exitChecksum = AppSettings.ReportInterval + AppSettings.RecordingTimeout*2 + AppSettings.SpeedThreshold*3
                            + AppSettings.RequiredAccuracy*4 + AppSettings.SpeedExceedances*5;
        }
    }
}