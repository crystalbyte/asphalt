#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Data.Linq;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Crystalbyte.Asphalt.Commands;
using Crystalbyte.Asphalt.Data;
using Crystalbyte.Asphalt.Resources;
using Crystalbyte.Asphalt.Converters;
using System.Globalization;

#endregion

namespace Crystalbyte.Asphalt.Contexts {
    [Export, Shared]
    public sealed class ExportContext : NotificationObject, IProgressAware {
        private static readonly TourTypeLocalizer TourTypeLocalizer = new TourTypeLocalizer();
        private IExportStrategy _selectedStrategy;
        private IExportSerializer _selectedSerializer;
        private ExportState _exportState;
        private double _progress;

        public ExportContext() {
            StartExportCommand = new RelayCommand {
                CanExecuteCallback = OnStartExportCommandCanExecute,
                ExecuteCallback = OnStartExportCommandExecute
            };

            TourExports = new ObservableCollection<Tour>();
            TourExports.CollectionChanged += (sender, e) => {
                RaisePropertyChanged(() => TourExports);
                StartExportCommand.OnCanExecuteChanged(EventArgs.Empty);
            };

            ExportStateChanged += (sender, e) => StartExportCommand.OnCanExecuteChanged(EventArgs.Empty);
        }

        public event EventHandler ExportStateChanged;

        public void OnExportStateChanged(EventArgs e) {
            var handler = ExportStateChanged;
            if (handler != null)
                handler(this, e);
        }

        [Import]
        public LocalStorage LocalStorage { get; set; }

        [Import]
        public Channels Channels { get; set; }

        private static readonly Dictionary<string, string> EmptyDictionary = new Dictionary<string, string>();

        [ImportMany]
        public IEnumerable<IExportSerializer> ExportSerializers { get; set; }

        [ImportMany]
        public IEnumerable<IExportStrategy> ExportStrategies { get; set; }

        public static IDictionary<string, string> CollectDataExports(object data) {
            var properties = data.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            if (properties.Length == 0) {
                return EmptyDictionary;
            }

            var exports = new Dictionary<string, string>();
            var exportProperties = properties.Where(x => x.GetCustomAttribute(typeof(DataExportAttribute), true) != null);
            foreach (var property in exportProperties.OrderBy(x => ((DataExportAttribute)x.GetCustomAttribute(typeof(DataExportAttribute), true)).Position)) {
                var attribute = (DataExportAttribute)property.GetCustomAttribute(typeof(DataExportAttribute), true);
                var key = property.Name;
                var value = property.GetValue(data);
                if (value is double) {
                    value = Math.Round((double)value, 1);
                }
                if (key.ToLower() == "type" && value is TourType) {
                    value = TourTypeLocalizer.Convert(value, typeof(string), null, CultureInfo.CurrentCulture);
                }
                var output = string.Format(attribute.Format, value);
                exports.Add(key, output);
            }

            return exports;
        }

        public ExportState ExportState {
            get { return _exportState; }
            set {
                if (_exportState == value) {
                    return;
                }

                RaisePropertyChanging(() => ExportState);
                _exportState = value;
                RaisePropertyChanged(() => ExportState);
                OnExportStateChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        ///   Refreshes all data to be exported.
        /// </summary>
        public async Task RefreshAsync() {
            ExportState = ExportState.Collecting;

            Progress = 0;
            TourExports.Clear();
            var exports = await CollectToursForExportAsync();
            TourExports.AddRange(exports);

            ExportState = ExportState.Idle;
        }

        public ObservableCollection<Tour> TourExports { get; set; }

        private void OnStartExportCommandExecute(object obj) {
            HandleStartExportCommand();
        }

        private async void HandleStartExportCommand() {
            try {
                var exports = new List<Tour>(TourExports);

                ExportState = ExportState.Uploading;
                var serializer = SelectedSerializer;
                serializer.Reset();

                var builder = new StringBuilder();
                foreach (var export in exports) {
                    serializer.Serialize(export, builder);
                }
                var data = builder.ToString();
                var extension = serializer.FileExtension;

                await SelectedStrategy.ExportAsync(data, extension, this);
                await MarkAllToursAsExported(exports);

                ExportState = ExportState.Completed;
            }
            catch (Exception ex) {
                var caption = AppResources.ErrorExportingDataCaption;
                MessageBox.Show(ex.Message, caption, MessageBoxButton.OK);
                ExportState = ExportState.Idle;
            }
        }

        private async Task MarkAllToursAsExported(IEnumerable<Tour> exports) {
            foreach (var export in exports) {
                export.IsExported = true;
                await Channels.Database.Enqueue(() => {
                    var context = LocalStorage.DataContext;
                    try {
                        context.SubmitChanges(ConflictMode.ContinueOnConflict);
                    }
                    catch (ChangeConflictException) {
                        context.ChangeConflicts.ResolveAll(RefreshMode.KeepChanges);
                    }
                });
            }
        }

        private async Task<IEnumerable<Tour>> CollectToursForExportAsync() {
            var exports = new List<Tour>();
            var selections = TourSelectionSource.Selections;
            exports.AddRange(selections);

            if (exports.Count == 0) {
                var pending = await Channels.Database.Enqueue(
                    () => LocalStorage.DataContext.Tours
                        .Where(x =>
                            x.IsExported == false &&
                            x.VehicleId == ActiveVehicle.Id &&
                            x.DriverId == ActiveDriver.Id)
                        .ToList()
                        .Select(x => x)
                        .OrderByDescending(x => x.StartTime));
                exports.AddRange(pending);
            }

            return exports;
        }

        private bool OnStartExportCommandCanExecute(object arg) {
            return SelectedStrategy != null && SelectedSerializer != null && TourExports.Count > 0;
        }

        public Driver ActiveDriver {
            get { return AppContext.Drivers.First(x => x.IsSelected); }
        }

        public Vehicle ActiveVehicle {
            get { return AppContext.Vehicles.First(x => x.IsSelected); }
        }

        public RelayCommand StartExportCommand { get; set; }

        public TourSelectionSource TourSelectionSource {
            get { return App.Composition.GetExport<TourSelectionSource>(); }
        }

        public AppContext AppContext {
            get { return App.Composition.GetExport<AppContext>(); }
        }

        public IExportStrategy SelectedStrategy {
            get { return _selectedStrategy; }
            set {
                if (_selectedStrategy == value) {
                    return;
                }

                RaisePropertyChanging(() => SelectedStrategy);
                _selectedStrategy = value;
                RaisePropertyChanged(() => SelectedStrategy);
                UpdateCommandState();
            }
        }

        public IExportSerializer SelectedSerializer {
            get { return _selectedSerializer; }
            set {
                if (_selectedSerializer == value) {
                    return;
                }

                RaisePropertyChanging(() => SelectedSerializer);
                _selectedSerializer = value;
                RaisePropertyChanged(() => SelectedSerializer);
                UpdateCommandState();
            }
        }

        private void UpdateCommandState() {
            StartExportCommand.OnCanExecuteChanged(EventArgs.Empty);
        }

        public double Progress {
            get { return _progress; }
            set {
                if (Math.Abs(_progress - value) < double.Epsilon) {
                    return;
                }

                RaisePropertyChanging(() => Progress);
                _progress = value;
                RaisePropertyChanged(() => Progress);
            }
        }

        #region Implementation of IProgressAware

        public void ReportProgress(double percentage) {
            SmartDispatcher.InvokeAsync(() => Progress = percentage);
        }

        #endregion
    }
}