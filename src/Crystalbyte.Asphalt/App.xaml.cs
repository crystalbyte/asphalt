﻿#region Using directives

using System;
using System.Composition.Hosting;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;
using Crystalbyte.Asphalt.Contexts;
using Crystalbyte.Asphalt.Data;
using Crystalbyte.Asphalt.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.Devices.Geolocation;

#endregion

namespace Crystalbyte.Asphalt {
    public partial class App {
        /// <summary>
        ///   Gets the main context object.
        /// </summary>
        /// <returns> The main context object. </returns>
        public static AppContext Context {
            get { return Composition.GetExport<AppContext>(); }
        }

        public static AppSettings AppSettings {
            get { return Composition.GetExport<AppSettings>(); }
        }

        public static event EventHandler GeolocatorTombstoned;

        public static void OnGeolocatorTombstoned(EventArgs e) {
            var handler = GeolocatorTombstoned;
            if (handler != null)
                handler(null, e);
        }

        /// <summary>
        ///   Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns> The root frame of the Phone Application. </returns>
        public static PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        ///   Gets the composition host container.
        /// </summary>
        public static CompositionHost Composition { get; set; }

        /// <summary>
        ///   Constructor for the Application object.
        /// </summary>
        public App() {
            // Global handler for uncaught exceptions.
            UnhandledException += OnApplicationUnhandledException;

            // Standard XAML initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Language display initialization
            InitializeLanguage();

            // Show graphics profiling information while debugging.
            if (!Debugger.IsAttached)
                return;

            // Display the current frame rate counters.
            //Current.Host.Settings.EnableFrameRateCounter = true;

            // Show the areas of the app that are being redrawn in each frame.
            //Current.Host.Settings.EnableRedrawRegions = true;

            // Enable non-production analysis visualization mode,
            // which shows areas of a page that are handed off to GPU with a colored overlay.
            //Current.Host.Settings.EnableCacheVisualization = true;

            // Prevent the screen from turning off while under the debugger by disabling
            // the application's idle detection.
            // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
            // and consume battery power when the user is not using the phone.
            //PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void OnApplicationLaunching(object sender, LaunchingEventArgs e) {
            ComposeApplication();
            InitializeApplication();

            // Ensure that application state is restored appropriately
            if (!Context.IsDataLoaded) {
                Context.LoadData();
            }
        }

        private static void InitializeApplication() {
            if (AppSettings.IsMovementDetectionEnabled) {
                InitializeGeolocator();
            }
        }

        public static void TombstoneGeolocator() {
            Debug.WriteLine("Geolocator tombstoning ...");

            if (Geolocator != null) {
                Geolocator.PositionChanged -= OnGeolocatorPositionChanged;
                Geolocator = null;
            }

            OnGeolocatorTombstoned(EventArgs.Empty);
            Debug.WriteLine("Geolocator tombstoned.");
        }

        private static void OnGeolocatorPositionChanged(Geolocator sender, PositionChangedEventArgs args) {

            if (Context.SetupState == SetupState.NotCompleted) {
                return;
            }

            if (args.Position.Coordinate.Accuracy < AppSettings.RequiredAccuracy) {
                SmartDispatcher.InvokeAsync(() => Context.LocationTracker.Update(args.Position));
                return;
            }

            Debug.WriteLine("Skipped update due to insufficient accuracy ({0}m).", args.Position.Coordinate.Accuracy);
        }

        private static void ComposeApplication() {
            var config = new ContainerConfiguration()
                .WithAssembly(typeof (App).GetTypeInfo().Assembly);

            Composition = config.CreateContainer();
        }

        public static bool IsGeolocatorAlive {
            get { return Geolocator != null; }
        }

        public static void InitializeGeolocator() {
            Debug.WriteLine("Geolocator initializing ...");
            Geolocator = new Geolocator
                             {
                                 DesiredAccuracy = PositionAccuracy.High,
                                 ReportInterval = AppSettings.ReportInterval
                             };
            Geolocator.PositionChanged += OnGeolocatorPositionChanged;
            Debug.WriteLine("Geolocator initialized.");
        }

        public static Geolocator Geolocator { get; private set; }

        /// <summary>
        ///   Returns <code>true</code> if the application is running in the background, <code>false</code> if not.
        /// </summary>
        public static bool IsRunningInBackground { get; private set; }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void OnApplicationActivated(object sender, ActivatedEventArgs e) {
            IsRunningInBackground = false;

            // App has been tombstoned, we need to reactivate its state
            if (Composition == null) {
                ComposeApplication();
            }
            // Ensure that application state is restored appropriately
            if (!Context.IsDataLoaded) {
                Context.LoadData();
            }
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void OnApplicationDeactivated(object sender, DeactivatedEventArgs e) {
            // Ensure that required application state is persisted here.
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void OnApplicationClosing(object sender, ClosingEventArgs e) {
            var storage = Composition.GetExport<LocalStorage>();
            storage.DataContext.Dispose();
        }

        // Code to execute if a navigation fails
        private static void OnRootFrameNavigationFailed(object sender, NavigationFailedEventArgs e) {
            if (Debugger.IsAttached) {
                // A navigation has failed; break into the debugger
                Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private static void OnApplicationUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e) {
            if (Debugger.IsAttached) {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool _phoneApplicationInitialized;

        // Do not add any additional code to this method
        private void InitializePhoneApplication() {
            if (_phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new TransitionFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += OnRootFrameNavigationFailed;

            // Handle reset requests for clearing the backstack
            RootFrame.Navigated += CheckForResetNavigation;

            // Ensure we don't initialize again
            _phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e) {
            // Set the root visual to allow the application to render
            if (RootFrame == null || RootVisual == RootFrame)
                return;

            RootVisual = RootFrame;
            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        private static void CheckForResetNavigation(object sender, NavigationEventArgs e) {
            // If the app has received a 'reset' navigation, then we need to check
            // on the next navigation to see if the page stack should be reset
            if (e.NavigationMode == NavigationMode.Reset)
                RootFrame.Navigated += ClearBackStackAfterReset;
        }

        private static void ClearBackStackAfterReset(object sender, NavigationEventArgs e) {
            // Unregister the event so it doesn't get called again
            RootFrame.Navigated -= ClearBackStackAfterReset;

            // Only clear the stack for 'new' (forward) and 'refresh' navigations
            if (e.NavigationMode != NavigationMode.New && e.NavigationMode != NavigationMode.Refresh)
                return;

            // For UI consistency, clear the entire page stack
            while (RootFrame.RemoveBackEntry() != null) {
                // do nothing
            }
        }

        #endregion

        // Initialize the app's font and flow direction as defined in its localized resource strings.
        //
        // To ensure that the font of your application is aligned with its supported languages and that the
        // FlowDirection for each of those languages follows its traditional direction, ResourceLanguage
        // and ResourceFlowDirection should be initialized in each resx file to match these values with that
        // file's culture. For example:
        //
        // AppResources.es-ES.resx
        //    ResourceLanguage's value should be "es-ES"
        //    ResourceFlowDirection's value should be "LeftToRight"
        //
        // AppResources.ar-SA.resx
        //     ResourceLanguage's value should be "ar-SA"
        //     ResourceFlowDirection's value should be "RightToLeft"
        //
        // For more info on localizing Windows Phone apps see http://go.microsoft.com/fwlink/?LinkId=262072.
        //
        private static void InitializeLanguage() {
            try {
                // Set the font to match the display language defined by the
                // ResourceLanguage resource string for each supported language.
                //
                // Fall back to the font of the neutral language if the Display
                // language of the phone is not supported.
                //
                // If a compiler error is hit then ResourceLanguage is missing from
                // the resource file.
                RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);

                // Set the FlowDirection of all elements under the root frame based
                // on the ResourceFlowDirection resource string for each
                // supported language.
                //
                // If a compiler error is hit then ResourceFlowDirection is missing from
                // the resource file.
                var flow = (FlowDirection) Enum.Parse(typeof (FlowDirection), AppResources.ResourceFlowDirection);
                RootFrame.FlowDirection = flow;
            }
            catch {
                // If an exception is caught here it is most likely due to either
                // ResourceLangauge not being correctly set to a supported language
                // code or ResourceFlowDirection is set to a value other than LeftToRight
                // or RightToLeft.

                if (Debugger.IsAttached) {
                    Debugger.Break();
                }

                throw;
            }
        }

        private void OnApplicationRunningInBackground(object sender, RunningInBackgroundEventArgs e) {
            IsRunningInBackground = true;
        }
    }
}