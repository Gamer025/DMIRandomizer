using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
