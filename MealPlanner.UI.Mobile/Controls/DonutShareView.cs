namespace MealPlanner.UI.Mobile.Controls
{
    public class DonutShareView : GraphicsView
    {
        public static readonly BindableProperty PercentageProperty = BindableProperty.Create(
            nameof(Percentage), typeof(double), typeof(DonutShareView), 0d, propertyChanged: OnVisualPropertyChanged);

        public static readonly BindableProperty RingColorProperty = BindableProperty.Create(
            nameof(RingColor), typeof(Color), typeof(DonutShareView), Colors.Purple, propertyChanged: OnVisualPropertyChanged);

        public static readonly BindableProperty TrackColorProperty = BindableProperty.Create(
            nameof(TrackColor), typeof(Color), typeof(DonutShareView), Colors.LightGray, propertyChanged: OnVisualPropertyChanged);

        private readonly DonutShareDrawable _drawable = new();

        public DonutShareView()
        {
            Drawable = _drawable;
        }

        public double Percentage
        {
            get => (double)GetValue(PercentageProperty);
            set => SetValue(PercentageProperty, value);
        }

        public Color RingColor
        {
            get => (Color)GetValue(RingColorProperty);
            set => SetValue(RingColorProperty, value);
        }

        public Color TrackColor
        {
            get => (Color)GetValue(TrackColorProperty);
            set => SetValue(TrackColorProperty, value);
        }

        private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is not DonutShareView view)
            {
                return;
            }

            view._drawable.Percentage = view.Percentage;
            view._drawable.RingColor = view.RingColor;
            view._drawable.TrackColor = view.TrackColor;
            view.Invalidate();
        }
    }
}
