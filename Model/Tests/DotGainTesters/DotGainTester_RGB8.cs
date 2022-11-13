using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaScaler.Model {
    internal class DotGainTester_RGB8 {
        public static RawImage_RGB8 TestDotGain(string fname, int strength, int spread, double scaling_factor) {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            Console.WriteLine($"Testing DotGain on RGB8 image with strength {strength}, spread {spread}, scaling factor {scaling_factor:0.00}");

            FileNameInfo finfo = new FileNameInfo(fname);
            finfo.SetAdditionDotGain(strength, spread, scaling_factor);

            //Opening
            RawImage_RGB8 src_img = FileHandler.Open_JpegRGB8(finfo);
            if (src_img == null)
                return null;

            //Bluring
            watch.Start();
            RawImage_RGB8 res_img = DotGain_RGB8.SimulateDotGain(src_img, strength, spread, scaling_factor);
            watch.Stop();
            Console.WriteLine($"\tDotGain is applied in {watch.ElapsedMilliseconds} ms");
            watch.Reset();

            //Saving
            FileHandler.Write_JpegRGB8(res_img, finfo);

            return res_img;
        }

        public static void TestBatchStrength(string fname) {
            TestDotGain(fname, 1, 50, 0.5);
            TestDotGain(fname, 10, 50, 0.5);
            TestDotGain(fname, 20, 50, 0.5);
            TestDotGain(fname, 30, 50, 0.5);
            TestDotGain(fname, 40, 50, 0.5);
            TestDotGain(fname, 50, 50, 0.5);
            TestDotGain(fname, 60, 50, 0.5);
            TestDotGain(fname, 70, 50, 0.5);
            TestDotGain(fname, 80, 50, 0.5);
            TestDotGain(fname, 90, 50, 0.5);
            TestDotGain(fname, 100, 50, 0.5);
        }

        public static void TestBatchSpread(string fname) {
            TestDotGain(fname, 50, 10, 0.5);
            TestDotGain(fname, 50, 20, 0.5);
            TestDotGain(fname, 50, 30, 0.5);
            TestDotGain(fname, 50, 40, 0.5);
            TestDotGain(fname, 50, 50, 0.5);
            TestDotGain(fname, 50, 60, 0.5);
            TestDotGain(fname, 50, 70, 0.5);
            TestDotGain(fname, 50, 80, 0.5);
            TestDotGain(fname, 50, 90, 0.5);
            TestDotGain(fname, 50, 100, 0.5);

        }

    }
}
