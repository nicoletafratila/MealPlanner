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

            var darkest = Color.DarkOliveGreen;

            // Lightest color: baseColor lightened, but not all the way to white
            const float maxLightenFactor = 0.7f;
            var lightest = ChangeColorBrightness(baseColor, maxLightenFactor);

            if (counter == 1)
            {
                // Single bucket -> just use darkest allowed color
                colors.Add($"#{darkest.R:X2}{darkest.G:X2}{darkest.B:X2}");
                return colors;
            }

            for (var index = 0; index < counter; index++)
            {
                // t=0 -> darkest (most used), t=1 -> lightest (least used)
                var t = (float)index / (counter - 1);

                int r = (int)Math.Round(darkest.R + (lightest.R - darkest.R) * t);
                int g = (int)Math.Round(darkest.G + (lightest.G - darkest.G) * t);
                int b = (int)Math.Round(darkest.B + (lightest.B - darkest.B) * t);

                r = Math.Clamp(r, 0, 255);
                g = Math.Clamp(g, 0, 255);
                b = Math.Clamp(b, 0, 255);

                colors.Add($"#{r:X2}{g:X2}{b:X2}");
            }

            return colors;
        }

        private static Color ChangeColorBrightness(Color color, float correctionFactor)
        {
            correctionFactor = Math.Clamp(correctionFactor, -1f, 1f);

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

            int r = Math.Clamp((int)Math.Round(red), 0, 255);
            int g = Math.Clamp((int)Math.Round(green), 0, 255);
            int b = Math.Clamp((int)Math.Round(blue), 0, 255);

            return Color.FromArgb(color.A, r, g, b);
        }
    }
}