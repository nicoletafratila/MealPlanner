namespace MealPlanner.UI.Mobile.Controls
{
    public class DonutShareDrawable : IDrawable
    {
        public double Percentage { get; set; }
        public Color RingColor { get; set; } = Colors.Purple;
        public Color TrackColor { get; set; } = Colors.LightGray;
        public float StrokeWidth { get; set; } = 8f;

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var size = Math.Min(dirtyRect.Width, dirtyRect.Height);
            var radius = (size - StrokeWidth) / 2f;
            var centerX = dirtyRect.Center.X;
            var centerY = dirtyRect.Center.Y;

            canvas.StrokeSize = StrokeWidth;
            canvas.StrokeLineCap = LineCap.Round;

            canvas.StrokeColor = TrackColor;
            canvas.DrawEllipse(centerX - radius, centerY - radius, radius * 2, radius * 2);

            var sweep = (float)Math.Clamp(Percentage, 0, 100) / 100f * 360f;
            if (sweep > 0)
            {
                canvas.StrokeColor = RingColor;
                canvas.DrawArc(centerX - radius, centerY - radius, radius * 2, radius * 2, 90, 90 - sweep, true, false);
            }

            canvas.FontColor = RingColor;
            canvas.FontSize = size * 0.22f;
            canvas.DrawString($"{Percentage:0}%", dirtyRect, HorizontalAlignment.Center, VerticalAlignment.Center);
        }
    }
}
