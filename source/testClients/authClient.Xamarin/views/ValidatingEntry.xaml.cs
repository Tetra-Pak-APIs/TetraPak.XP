using Xamarin.Forms;

namespace authClient.views
{
    // ReSharper disable HeapView.BoxingAllocation
    public partial class ValidatingEntry 
    {
        #region Bindable: Placeholder
        public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
            "Placeholder",
            typeof(string),
            typeof(ValidatingEntry),
            default(string));

        public string Placeholder
        {
            get => (string) GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }
        #endregion

        #region Bindable: PlaceholderColor

        public static readonly BindableProperty PlaceholderColorProperty = BindableProperty.Create(
            "PlaceholderColor",
            typeof(Color),
            typeof(ValidatingEntry),
           Color.Goldenrod);

        public Color PlaceholderColor
        {
            get => (Color) GetValue(PlaceholderColorProperty);
            set => SetValue(PlaceholderColorProperty, value);
        }

        #endregion

        #region Bindable: TextColor
        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
            "TextColor",
            typeof(Color),
            typeof(ValidatingEntry),
            Color.Black);

        public Color TextColor
        {
            get => (Color)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }
        #endregion

        #region Bindable: IsInvalid
        public static readonly BindableProperty IsInvalidProperty = BindableProperty.Create(
            "IsInvalid",
            typeof(bool),
            typeof(ValidatingEntry),
            false);

        public bool IsInvalid
        {
            get => (bool) GetValue(IsInvalidProperty);
            set => SetValue(IsInvalidProperty, value);
        }
        #endregion

        #region Bindable: Text

        public static readonly BindableProperty TextProperty = BindableProperty.Create(
            "Text",
            typeof(string),
            typeof(ValidatingEntry),
            default(string),
            BindingMode.TwoWay);

        public string Text
        {
            get => (string) GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        #endregion

        #region Bindable: IsRequired

        public static readonly BindableProperty IsRequiredProperty = BindableProperty.Create(
            "IsRequired",
            typeof(bool),
            typeof(ValidatingEntry),
            default(bool));

        public bool IsRequired
        {
            get => (bool) GetValue(IsRequiredProperty);
            set => SetValue(IsRequiredProperty, value);
        }

        #endregion

        #region Bindable: FontSize

        public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
            nameof(FontSize),
            typeof(double),
            typeof(ValidatingEntry),
            Entry.FontSizeProperty.DefaultValue);

        public double FontSize
        {
            get => (double) GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        #endregion

        public ValidatingEntry()
        {
            InitializeComponent();
            TextColor = (Color) TextColorProperty.DefaultValue;
            FontSize = (double) FontSizeProperty.DefaultValue;
        }
    }
    // ReSharper restore HeapView.BoxingAllocation
}
