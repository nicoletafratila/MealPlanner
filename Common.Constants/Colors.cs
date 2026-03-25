using System.Drawing;

namespace Common.Constants
{
    public static class Colors
    {
        public static List<string> GetBackgroundColors(int counter, Color baseColor)
        {
            var colors = new List<string>();
            if (counter <= 0)
            {
                return colors;
            }

            for (var index = 0; index < counter; index++)
            {
                // 1.0 → lightest (closest to white), ~1/counter → closest to baseColor
                var brightnessFactor = 1.0f - (index * 1.0f / counter);
                var adjustedColor = ChangeColorBrightness(baseColor, brightnessFactor);
                var colorHex = $"#{adjustedColor.R:X2}{adjustedColor.G:X2}{adjustedColor.B:X2}";
                colors.Add(colorHex);
            }

            return colors;
        }

        private static Color ChangeColorBrightness(Color color, float correctionFactor)
        {
            // clamp to expected range
            correctionFactor = Math.Clamp(correctionFactor, -1f, 1f);

            float red = color.R;
            float green = color.G;
            float blue = color.B;

            if (correctionFactor < 0)
            {
                // Darken
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                // Lighten
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            int r = Math.Clamp((int)Math.Round(red), 0, 255);
            int g = Math.Clamp((int)Math.Round(green), 0, 255);
            int b = Math.Clamp((int)Math.Round(blue), 0, 255);

            return Color.FromArgb(color.A, r, g, b);
        }
    }
}