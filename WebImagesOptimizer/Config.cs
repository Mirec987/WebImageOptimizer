namespace ImageOptimizer
{
    internal class Config
    {
        public string InputDirectory { get; set; }
        public string EditedDirectory { get; set; }
        public string NotEditedDirectory { get; set; }
        public string ThumbnailDirectory { get; set; }
        public int JpegQuality { get; set; }
        public int ThumbnailWidth { get; set; }
        public int ThumbnailHeight { get; set; }
    }
}
