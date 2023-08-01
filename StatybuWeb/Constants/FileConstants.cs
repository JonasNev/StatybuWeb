namespace StatybuWeb.Constants
{
    public static class FileConstants
    {
        public static class FileExtensions
        {
            public const string Jpg = ".jpg";
            public const string Png = ".png";
            public const string Gif = ".gif";
            public const string Jpeg = ".jpeg";
        }

        public static readonly IEnumerable<string> ImageExtensions = new List<string>()
        {
            new string(FileExtensions.Png),
            new string(FileExtensions.Jpg),
            new string(FileExtensions.Jpeg)
        };
    }
}
