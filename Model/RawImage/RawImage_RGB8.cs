using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitMiracle.LibJpeg;

namespace MangaScaler.Model {
    /// <summary>
    /// Object for storing raw image pixels.
    /// Suitable for RGB images with 8 bit per component.
    /// </summary>
    internal class RawImage_RGB8 {

        //------------------------------------------
        //PUBLIC PROPERTIES
        //------------------------------------------

        /// <summary>
        /// Height in pixels.
        /// </summary>
        public int Height {
            get {
                return Pixel.Length;
            }
            private set { }
        }

        /// <summary>
        /// Width in pixels.
        /// </summary>
        public int Width {
            get {
                return Pixel[0].Length;
            }
            private set { }
        }


        /// <summary>
        /// Number of pixel components.
        /// </summary>
        public int NumCmp {
            get { return 3; }
            private set { }
        }


        /// <summary>
        /// Bit depth of image pixel component.
        /// </summary>
        public int CmpBitDepth {
            get { return 8; }
            private set { }

        }


        /// <summary>
        /// Array that contains image pixels
        /// Order is [Row][Column].
        /// </summary>
        public Pixel_RGB8[][] Pixel { get; private set; }


        //------------------------------------------
        //CONSTRUCTORS
        //------------------------------------------

        private class CreateRowParams {
            public int row;
        }

        /// <summary>
        /// Creates empty image. Individual pixels are NOT created.
        /// </summary>
        /// <param name="Width">Width of the new image.</param>
        /// <param name="Height">Height of the new image.</param>
        public RawImage_RGB8(int Height, int Width) { 
            //Creating pixel rows array
            Pixel = new Pixel_RGB8[Height][];

            //Initializing rows in parallel
            Task[] create_row_tasks = new Task[Height];

            //Create empty row lambda function
            Action<object> create_row = (param) => {
                CreateRowParams p = param as CreateRowParams;
                Pixel[p.row] = new Pixel_RGB8[Width];
            };

            //Creating tasks
            for (int row = 0; row < Height; row++) {
                CreateRowParams p = new CreateRowParams();
                p.row = row;
                create_row_tasks[row] = Task.Factory.StartNew(create_row, p);
            }

            //Waiting tasks to complete
            Task.WaitAll(create_row_tasks);
        }


        /// <summary>
        /// Creates empty image. Initializes to given values.
        /// </summary>
        /// <param name="Width">Width of the new image.</param>
        /// <param name="Height">Height of the new image.</param>
        public RawImage_RGB8(int Height, int Width, Pixel_RGB8 init_px) {
            //Creating pixel rows array
            Pixel = new Pixel_RGB8[Height][];

            //Initializing rows in parallel
            Task[] create_row_tasks = new Task[Height];

            //Create row lambda function
            Action<object> create_row = (param) => {
                CreateRowParams p = param as CreateRowParams;
                Pixel_RGB8[] new_row = new Pixel_RGB8[Width];
                for (int pix = 0; pix < Width; pix++)
                    new_row[pix] = new Pixel_RGB8(init_px.R, init_px.G, init_px.B);
                Pixel[p.row] = new_row;
            };

            //Creating tasks
            for (int row = 0; row < Height; row++) {
                CreateRowParams p = new CreateRowParams();
                p.row = row;
                create_row_tasks[row] = Task.Factory.StartNew(create_row, p);
            }

            //Waiting tasks to complete
            Task.WaitAll(create_row_tasks);
        }


        /// <summary>
        /// Creates RawImage object with pixels data.
        /// </summary>
        /// <param name="data">Image data array.</param>
        /// <param name="Layout">Image pixel components layout.</param>
        public RawImage_RGB8(Pixel_RGB8[][] pixels) {
            this.Pixel = pixels;
        }

        /// <summary>
        /// Creates RawImage from JpegImage by copying content of the Jpeg image.
        /// </summary>
        /// <param name="Jpeg_Image">Source Jpeg image.</param>
        public RawImage_RGB8(JpegImage Jpeg_Image) {
            //Creating Pixels array
            Pixel = new Pixel_RGB8[Jpeg_Image.Height][];

            //Copying jpeg rows bytes in parallel
            Task[] copy_row_tasks = new Task[Jpeg_Image.Height];

            //Copy row lambda function
            Action<object> copy_row = (param) => {
                CopyRowParams p = param as CopyRowParams;
                //Creating result array
                Pixel_RGB8[] pixel_row = new Pixel_RGB8[Jpeg_Image.Width];
                byte[] jpegRowBytes = p.JpegImage.GetRow(p.Row).ToBytes();
                //Copying jpeg bytes to RawImage pixels
                for (int px = 0; px < Jpeg_Image.Width; px++)
                    pixel_row[px] = new Pixel_RGB8(jpegRowBytes[px * 3 + 0], jpegRowBytes[px * 3 + 1], jpegRowBytes[px * 3 + 2]);
                //Inserting pixels row to Pixels array
                Pixel[p.Row] = pixel_row;
            };

            //Copying rows
            for (int row = 0; row < Jpeg_Image.Height; row++) {
                //Writing parameters
                CopyRowParams p = new CopyRowParams();
                p.JpegImage = Jpeg_Image;
                p.Row = row;
                //Creating task
                copy_row_tasks[row] = Task.Factory.StartNew(copy_row, p);
            }
            //Waiting for tasks to complete
            Task.WaitAll(copy_row_tasks);
        }

        /// <summary>
        /// Parameters for copying row from jpeg to RawImage in parallel.
        /// </summary>
        private class CopyRowParams {
            public JpegImage JpegImage;
            public int Row;
        }


        //------------------------------------------
        //METHODS
        //------------------------------------------

        /// <summary>
        /// Arguments for converting RawImage row to Jpeg row.
        /// </summary>
        private class RowToJpegParams {
            public SampleRow[] JpegRows;
            public int Row;
            public int Width;
        }

        /// <summary>
        /// Produces JpegImage from this RawImage by copying pixel data.
        /// </summary>
        /// <returns>Jpeg image object.</returns>
        public JpegImage ToJpegImage() {
            //Jpeg rows array
            SampleRow[] jpeg_rows = new SampleRow[Height];

            //Converting rows to jpeg rows in parallel
            Task[] row_to_jpeg_tasks = new Task[Height];

            //Lamda function that converts RawImage row to Jpeg row
            Action<object> row_to_jpeg = (param) => {
                RowToJpegParams p = param as RowToJpegParams;
                //Copying pixel components to bytes array
                byte[] row_bytes = new byte[Width * 3];
                for (int px = 0; px < Width; px++) {
                    row_bytes[px * 3 + 0] = Pixel[p.Row][px].R;
                    row_bytes[px * 3 + 1] = Pixel[p.Row][px].G;
                    row_bytes[px * 3 + 2] = Pixel[p.Row][px].B;
                }
                p.JpegRows[p.Row] = new SampleRow(row_bytes, p.Width, 8, 3);
            };

            //Creating tasks
            for (int row = 0; row < Height; row++) {
                //Creating arguments object
                RowToJpegParams p = new RowToJpegParams();
                p.Row = row;
                p.Width = Width;
                p.JpegRows = jpeg_rows;

                //Starting task
                row_to_jpeg_tasks[row] = Task.Factory.StartNew(row_to_jpeg, p);
            }

            //Waiting for tasks to finish
            Task.WaitAll(row_to_jpeg_tasks);

            //Creating Jpeg from rows
            return new JpegImage(jpeg_rows, Colorspace.RGB);
        }

        /// <summary>
        /// Sets pixel values.
        /// </summary>
        /// <param name="row">Pixel row in the image.</param>
        /// <param name="col">Pixel column in the image.</param>
        /// <param name="r">Red component.</param>
        /// <param name="g">Green component.</param>
        /// <param name="b">Blue component.</param>
        public void SetPixel(int row, int col, byte r, byte g, byte b) {
            Pixel[row][col].R = r;
            Pixel[row][col].G = g;
            Pixel[row][col].B = b;
        }


        ///// <summary>
        ///// Converts this RawImage to RawImageLuminance.
        ///// </summary>
        ///// <returns>New RawImageLuminance object.</returns>
        //public RawImageLuminance ToLuminance() {
        //    return null;
        //}

        //private class RowToLuminanceParams {
        //    public int row;
        //    public double[][] LumData;
        //    public byte[][] AlphaData;
        //    public LuminanceConverter lc;
        //}

        ///// <summary>
        ///// Converts RawImage to RawImageLuminance when data layout is G8.
        ///// </summary>
        ///// <returns>New RawImageLuminance object.</returns>
        //private RawImageLuminance ToLuminanceG8() {
        //    //Luminance data
        //    double[][] LumData = new double[Height][];

        //    LuminanceConverter lc = LuminanceConverterFactory.CreateConverter(8);

        //    //Creating task for each row
        //    Task[] row_to_lum_tasks = new Task[Height];
        //    //Lamda function for conversion
        //    Action<object> row_to_lum = (param) => {
        //        RowToLuminanceParams p = param as RowToLuminanceParams;
        //        //Creating row
        //        double[] lum_row = new double[this.Width];
        //        //Converting each pixel to luminance
        //        for (int px = 0; px < this.Width; px++)
        //            lum_row[px] = p.lc.ComponentToLuminance(this.Data[p.row][px]);
        //        //Attaching row
        //        p.LumData[p.row] = lum_row;
        //    };

        //    //Starting tasks
        //    for (int row = 0; row < this.Height; row++) {
        //        RowToLuminanceParams p = new RowToLuminanceParams();
        //        p.LumData = LumData;
        //        p.row = row;
        //        p.lc = lc;
        //        row_to_lum_tasks[row] = Task.Factory.StartNew(row_to_lum, p);
        //    }

        //    //Waiting for tasks to complete
        //    Task.WaitAll(row_to_lum_tasks);

        //    //Creaing and returning raw luminance object
        //    return new RawImageLuminance(LumData, RawImageLayout.G8);
        //}


    }
}
