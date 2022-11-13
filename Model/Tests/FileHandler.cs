using BitMiracle.LibJpeg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaScaler.Model {
    internal class FileHandler {

        /// <summary>
        /// Opens RGB8 Jpeg.
        /// </summary>
        /// <param name="fname"></param>
        /// <returns></returns>
        public static RawImage_RGB8 Open_JpegRGB8(FileNameInfo finfo) {
            Console.WriteLine($"\tOpening \"{finfo.full_fname_src}\".");

            //Checking if file is jpeg
            if ((finfo.ext != ".jpeg") && (finfo.ext != ".jpg")) {
                Console.WriteLine("\tFile is not jpeg. Abort.");
                return null;
            }

            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            //Opening the file
            FileStream fs = File.OpenRead(finfo.full_fname_src);
            JpegImage src_jpeg = new JpegImage(fs);
            fs.Close();
            watch.Stop();
            Console.WriteLine($"\tFile is opened in {watch.ElapsedMilliseconds} ms");
            watch.Reset();


            //Checking if file is in RGB8 format
            if (src_jpeg.ComponentsPerSample == 3 && src_jpeg.BitsPerComponent == 8)
                Console.WriteLine("\tFile is RGB8.");
            else {
                Console.WriteLine("\tFile is not RGB8. Abort.");
                return null;
            }

            watch.Start();
            //Converting to RawImage
            RawImage_RGB8 src_img = new RawImage_RGB8(src_jpeg);
            watch.Stop();
            Console.WriteLine($"\tConverted to Raw in {watch.ElapsedMilliseconds} ms");
            watch.Reset();

            return src_img;
        }



        /// <summary>
        /// Writes RGB8 Jpeg to disk.
        /// </summary>
        /// <param name="src_fname"></param>
        /// <param name="radius"></param>
        public static void Write_JpegRGB8(RawImage_RGB8 res_img, FileNameInfo finfo) {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

            //Converting back to jpeg
            watch.Start();
            JpegImage res_jpeg = res_img.ToJpegImage();
            watch.Stop();
            Console.WriteLine($"\tConverted to jpeg in {watch.ElapsedMilliseconds} ms");
            watch.Reset();

            //Writing resulting image
            FileStream result_stream = File.Open($"{finfo.full_fname_res}", FileMode.OpenOrCreate);

            CompressionParameters p = new CompressionParameters();
            p.Quality = 100;
            res_jpeg.WriteJpeg(result_stream, p);
            result_stream.Close();

            watch.Start();
            Console.WriteLine($"{finfo.full_fname_res} saved");
            watch.Stop();
            Console.WriteLine($"\tFile saved in {watch.ElapsedMilliseconds} ms");
            watch.Reset();

            Console.WriteLine("----------------\n\n");
        }
    }

    public class FileNameInfo {
        public string full_fname_src;
        public string full_fname_res {
            get { return full_fname_src + addition + ext; }
            private set { }
        }
        public string fname; //Name without extension
        public string ext; //Extension with a dot
        public string addition;

        public FileNameInfo(string fname) {
            full_fname_src = fname;
            addition = "";

            //Getting file extension and name 
            string src_fname; {
                int ext_len = 0;
                while (fname[fname.Length - 1 - ext_len] != '.')
                    ext_len++;
                ext = fname.Substring(fname.Length - ext_len - 1, ext_len + 1);
                src_fname = fname.Substring(0, fname.Length - ext_len - 1);
            }
        }

        public void SetAdditionBlur(double radius) {
            addition = $"_blur_{radius:0.00}";
        }

        public void SetAdditionDotGain(int strength, int spread, double scaling_factor) {
            addition = $"_dotgain_St{strength}_Sp{spread}_Sc{scaling_factor:0.00}";
        
        }

        public void SetAdditionDownscale(double sc_factor, bool dotgain, int strength, int spread) {
            if (dotgain)
                addition = $"_down_{sc_factor}_dg_st{strength}_sp{spread}";
            else
                addition = $"_down_{sc_factor}_nodg";
        }
    }
}
