using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaScaler.Model {
    internal class Blurer_RGB8 {
        /// <summary>
        /// Blur the image with Gaussian Blur with provided blur radius setting.
        /// </summary>
        /// <param name="source">Source image.</param>
        /// <param name="radius">Blur radius.</param>
        /// <returns>Blured image.</returns>
        public static RawImage_RGB8 BlurImage(RawImage_RGB8 source, double radius) {
            //Bluring rows
            RawImage_RGB8 blured_rows_img = BlurRows(source, radius);

            //Bluring columns
            RawImage_RGB8 blured_cols_img = BlurColumns(blured_rows_img, radius);

            return blured_cols_img;
        }


        /// <summary>
        /// Blurs rows of provided image and creates result image.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="radius"></param>
        /// <returns>Resulting image object.</returns>
        public static RawImage_RGB8 BlurRows(RawImage_RGB8 source, double radius) {
            //Creating distance weights
            double[] dist_weights = GFunc.CreateDistanceWeights_Normalized(radius);

            //Creating resulting image object
            RawImage_RGB8 res_img = new RawImage_RGB8(source.Height, source.Width);

            //Bluring rows in parallel
            Task[] blur_row_tasks = new Task[source.Height];
            for (int src_row = 0; src_row < source.Height; src_row++) {
                //Setting parameters
                BlurRowParams p = new BlurRowParams();
                p.src_img = source;
                p.src_row = src_row;
                p.dist_weights = dist_weights;
                p.res_img = res_img;

                //Starting task for bluring one row
                blur_row_tasks[src_row] = Task.Factory.StartNew(BlurRow, p);
            }
            //Waiting for all tasks to finish
            Task.WaitAll(blur_row_tasks);

            //Returning result object
            return res_img;
        }


        /// <summary>
        /// Arguments object for BlurRow method.
        /// </summary>
        private class BlurRowParams {
            public RawImage_RGB8 src_img; //Source image object
            public int src_row;           //Index of row to blur
            public RawImage_RGB8 res_img; //Result RawImage object
            public double[] dist_weights; //Distance weighs array
        }

        /// <summary>
        /// Blurs one row of an image.
        /// </summary>
        /// <param name="param">BlurRowParams arguments object.</param>
        private static void BlurRow(object param) {
            BlurRowParams p = param as BlurRowParams;

            //Alias for source image pixels array
            Pixel_RGB8[][] src_data = p.src_img.Pixel;

            //Iterating source pixels (targets)
            for (int src_col = 0; src_col < p.src_img.Width; src_col++) {
                //Resulting blured pixel
                DPixel_RGB res_px;

                //Resulting pixel value (target pixel that is sampled now)
                res_px = src_data[p.src_row][src_col].Scale(p.dist_weights[0]);

                //Applying weights to sample pixels before target
                for (int w = 1; w < p.dist_weights.Length; w++) {
                    if (src_col - w >= 0)
                        //If sampled pixel is inside of image array we sample it.
                        res_px += src_data[p.src_row][src_col - w].Scale(p.dist_weights[w]);
                    else
                        //If sampled pixel index is outside of image array we sample first pixel of the row.
                        res_px += src_data[p.src_row][0].Scale(p.dist_weights[w]);
                }

                //Applying weights to sample pixels after target
                for (int w = 1; w < p.dist_weights.Length; w++) {
                    if (src_col + w < p.src_img.Width)
                        //If sampled pixel is inside of image array we sample it.
                        res_px += src_data[p.src_row][src_col + w].Scale(p.dist_weights[w]);
                    else
                        //If sampled pixel index is outside of image array we sample last pixel of the row.
                        res_px += src_data[p.src_row][p.src_img.Width - 1].Scale(p.dist_weights[w]);
                }

                //Writing resulting pixel
                p.res_img.Pixel[p.src_row][src_col] = res_px.ToPixel_RGB8();

            }
        }//BlurRow end.


        /// <summary>
        /// Blurs columns of provided image and creates result image.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="radius"></param>
        /// <returns>Resulting image object.</returns>
        public static RawImage_RGB8 BlurColumns(RawImage_RGB8 source, double radius) {
            //Creating distance weights
            double[] dist_weights = GFunc.CreateDistanceWeights_Normalized(radius);

            //Creating result image object
            RawImage_RGB8 res_img = new RawImage_RGB8(source.Height, source.Width);

            //Bluring columns
            Task[] blur_col_tasks = new Task[source.Width];
            for (int src_col = 0; src_col < source.Width; src_col++) {
                //Setting task parameters
                BlurColumnParams p = new BlurColumnParams();
                p.src_img = source;
                p.src_col = src_col;
                p.res_img = res_img;
                p.dist_weights = dist_weights;

                //Starting task to blur one column
                blur_col_tasks[src_col] = Task.Factory.StartNew(BlurColumn, p);
            }
            //Waiting for all tasks to finish
            Task.WaitAll(blur_col_tasks);

            return res_img;
        }


        /// <summary>
        /// Arguments object for BlurColumn method.
        /// </summary>
        private class BlurColumnParams {
            public RawImage_RGB8 src_img;   //Source image object
            public int src_col;             //Target column index
            public RawImage_RGB8 res_img;   //Resulting image object
            public double[] dist_weights;   //Distance weights array
        }

        /// <summary>
        /// Blurs one column of an image.
        /// </summary>
        /// <param name="param">BlurColumnParams arguments object.</param>
        private static void BlurColumn(object param) {
            BlurColumnParams p = param as BlurColumnParams;

            //Source pixels alias
            Pixel_RGB8[][] src_data = p.src_img.Pixel;

            //Iterating target column rows (column pixels)
            for (int src_row = 0; src_row < p.src_img.Height; src_row++) {
                //Resulting pixel
                DPixel_RGB res_px;

                //Sampling target pixel
                res_px = src_data[src_row][p.src_col].Scale(p.dist_weights[0]);

                //Applying weights to sample pixels above target
                for (int w = 1; w < p.dist_weights.Length; w++) {
                    if (src_row - w >= 0)
                        //If sampled pixel is inside of image array we sample it.
                        res_px += src_data[src_row - w][p.src_col].Scale(p.dist_weights[w]);
                    else {
                        //If sampled pixel index is outside of image array we sample first pixel of the column.
                        res_px += src_data[0][p.src_col].Scale(p.dist_weights[w]);
                    }
                }

                //Applying weights to sample pixels below target
                for (int w = 1; w < p.dist_weights.Length; w++) {
                    //If sampled pixel is inside of image array we sample it.
                    if (src_row + w < p.src_img.Height) {
                        res_px += src_data[src_row + w][p.src_col].Scale(p.dist_weights[w]);
                    }
                    else {
                        //If sampled pixel index is outside of image array we sample last pixel of the row.
                        res_px += src_data[p.src_img.Height - 1][p.src_col].Scale(p.dist_weights[w]);
                    }
                }

                p.res_img.Pixel[src_row][p.src_col] = res_px.ToPixel_RGB8();
            }
        }//BlurColumn end.
    }
}
