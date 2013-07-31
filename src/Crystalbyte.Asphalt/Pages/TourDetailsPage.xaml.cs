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
    public partial class TourDetailsPage {

        private const string TourStateKey = "tour";
        private bool _isNewPageInstance;

        public TourDetailsPage() {
            InitializeComponent();
            TourSelector = App.Composition.GetExport<TourSelectionSource>();
        }

        protected TourSelectionSource TourSelector { get; set; }

        public Tour Tour {
            get { return (Tour)DataContext; }
            set { DataContext = value; }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            base.OnNavigatedFrom(e);

            if (e.NavigationMode == NavigationMode.New) {
                State[TourStateKey] = Tour;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New) {
                Tour = TourSelector.Selection;
            }

            if (_isNewPageInstance && Tour == null) {
                Tour = (Tour)State[TourStateKey];
            }

            if (!Tour.IsDataLoaded) {
                Tour.LoadData();
            }

            Tour.ValidateAll();

            this.UpdateApplicationBar();

            _isNewPageInstance = false;
        }
    }
}