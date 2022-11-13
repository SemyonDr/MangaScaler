using BitMiracle.LibJpeg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaScaler.Model {
    internal class BlurTester_RGB8 {
        public static RawImage_RGB8 TestBlur(string fname, double radius) {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            Console.WriteLine($"Testing blug on RGB8 image with radius {radius:0.00}");

            FileNameInfo finfo = new FileNameInfo(fname);
            finfo.SetAdditionBlur(radius);

            //Opening
            RawImage_RGB8 src_img = FileHandler.Open_JpegRGB8(finfo);
            if (src_img == null)
                return null;

            //Bluring
            watch.Start();
            RawImage_RGB8 res_img = Blurer_RGB8.BlurImage(src_img, radius);
            watch.Stop();
            Console.WriteLine($"\tBlur is applied in {watch.ElapsedMilliseconds} ms");
            watch.Reset();

            //Saving
            FileHandler.Write_JpegRGB8(res_img, finfo);

            return res_img;
        }

        public static void TestBatch(string fname) {
            TestBlur(fname, 0.5);
            TestBlur(fname, 0.75);
            TestBlur(fname, 0.83);
            TestBlur(fname, 1);
            TestBlur(fname, 1.33);
            TestBlur(fname, 2);
            TestBlur(fname, 3);
            TestBlur(fname, 4);
            TestBlur(fname, 4.58);
            TestBlur(fname, 5);
            TestBlur(fname, 10);
            TestBlur(fname, 15);
            TestBlur(fname, 15.67);
            TestBlur(fname, 20);
            TestBlur(fname, 25);
            TestBlur(fname, 50);
        }

    }
}
