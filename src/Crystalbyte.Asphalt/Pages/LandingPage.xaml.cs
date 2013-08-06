using System;
using System.Data.Linq;
using System.Diagnostics;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Data;
using Microsoft.Phone.Controls;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Media;

namespace Crystalbyte.Asphalt.Pages {
    public partial class LandingPage {
        private bool _isNewPageInstance;
        private bool _skipNextTapEvent;

        // Constructor
        public LandingPage() {
            InitializeComponent();

            AppContext = App.Context;
            AppContext.SelectionEnabledChanged += AppContextOnSelectionEnabledChanged;

            _isNewPageInstance = true;
        }

        private void AppContextOnSelectionEnabledChanged(object sender, EventArgs eventArgs) {
            _skipNextTapEvent = true;
        }

        public AppContext AppContext {
            get { return DataContext as AppContext; }
            set { DataContext = value; }
        }

        // [Import]
        public Channels Channels {
            get { return App.Composition.GetExport<Channels>(); }
        }

        // [Import]
        public LocalStorage LocalStorage {
            get { return App.Composition.GetExport<LocalStorage>(); }
        }

        // [Import]
        public TourSelectionSource TourSelectionSource {
            get { return App.Composition.GetExport<TourSelectionSource>(); }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            if (_isNewPageInstance) {
                var navigation = App.Composition.GetExport<Navigation>();
                navigation.Initialize(NavigationService);
            }

            this.UpdateApplicationBar();

            if (!App.Context.IsDataLoaded) {
                App.Context.LoadData();
            }

            _isNewPageInstance = false;
        }

        private async void OnDeleteTourMenuItemClicked(object sender, RoutedEventArgs e) {
            var item = (MenuItem)sender;
            var tour = (Tour)item.DataContext;

            Debug.WriteLine("Deleting tour with id {0} ...", tour.Id);

            await Channels.Database.Enqueue(() => {
                LocalStorage.DataContext.Tours.DeleteOnSubmit(tour);
                LocalStorage.DataContext.SubmitChanges(ConflictMode.FailOnFirstConflict);
            });

            Debug.WriteLine("Tour with id {0} has been successfully deleted.", tour.Id);

            // Reload items
            App.Context.LoadData();
        }

        private void OnTourTapped(object sender, System.Windows.Input.GestureEventArgs e) {
            if (_skipNextTapEvent) {
                _skipNextTapEvent = false;
                return;
            }

            Tour tour;
            var success = TryFindTour(e.OriginalSource as DependencyObject, out tour);
            if (success) {
                HandleTourTap(tour);
            }    
        }

        /// <summary>
        /// Searches for the item's datacontext. The search will succeed if the tap was performed on a
        /// <see cref="LongListMultiSelectorItem"/>, else it will fail.
        /// </summary>
        /// <param name="originalSource">The original source that registered the tap.</param>
        /// <param name="tour">The tour context if found.</param>
        /// <returns>True on success, else false.</returns>
        private static bool TryFindTour(DependencyObject originalSource, out Tour tour) {
            LongListMultiSelectorItem parent = null;
            var current = originalSource;
            while (true) {
                if (current == null) {
                    break;
                }
                current = VisualTreeHelper.GetParent(current);
                parent = current as LongListMultiSelectorItem;
                if (parent != null) {
                    break;
                }
            }

            if (parent == null) {
                tour = null;
                return false;
            }

            tour = (Tour)parent.DataContext;
            return true;
        }

        private void HandleTourTap(Tour tour) {
            TourSelectionSource.Selection = tour;
            NavigationService.Navigate(new Uri(string.Format("/Pages/{0}.xaml", typeof(TourDetailsPage).Name),
                                               UriKind.Relative));
        }
    }
}