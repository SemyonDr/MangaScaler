using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaScaler.Model {
    internal class DownscalerTester_RGB8 {

        public static RawImage_RGB8 TestDownscaler(string fname, double scaling_factor, bool dotgain, int dg_strength, int dg_spread) {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            Console.WriteLine($"Testing downscaling with factor {scaling_factor}.");

            FileNameInfo finfo = new FileNameInfo(fname);
            finfo.SetAdditionDownscale(scaling_factor, dotgain, dg_strength, dg_spread);

            //Opening
            RawImage_RGB8 src_img = FileHandler.Open_JpegRGB8(finfo);
            if (src_img == null)
                return null;

            //Dot gain
            if (dotgain) {
                watch.Start();
                src_img = DotGain_RGB8.SimulateDotGain(src_img, dg_strength, dg_spread, scaling_factor);
                watch.Stop();
                Console.WriteLine($"\tDotGain is applied in {watch.ElapsedMilliseconds} ms");
                watch.Reset();
            }

            //Resizing
            int new_height = (int)(src_img.Height * scaling_factor);
            int new_width = (int)(src_img.Width * scaling_factor);
            watch.Start();
            RawImage_RGB8 res_img = Downscaler_RGB8.Downscale(src_img, new_height, new_width);
            watch.Stop();
            Console.WriteLine($"\tDownscaled in {watch.ElapsedMilliseconds} ms");
            watch.Reset();
            

            //Saving
            FileHandler.Write_JpegRGB8(res_img, finfo);

            return res_img;
        }
    }
}
