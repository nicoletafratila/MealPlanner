using System.Drawing;

namespace MealPlanner.UI.Web
{
    public static class Extensions
    {
        public static List<string> GetBackgroundColors(int counter)
        {
            var rand = new Random();
            var colors = new List<string>();
            for (var index = 0; index < counter; index++)
            {
                var color = string.Empty;
                while (!colors.Contains(color))
                {
                    color = string.Concat("#", Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256)).Name.AsSpan(2));
                    if (!colors.Contains(color))
                        colors.Add(color);
                }
            }
            return colors;
        }
    }
}
