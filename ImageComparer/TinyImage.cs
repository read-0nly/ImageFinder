using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;

namespace ImageComparer
{
    class TinyImage
    {
        public int[] sizes = { 8, 16, 32, 64, 128, 256, 512 };
        public int setSize = 0;
        public int mode = 0;
        public float percentile;
        public float threshold;
        public const int MODE_HUE=0;
        public const int MODE_SAT=1;
        public const int MODE_LIGHT=2;
        public const int MODE_R=3;
        public const int MODE_G =4;
        public const int MODE_B=5;
        public const int MODE_COLOR =6;

        public TinyImage(int s1,int m1)
        {
            setSize = s1;
            mode = m1;
        }
        public Image getResized(Image i1)
        {
            return (Image)(new Bitmap(i1, new Size(sizes[setSize], sizes[setSize])));
        }

        public float compareImages(string i1, string i2)
        {
            try
            {
                Bitmap b1 = new Bitmap(new Bitmap(i1), new Size(sizes[setSize], sizes[setSize]));
                Bitmap b2 = new Bitmap(new Bitmap(i2), new Size(sizes[setSize], sizes[setSize]));
                float f1 = 0.0f;

                for (int x = 0; x < sizes[setSize]; x++)
                {
                    for (int y = 0; y < sizes[setSize]; y++)
                    {
                        f1 += comparePixels(b1.GetPixel(x, y), b2.GetPixel(x, y));
                    }
                }
                b1.Dispose();
                b2.Dispose();
                return f1;
            }
            catch (Exception e)
            {
                return -512;
            }
        }

        public float compareImages(Image i1, Image i2)
        {
            try
            {
                Bitmap b1 = (Bitmap)i1;
                Bitmap b2 = (Bitmap)i2;
                float f1 = 0.0f;

                for (int x = 0; x < sizes[setSize]; x++)
                {
                    for (int y = 0; y < sizes[setSize]; y++)
                    {
                        f1 += comparePixels(b1.GetPixel(x, y), b2.GetPixel(x, y));
                    }
                }
                return f1;
            }
            catch (Exception e)
            {
                return -512;
            }
        }

        private float comparePixels(Color c1, Color c2)
        {
            float f1 = 200000;
            switch (mode)
            {
                case MODE_HUE:
                    f1 = Math.Abs(c1.GetHue() - c2.GetHue());                    
                    break;
                case MODE_SAT:
                    f1 = Math.Abs(c1.GetSaturation() - c2.GetSaturation());
                    break;
                case MODE_LIGHT:
                    f1 = Math.Abs(c1.GetBrightness() - c2.GetBrightness());
                    break;
                case MODE_R:
                    f1 = Math.Abs(c1.R - c2.R);
                    break;
                case MODE_G:
                    f1 = Math.Abs(c1.G - c2.G);
                    break;
                case MODE_B:
                    f1 = Math.Abs(c1.B - c2.B);
                    break;
            }
            return f1;
        }

    }
}
