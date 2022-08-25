using SymbolBlaster.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SymbolBlaster.UI.Controls
{
    /// <summary>
    /// Interaction logic for ColorSelector.xaml
    /// </summary>
    /// 

    public partial class ColorSelector : UserControl
    {
        public ColorSelector() : base()
        {
            InitializeComponent();

            AddCustomColor = new DelegateCommand(OnAddCustomColor, null);
            SelectColor = new DelegateCommand(OnSelectColor, null);
        }

        public static readonly RoutedEvent ColorSelectedEvent = EventManager.RegisterRoutedEvent(
        name: nameof(ColorSelected),
        routingStrategy: RoutingStrategy.Direct,
        handlerType: typeof(RoutedEventHandler),
        ownerType: typeof(ColorSelector));

        public event RoutedEventHandler ColorSelected
        {
            add { AddHandler(ColorSelectedEvent, value); }
            remove { RemoveHandler(ColorSelectedEvent, value); }
        }

        void RaiseColorSelectedEvent()
        {
            RoutedEventArgs routedEventArgs = new(routedEvent: ColorSelectedEvent);
            RaiseEvent(routedEventArgs);
        }

        public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent(
        name: nameof(Closed),
        routingStrategy: RoutingStrategy.Direct,
        handlerType: typeof(RoutedEventHandler),
        ownerType: typeof(ColorSelector));

        public event RoutedEventHandler Closed
        {
            add { AddHandler(ClosedEvent, value); }
            remove { RemoveHandler(ClosedEvent, value); }
        }

        void RaiseClosedEvent()
        {
            RoutedEventArgs routedEventArgs = new(routedEvent: ClosedEvent);
            RaiseEvent(routedEventArgs);
        }

        private void OnSelectColor(object? obj)
        {
            if (obj == null)
                return;

            Color color = (Color)obj;

            SelectedColor = color;

            RaiseColorSelectedEvent();

            CurrentColor = color;
        }

        private void OnAddCustomColor(object? o)
        {
            if (CustomColors.Count >= 10)
                CustomColors.RemoveAt(CustomColors.Count - 1);

            CustomColors.Insert(0, CurrentColor);
        }

        static readonly DependencyProperty AddCustomColorCommandProperty =
            DependencyProperty.Register(nameof(AddCustomColor), typeof(ICommand), typeof(ColorSelector), new PropertyMetadata());

        ICommand AddCustomColor
        {
            get { return (ICommand)GetValue(AddCustomColorCommandProperty); }
            set { SetValue(AddCustomColorCommandProperty, value); }
        }

        static readonly DependencyProperty SelectColorCommandProperty =
            DependencyProperty.Register(nameof(SelectColor), typeof(ICommand), typeof(ColorSelector), new PropertyMetadata());

        ICommand SelectColor
        {
            get { return (ICommand)GetValue(SelectColorCommandProperty); }
            set { SetValue(SelectColorCommandProperty, value); }
        }

        public ObservableCollection<Color> CustomColors { get; set; } = new ObservableCollection<Color>() { };

        public static readonly DependencyProperty PresetColorsProperty =
            DependencyProperty.Register(nameof(PresetColors), typeof(ObservableCollection<Color>), typeof(ColorSelector), new PropertyMetadata(new ObservableCollection<Color>()));

        public ObservableCollection<Color> PresetColors
        {
            get { return (ObservableCollection<Color>)GetValue(PresetColorsProperty); }
            set { SetValue(PresetColorsProperty, value); }
        }

        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(nameof(SelectedColor), typeof(Color?), typeof(ColorSelector), new PropertyMetadata(null));

        public Color? SelectedColor
        {
            get { return (Color?)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        static Color DefaultColor = Colors.Black;

        public static readonly DependencyProperty CurrentColorProperty =
            DependencyProperty.Register(nameof(CurrentColor), typeof(Color), typeof(ColorSelector), new PropertyMetadata(DefaultColor, new PropertyChangedCallback(CurrentColorChanged)));

        public Color CurrentColor
        {
            get { return (Color)GetValue(CurrentColorProperty); }
            set { SetValue(CurrentColorProperty, value); }
        }

        private static void CurrentColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorSelector selector = (ColorSelector)d;
            if (selector.CurrentColor.ToString() != selector.HexValueTextBox)
            {
                selector.HexValueTextBox = selector.CurrentColor.ToString();
            }
        }

        static readonly DependencyProperty HexValueTextBoxProperty =
            DependencyProperty.Register(nameof(HexValueTextBox), typeof(string), typeof(ColorSelector), new PropertyMetadata(DefaultColor.ToString(), new PropertyChangedCallback(HexValueTextBoxChanged)));

        private static void HexValueTextBoxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorSelector colorSelector = (ColorSelector)d;
            Color? c = (Color)ColorConverter.ConvertFromString(colorSelector.HexValueTextBox);
            if (c != null)
            {
                if (colorSelector.A != c.Value.A)
                    colorSelector.A = c.Value.A;
                if (colorSelector.R != c.Value.R)
                    colorSelector.R = c.Value.R;
                if (colorSelector.G != c.Value.G)
                    colorSelector.G = c.Value.G;
                if (colorSelector.B != c.Value.B)
                    colorSelector.B = c.Value.B;
            }
        }

        string HexValueTextBox
        {
            get { return (string)GetValue(HexValueTextBoxProperty); }
            set { SetValue(HexValueTextBoxProperty, value); }
        }

        public static readonly DependencyProperty AProperty =
            DependencyProperty.Register(nameof(A), typeof(double), typeof(ColorSelector), new PropertyMetadata(Convert.ToDouble(DefaultColor.A)));

        public double A
        {
            get { return (double)GetValue(AProperty); }
            set
            {
                SetValue(AProperty, value);
                Byte byteVal = System.Convert.ToByte(A);
                if (AValueSlider != A)
                {
                    AValueSlider = A;
                }
                if (AValueTextBox != A)
                {
                    AValueTextBox = A;
                }
                Byte[] colorBytes = { byteVal, CurrentColor.R, CurrentColor.G, CurrentColor.B };
                Color? hexColor = (Color)ColorConverter.ConvertFromString(HexValueTextBox);
                if (hexColor != null)
                {
                    if (hexColor?.A != byteVal)
                    {
                        HexValueTextBox = $"#{colorBytes[0]:X2}{colorBytes[1]:X2}{colorBytes[2]:X2}{colorBytes[3]:X2}";
                    }
                }
                CurrentColor = Color.FromArgb(colorBytes[0], colorBytes[1], colorBytes[2], colorBytes[3]);
            }
        }

        readonly static DependencyProperty AValueSliderProperty =
            DependencyProperty.Register(nameof(AValueSlider), typeof(double), typeof(ColorSelector), new PropertyMetadata(Convert.ToDouble(DefaultColor.A), new PropertyChangedCallback(AValueSliderChanged)));

        private static void AValueSliderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorSelector selector = (ColorSelector)d;
            if (selector.A != selector.AValueSlider)
                selector.A = selector.AValueSlider;
        }

        double AValueSlider
        {
            get { return (double)GetValue(AValueSliderProperty); }
            set { SetValue(AValueSliderProperty, value); }
        }

        static readonly DependencyProperty AValueTextBoxProperty =
            DependencyProperty.Register(nameof(AValueTextBox), typeof(double), typeof(ColorSelector), new PropertyMetadata(Convert.ToDouble(DefaultColor.A), new PropertyChangedCallback(AValueTextBoxChanged)));

        private static void AValueTextBoxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorSelector selector = (ColorSelector)d;
            if (selector.A != selector.AValueTextBox)
                selector.A = selector.AValueTextBox;
        }

        double AValueTextBox
        {
            get { return (double)GetValue(AValueTextBoxProperty); }
            set { SetValue(AValueTextBoxProperty, value); }
        }

        public static readonly DependencyProperty RProperty =
            DependencyProperty.Register(nameof(R), typeof(double), typeof(ColorSelector), new PropertyMetadata(Convert.ToDouble(DefaultColor.R)));

        public double R
        {
            get { return (double)GetValue(RProperty); }
            set
            {
                SetValue(RProperty, value);
                Byte byteVal = System.Convert.ToByte(R);
                if (RValueSlider != R)
                {
                    RValueSlider = R;
                }
                if (RValueTextBox != R)
                {
                    RValueTextBox = R;
                }
                Byte[] colorBytes = { CurrentColor.A, byteVal, CurrentColor.G, CurrentColor.B };
                Color? hexColor = (Color)ColorConverter.ConvertFromString(HexValueTextBox);
                if (hexColor != null)
                {
                    if (hexColor?.R != byteVal)
                    {
                        HexValueTextBox = $"#{colorBytes[0]:X2}{colorBytes[1]:X2}{colorBytes[2]:X2}{colorBytes[3]:X2}";
                    }
                }
                CurrentColor = Color.FromArgb(colorBytes[0], colorBytes[1], colorBytes[2], colorBytes[3]);
            }
        }

        readonly static DependencyProperty RValueSliderProperty =
            DependencyProperty.Register(nameof(RValueSlider), typeof(double), typeof(ColorSelector), new PropertyMetadata(Convert.ToDouble(DefaultColor.R), new PropertyChangedCallback(RValueSliderChanged)));

        private static void RValueSliderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorSelector selector = (ColorSelector)d;
            if (selector.R != selector.RValueSlider)
                selector.R = selector.RValueSlider;
        }

        double RValueSlider
        {
            get { return (double)GetValue(RValueSliderProperty); }
            set { SetValue(RValueSliderProperty, value); }
        }

        static readonly DependencyProperty RValueTextBoxProperty =
            DependencyProperty.Register(nameof(RValueTextBox), typeof(double), typeof(ColorSelector), new PropertyMetadata(Convert.ToDouble(DefaultColor.R), new PropertyChangedCallback(RValueTextBoxChanged)));

        private static void RValueTextBoxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorSelector selector = (ColorSelector)d;
            if (selector.R != selector.RValueTextBox)
                selector.R = selector.RValueTextBox;
        }

        double RValueTextBox
        {
            get { return (double)GetValue(RValueTextBoxProperty); }
            set { SetValue(RValueTextBoxProperty, value); }
        }

        public static readonly DependencyProperty GProperty =
            DependencyProperty.Register(nameof(G), typeof(double), typeof(ColorSelector), new PropertyMetadata(Convert.ToDouble(DefaultColor.G)));

        public double G
        {
            get { return (double)GetValue(GProperty); }
            set
            {
                SetValue(GProperty, value);
                Byte byteVal = System.Convert.ToByte(G);
                if (GValueSlider != G)
                {
                    GValueSlider = G;
                }
                if (GValueTextBox != G)
                {
                    GValueTextBox = G;
                }
                Byte[] colorBytes = { CurrentColor.A, CurrentColor.R, byteVal, CurrentColor.B };
                Color? hexColor = (Color)ColorConverter.ConvertFromString(HexValueTextBox);
                if (hexColor != null)
                {
                    if (hexColor?.G != byteVal)
                    {
                        HexValueTextBox = $"#{colorBytes[0]:X2}{colorBytes[1]:X2}{colorBytes[2]:X2}{colorBytes[3]:X2}";
                    }
                }
                CurrentColor = Color.FromArgb(colorBytes[0], colorBytes[1], colorBytes[2], colorBytes[3]);
            }
        }

        readonly static DependencyProperty GValueSliderProperty =
            DependencyProperty.Register(nameof(GValueSlider), typeof(double), typeof(ColorSelector), new PropertyMetadata(Convert.ToDouble(DefaultColor.G), new PropertyChangedCallback(GValueSliderChanged)));

        private static void GValueSliderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorSelector selector = (ColorSelector)d;
            if (selector.G != selector.GValueSlider)
                selector.G = selector.GValueSlider;
        }

        double GValueSlider
        {
            get { return (double)GetValue(GValueSliderProperty); }
            set { SetValue(GValueSliderProperty, value); }
        }

        static readonly DependencyProperty GValueTextBoxProperty =
            DependencyProperty.Register(nameof(GValueTextBox), typeof(double), typeof(ColorSelector), new PropertyMetadata(Convert.ToDouble(DefaultColor.G), new PropertyChangedCallback(GValueTextBoxChanged)));

        private static void GValueTextBoxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorSelector selector = (ColorSelector)d;
            if (selector.G != selector.GValueTextBox)
                selector.G = selector.GValueTextBox;
        }

        double GValueTextBox
        {
            get { return (double)GetValue(GValueTextBoxProperty); }
            set { SetValue(GValueTextBoxProperty, value); }
        }

        public static readonly DependencyProperty BProperty =
            DependencyProperty.Register(nameof(B), typeof(double), typeof(ColorSelector), new PropertyMetadata(Convert.ToDouble(DefaultColor.B)));

        public double B
        {
            get { return (double)GetValue(BProperty); }
            set
            {
                SetValue(BProperty, value);
                Byte byteVal = System.Convert.ToByte(B);
                if (BValueSlider != B)
                {
                    BValueSlider = B;
                }
                if (BValueTextBox != B)
                {
                    BValueTextBox = B;
                }

                Byte[] colorBytes = { CurrentColor.A, CurrentColor.R, CurrentColor.G, byteVal };
                Color? hexColor = (Color)ColorConverter.ConvertFromString(HexValueTextBox);
                if (hexColor != null)
                {
                    if (hexColor?.B != byteVal)
                    {
                        HexValueTextBox = $"#{colorBytes[0]:X2}{colorBytes[1]:X2}{colorBytes[2]:X2}{colorBytes[3]:X2}";
                    }
                }
                CurrentColor = Color.FromArgb(colorBytes[0], colorBytes[1], colorBytes[2], colorBytes[3]);
            }
        }

        readonly static DependencyProperty BValueSliderProperty =
            DependencyProperty.Register(nameof(BValueSlider), typeof(double), typeof(ColorSelector), new PropertyMetadata(Convert.ToDouble(DefaultColor.B), new PropertyChangedCallback(BValueSliderChanged)));

        private static void BValueSliderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorSelector selector = (ColorSelector)d;
            if (selector.B != selector.BValueSlider)
                selector.B = selector.BValueSlider;
        }

        double BValueSlider
        {
            get { return (double)GetValue(BValueSliderProperty); }
            set { SetValue(BValueSliderProperty, value); }
        }

        static readonly DependencyProperty BValueTextBoxProperty =
            DependencyProperty.Register(nameof(BValueTextBox), typeof(double), typeof(ColorSelector), new PropertyMetadata(Convert.ToDouble(DefaultColor.B), new PropertyChangedCallback(BValueTextBoxChanged)));

        private static void BValueTextBoxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorSelector selector = (ColorSelector)d;
            if (selector.B != selector.BValueTextBox)
                selector.B = selector.BValueTextBox;
        }

        double BValueTextBox
        {
            get { return (double)GetValue(BValueTextBoxProperty); }
            set { SetValue(BValueTextBoxProperty, value); }
        }

        private void CloseColorSelector(object sender, RoutedEventArgs e)
        {
            RaiseClosedEvent();
        }

        private void CustomRgbTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression exp = customRgbTextBox.GetBindingExpression(TextBox.TextProperty);
                exp.UpdateSource();
            }
        }
    }
}
