using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaScaler.Model {
    internal class DRawImage_G {
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
            get { return 1; }
            private set { }
        }


        /// <summary>
        /// Array that contains image pixels
        /// Order is [Row][Column].
        /// </summary>
        public DPixel_G[][] Pixel { get; private set; }


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
        public DRawImage_G(int Height, int Width) {
            //Creating pixel rows array
            Pixel = new DPixel_G[Height][];

            //Initializing rows in parallel
            Task[] create_row_tasks = new Task[Height];

            //Create empty row lambda function
            Action<object> create_row = (param) => {
                CreateRowParams p = param as CreateRowParams;
                Pixel[p.row] = new DPixel_G[Width];
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
        public DRawImage_G(int Height, int Width, double init) {
            //Creating pixel rows array
            Pixel = new DPixel_G[Height][];

            //Initializing rows in parallel
            Task[] create_row_tasks = new Task[Height];

            //Create row lambda function
            Action<object> create_row = (param) => {
                CreateRowParams p = param as CreateRowParams;
                DPixel_G[] new_row = new DPixel_G[Width];
                for (int px = 0; px < Width; px++)
                    new_row[px] = new DPixel_G(init);
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
        public DRawImage_G(DPixel_G[][] pixels) {
            this.Pixel = pixels;
        }


        //------------------------------------------
        //METHODS
        //------------------------------------------

        /// <summary>
        /// Sets pixel values.
        /// </summary>
        /// <param name="row">Pixel row in the image.</param>
        /// <param name="col">Pixel column in the image.</param>
        /// <param name="r">Red component.</param>
        /// <param name="g">Green component.</param>
        /// <param name="b">Blue component.</param>
        public void SetPixel(int row, int col, double gr) {
            Pixel[row][col].Gr = gr;
        }
    }
}
