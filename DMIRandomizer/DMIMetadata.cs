namespace DMIRandomizer
{
    public class DMIMetadata
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int SpriteCount { get; set; }

        public DMIMetadata(int width, int height, int spriteCount)
        {
            Width = width;
            Height = height;
            SpriteCount = spriteCount;
        }
    }
}
