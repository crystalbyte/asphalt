#region Using directives

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Data;
using Microsoft.Phone.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;
using Crystalbyte.Asphalt.Commands;
using System.Windows.Input;

#endregion

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

        public int PanoramaIndex { get; set; }

        public ICommand DeleteTourCommand {
            get { return App.Composition.GetExport<DeleteTourCommand>(); }
        }

        public Channels Channels {
            get { return App.Composition.GetExport<Channels>(); }
        }

        public LocalStorage LocalStorage {
            get { return App.Composition.GetExport<LocalStorage>(); }
        }

        public TourSelectionSource TourSelectionSource {
            get { return App.Composition.GetExport<TourSelectionSource>(); }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            if (_isNewPageInstance) {
                var navigation = App.Composition.GetExport<Navigator>();
                navigation.Initialize(NavigationService);
            }

            ClearSelection();

            this.UpdateApplicationBar();

            if (!App.Context.IsDataLoaded) {
                App.Context.LoadData();
            }

            _isNewPageInstance = false;
        }

        private void ClearSelection() {
            TourSelectionSource.Selection = null;
        }

        private void OnDeleteTourMenuItemClicked(object sender, RoutedEventArgs e) {
            TourSelectionSource.Selection = ((MenuItem)sender).DataContext as Tour;
            DeleteTourCommand.Execute(null);
        }

        private void OnTourTapped(object sender, GestureEventArgs e) {
            if (_skipNextTapEvent || AppContext.IsSelectionEnabled) {
                _skipNextTapEvent = false;
                return;
            }

            Tour tour;
            var success = TryFindDataContext(e.OriginalSource as DependencyObject, out tour);
            if (success) {
                HandleTourTap(tour);
            }
        }

        /// <summary>
        ///   Searches for the item's datacontext. The search will succeed if the tap was performed on a
        ///   <see cref="Microsoft.Phone.Controls.LongListMultiSelectorItem" />, else it will fail.
        /// </summary>
        /// <param name="originalSource"> The original source that registered the tap. </param>
        /// <param name="tour"> The tour context if found. </param>
        /// <returns> True on success, else false. </returns>
        private static bool TryFindDataContext<T>(DependencyObject originalSource, out T tour) where T : NotificationObject {
            ContentControl parent = null;
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

            tour = (T)parent.DataContext;
            return true;
        }

        private void HandleTourTap(Tour tour) {
            TourSelectionSource.Selection = tour;
            NavigationService.Navigate(new Uri(string.Format("/Pages/{0}.xaml", typeof(TourDetailsPage).Name),
                                               UriKind.Relative));
        }

        private void OnToursSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selector = (LongListMultiSelector)sender;

            var selections = TourSelectionSource.Selections;
            var prevCount = selections.Count;
            selections.Clear();

            if (selector.SelectedItems == null) {
                return;
            }

            selections.AddRange(selector.SelectedItems.Cast<Tour>());

            var postCount = selections.Count;
            if (prevCount == 0 || postCount == 0) {
                this.UpdateApplicationBar();
            }
        }

        public void OnPanoramaSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var panorama = (Panorama)sender;

            var first = e.AddedItems.Cast<PanoramaItem>().FirstOrDefault();
            if (first != null) {
                PanoramaIndex = panorama.Items.IndexOf(first);
            }

            this.UpdateApplicationBar();
        }

        private static void HandleVehicleTap(Vehicle vehicle) {
            vehicle.SelectionTime = DateTime.Now;
            App.Context.Vehicles.ForEach(x => x.InvalidateSelection());
        }

        private void OnVehicleSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var vehicle = e.AddedItems.Cast<Vehicle>().FirstOrDefault();
            if (vehicle == null) {
                return;
            }

            HandleVehicleTap(vehicle);
        }
    }
}