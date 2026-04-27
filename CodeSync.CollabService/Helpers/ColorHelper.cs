namespace CodeSync.CollabService.Helpers
{
    public static class ColorHelper
    {
        private static readonly string[] Colors = new[]
        {
            "#f85149", // red
            "#3fb950", // green
            "#f0c040", // yellow
            "#00b0ff", // blue
            "#a371f7"  // purple
        };

        private static int _index = 0;

        public static string GetNextColor()
        {
            var color = Colors[_index % Colors.Length];
            _index++;
            return color;
        }
    }
}