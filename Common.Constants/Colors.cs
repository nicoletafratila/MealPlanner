using System.Drawing;

namespace Common.Constants
{
    public static class Colors
    {
        public static List<string> GetBackgroundColors(int counter, Color baseColor)
        {
            var colors = new List<string>();
            for (var index = 0; index < counter; index++)
            {
                float brightnessFactor = 1.0f - (index * 1.0f / counter);
                var adjustedColor = ChangeColorBrightness(baseColor, brightnessFactor);
                var colorHex = $"#{adjustedColor.R:X2}{adjustedColor.G:X2}{adjustedColor.B:X2}";
                colors.Add(colorHex);
            }
            return colors;
        }

        private static Color ChangeColorBrightness(Color color, float correctionFactor)
        {
            float red = color.R;
            float green = color.G;
            float blue = color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
        }
    }
}
