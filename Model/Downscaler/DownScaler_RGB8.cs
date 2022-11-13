using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaScaler.Model {
    /// <summary>
    /// Downscaler for grayscale images without alpha channel and bit depth 8 bit per component.
    /// </summary>
    internal class Downscaler_RGB8 {
        /// <summary>
        /// Downscales source image.
        /// </summary>
        /// <param name="source">Source image.</param>
        /// <param name="new_width">New width.</param>
        /// <param name="new_height">New height.</param>
        /// <returns>Downscaled image object.</returns>
        public static RawImage_RGB8 Downscale(RawImage_RGB8 source, int new_height, int new_width) {
            //Pixel binning algorithm is when each pixel of resulting resized image 
            //consist of average of each pixel that corresponds to it in the original image.
            //This averaging might be done in two passes -
            //first pass resizes the rows, second one takes resized rows as input and
            //resizes them vertically.
            //Each one of those two passes should first accumulate values for resized
            //pixel and then average them.
            //We can make algorithm more efficient if we make two passes and only accumulate
            //values and then average them in a third pass.

            //We accumulate rows values
            DRawImage_RGB rows_accumulator = new DRawImage_RGB(source.Height, new_width, new DPixel_RGB(0.0, 0.0, 0.0));

            Task[] acc_rows_tasks = new Task[source.Height];
            for (int row = 0; row < source.Height; row++) {
                //Setting row accumulation parameters
                AccumulateRowParams p = new AccumulateRowParams();
                p.rows_accumulator = rows_accumulator;
                p.new_width = new_width;
                p.row = row;
                p.source = source;

                //Starting task for one row
                acc_rows_tasks[row] = Task.Factory.StartNew(AccumulateRow, p);
            }
            //Waiting for all tasks to finish
            Task.WaitAll(acc_rows_tasks);

            //We accumulate columns values of accumulated rows
            DRawImage_RGB cols_accumulator = new DRawImage_RGB(new_height, new_width, new DPixel_RGB(0.0,0.0,0.0));

            Task[] acc_cols_tasks = new Task[new_width];
            for (int col = 0; col < new_width; col++) {
                //Setting column accumulation parameters
                AccumulateColParams p = new AccumulateColParams();
                p.cols_accumulator = cols_accumulator;
                p.rows_accumulator = rows_accumulator;
                p.col = col;
                p.new_height = new_height;
               
                //Starting task for one column
                acc_cols_tasks[col] = Task.Factory.StartNew(AccumulateCol, p);
            }
            //Waiting for all tasks to finish
            Task.WaitAll(acc_cols_tasks);

            //Resulting image object
            RawImage_RGB8 res_img = new RawImage_RGB8(new_height, new_width);

            //Averaging accumulated values and writing results
            double avg_factor = 1 / (((double)source.Width / (double)new_width) * ((double)source.Height / (double)new_height)); //Averaging factor
            for (int row = 0; row < new_height; row++) {
                for (int col = 0; col < new_width; col++) {
                    res_img.Pixel[row][col] = cols_accumulator.Pixel[row][col].Scale(avg_factor).ToPixel_RGB8();
                }
            }

            return res_img;
        }


        /// <summary>
        /// Arguments object for AccumulateRow method.
        /// </summary>
        private class AccumulateRowParams {
            public RawImage_RGB8 source;            //Source image
            public int row;                         //Index of source row
            public DRawImage_RGB rows_accumulator; //Resulting array of accumulated rows
            public int new_width;                   //New width of the row in pixels
        }

        /// <summary>
        /// Accumulates values of source row pixels to result row and writes it to provided result array.
        /// </summary>
        /// <param name="param">Arguments object.</param>
        private static void AccumulateRow(object param) {
            //Unpacking the arguments
            AccumulateRowParams p = param as AccumulateRowParams;

            //Creating pixel bin
            PixelBin bin = new PixelBin(p.source.Width, p.new_width);

            //Source data alias
            Pixel_RGB8[] src_row = p.source.Pixel[p.row];

            //Resulting row alias
            DPixel_RGB[] res_row = p.rows_accumulator.Pixel[p.row];

            //Iterating source row pixels
            for (int px = 0; px < p.source.Width; px++) {
                if (!bin.OnMargin(px)) {
                    //Pixel is fully inside the bin
                    res_row[bin.Target] += src_row[px].ToDouble();
                }
                else {
                    //Pixel is on the right margin of the bin (split by boundary)

                    //We add pixel adjusted by margin weight
                    res_row[bin.Target] += src_row[px].Scale(bin.Margin);

                    if (px != p.source.Width - 1) {
                        //If there is next pixel in the row we accumulate it to next accumulator pixel weighted by adjacent margin weight
                        res_row[bin.Target + 1] += src_row[px].Scale(bin.MarginNext);

                        //We advance pixel bin
                        bin.Advance();
                    }
                }
            }
        }


        /// <summary>
        /// Arguments object for AccumulateCol method.
        /// </summary>
        private class AccumulateColParams {
            public DRawImage_RGB rows_accumulator; //Resulting array of accumulated rows
            public DRawImage_RGB cols_accumulator; //Result array of accumulated columns
            public int col; //Index of source column
            public int new_height; //New height of the column in pixels
        }

        /// <summary>
        /// Accumulates values of source column pixels and writes it to provided result array.
        /// </summary>
        /// <param name="param">Arguments object.</param>
        private static void AccumulateCol(object param) {
            //Unpacking the arguments
            AccumulateColParams p = param as AccumulateColParams;

            //Creating pixel bin
            PixelBin bin = new PixelBin(p.rows_accumulator.Height, p.new_height);

            //Iterating source column pixels
            for (int px = 0; px < p.rows_accumulator.Height; px++) {
                if (!bin.OnMargin(px)) {
                    //Pixel is fully inside the bin
                    p.cols_accumulator.Pixel[bin.Target][p.col] += p.rows_accumulator.Pixel[px][p.col];
                }
                else {
                    //Pixel is partially inside the bin

                    //We add pixel adjusted by margin weight
                    p.cols_accumulator.Pixel[bin.Target][p.col] += p.rows_accumulator.Pixel[px][p.col].Scale(bin.Margin);

                    if (px != p.rows_accumulator.Height - 1) {
                        //If there is next pixel in the row we accumulate it to next accumulator pixel weighted by adjacent margin weight
                        p.cols_accumulator.Pixel[bin.Target + 1][p.col] += p.rows_accumulator.Pixel[px][p.col].Scale(bin.MarginNext);

                        //We advance pixel bin
                        bin.Advance();
                    }
                }
            }
        }
    }
}
