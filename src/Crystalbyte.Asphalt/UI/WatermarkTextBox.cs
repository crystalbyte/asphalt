#region Using directives

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

#endregion

namespace Crystalbyte.Asphalt.UI {
    [StyleTypedProperty(Property = "WatermarkTextStyle", StyleTargetType = typeof (TextBlock)),
     TemplatePart(Name = "WatermarkText", Type = typeof (TextBlock)),
     TemplateVisualState(Name = "WatermarkTextVisible", GroupName = "WatermarkTextStates"),
     TemplateVisualState(Name = "WatermarkTextHidden", GroupName = "WatermarkTextStates")]
    public class WatermarkTextBox : TextBox {
        public static readonly DependencyProperty WatermarkTextProperty = DependencyProperty.Register(
            "WatermarkText",
            typeof (string),
            typeof (WatermarkTextBox),
            new PropertyMetadata(string.Empty, OnWatermarkTextPropertyChanged));

        public static readonly DependencyProperty WatermarkTextForegroundProperty = DependencyProperty.Register(
            "WatermarkTextForeground",
            typeof (Brush),
            typeof (WatermarkTextBox),
            new PropertyMetadata(new SolidColorBrush(Colors.Gray), OnWatermarkTextForegroundPropertyChanged));

        public static readonly DependencyProperty WatermarkTextStyleProperty = DependencyProperty.Register(
            "WatermarkTextStyle",
            typeof (Style),
            typeof (WatermarkTextBox),
            new PropertyMetadata(null, OnWatermarkTextStylePropertyChanged));


        private bool _isFocused;

        public string WatermarkText {
            get { return (string) GetValue(WatermarkTextProperty); }
            set { SetValue(WatermarkTextProperty, value); }
        }

        public Brush WatermarkTextForeground {
            get { return (Brush) GetValue(WatermarkTextForegroundProperty); }
            set { SetValue(WatermarkTextForegroundProperty, value); }
        }

        public Style WatermarkTextStyle {
            get { return (Style) GetValue(WatermarkTextStyleProperty); }
            set { SetValue(WatermarkTextStyleProperty, value); }
        }


        private static void OnWatermarkTextPropertyChanged(DependencyObject theTarget,
                                                           DependencyPropertyChangedEventArgs
                                                               theDependencyPropertyChangedEventArgs) {
            // Do nothing
        }

        private static void OnWatermarkTextForegroundPropertyChanged(DependencyObject theTarget,
                                                                     DependencyPropertyChangedEventArgs
                                                                         theDependencyPropertyChangedEventArgs) {
            // Do nothing
        }

        private static void OnWatermarkTextStylePropertyChanged(DependencyObject theTarget,
                                                                DependencyPropertyChangedEventArgs
                                                                    theDependencyPropertyChangedEventArgs) {
            // Do nothing
        }


        public WatermarkTextBox() {
            DefaultStyleKey = typeof (WatermarkTextBox);

            GotFocus += OnWatermarkTextBoxGotFocus;
            LostFocus += OnWatermarkTextBoxLostFocus;
            Loaded += OnWatermarkTextBoxLoaded;
            TextChanged += OnWatermarkTextBoxTextChanged;
        }

        private void OnWatermarkTextBoxLoaded(object sender, RoutedEventArgs e) {
            GoToVisualState(true);
        }

        private void OnWatermarkTextBoxGotFocus(object sender, RoutedEventArgs e) {
            _isFocused = true;
            GoToVisualState(false);
        }

        private void OnWatermarkTextBoxLostFocus(object sender, RoutedEventArgs e) {
            _isFocused = false;
            GoToVisualState(true);
        }

        private void OnWatermarkTextBoxTextChanged(object sender, TextChangedEventArgs e) {
            if (!_isFocused) {
                GoToVisualState(false);
            }
        }

        private void GoToVisualState(bool theIsWatermarkDisplayed) {
            if (theIsWatermarkDisplayed && (Text == null || (Text != null && Text.Length == 0))) {
                VisualStateManager.GoToState(this, "WatermarkTextVisible", true);
            }
            else {
                VisualStateManager.GoToState(this, "WatermarkTextHidden", true);
            }
        }
    }
}