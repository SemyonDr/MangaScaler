using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaScaler.Model {
    internal class DotGain_RGB8 : DotGain {
        /// <summary>
        /// Arguments object for ProcessRow method.
        /// </summary>
        private class ProcessRowParams {
            public RawImage_RGB8 src_img; //Source image
            public int src_row; //Number of row to process in the source image
            public RawImage_RGB8 res_img; //Resulting image byte array
            public double[][] dist_matrix; //Array of distance weights
            public double[] brightness_weights; //Array of brightness weights
        }


        /// <summary>
        /// Applies DotGain effect on one row of souce image and writes result into provided result array.
        /// </summary>
        /// <param name="param">ProcessRowParams Arguments object.</param>
        private static void ProcessRow(object param) {
            ProcessRowParams p = param as ProcessRowParams;

            //Source image data alias
            Pixel_RGB8[][] src_data = p.src_img.Pixel;

            //We use distance weights and brightness weights to create
            //individual weights matrix for each pixel of the row.
            //Resulting pixel is the weighted average of neigboring pixels
            //which we call samples and weights come from the combined weights matrix.
            //Combined weights matrix itself isn't stored explicitly, instead we store
            //weighted samples and accumulate weights sum.
            //When every sample pixel is weighted we sum them and average their value using weights sum.
            //This way we can avoid traversing samples twice.

            //Size of the combined weights matrix (alias)
            int m_size = p.dist_matrix.Length;

            //Matrix for storing weighted samples.
            DPixel_RGB[][] weighted_samples = new DPixel_RGB[m_size][];
            //Initializing weighted samples matrix
            for (int smp_matrix_row = 0; smp_matrix_row < m_size; smp_matrix_row++)
                weighted_samples[smp_matrix_row] = new DPixel_RGB[m_size];

            //ITERATING SOURCE ROW PIXELS
            //We call current source pixel the target.
            //-----------------------------------------
            for (int src_col = 0; src_col < p.src_img.Width; src_col++) {
                //Weighting samples
                double cmb_weights_sum = WeightSamples(src_data, p.src_row, src_col, m_size, weighted_samples, p.dist_matrix, p.brightness_weights);

                //Now we have all samples weighted by combined brightness and distance weights in one matrix.
                //We accumulate resulting pixels brightness in one value.
                DPixel_RGB res_px = new DPixel_RGB(0.0, 0.0, 0.0);
                for (int m_row = 0; m_row < m_size; m_row++) {
                    for (int m_col = 0; m_col < m_size; m_col++) {
                        res_px += weighted_samples[m_row][m_col];
                    }
                }

                //Now we find average weighted brightness
                res_px = res_px.Scale(1/cmb_weights_sum);

                //Converting result to byte and saving the result
                p.res_img.Pixel[p.src_row][src_col] = res_px.ToPixel_RGB8();

            }//Source row pixels iterations end.-----------------
        }//End of ProcessRow method.



        /// <summary>
        /// Weights samples around target pixel by distance and brightness.
        /// Writes weighted values to provided matrix.
        /// </summary>
        /// <param name="src_data">Source image data array.</param>
        /// <param name="src_row">Target pixel row index in source array.</param>
        /// <param name="src_col">Target pixel column index in source array.</param>
        /// <param name="m_size">Weighted samples matrix size.</param>
        /// <param name="weighted_samples">Matrix for storing the result.</param>
        /// <param name="dist_matrix">Matrix that represents distance weights.</param>
        /// <param name="brightness_weights">Brightness weights array.</param>
        /// <returns>Combined distance and brightness sum.</returns>
        private static double WeightSamples(Pixel_RGB8[][] src_data, int src_row, int src_col, int m_size, DPixel_RGB[][] weighted_samples, double[][] dist_matrix, double[] brightness_weights) {
            //Sum of combined weights for every sample pixel.
            //Will be used for averaging
            double cmb_weights_sum = 0.0;

            //Visiting every pixel around src[row][column] in radius of the matrix, retrieving brightness weight and applying combined with distance weight
            //Matrix radius (half matrix size rounded down)
            int radius = (m_size - 1) / 2;

            //ITERATING SAMPLES ROWS
            //------------------------------------------------
            for (int m_row = 0; m_row < m_size; m_row++) {
                //Sampled row. Index of row in source array that corresonds to smp_matrix_row of the matrix when target pixel is at the center of the matrix
                int smp_row = src_row - radius + m_row;
                if (smp_row < 0)
                    //If sample is above first row of the source image we sample row 0 
                    smp_row = 0;
                else
                    if (smp_row >= src_data.Length)
                    //If sample is below last row of source image we sample last row
                    smp_row = src_data.Length - 1;

                //ITERATING SAMPLES ROW PIXELS
                //---------------------------------------------------------------------------
                for (int m_col = 0; m_col < m_size; m_col++) {
                    //Index of column in source array that corresponds to m_col of the matrix when target pixel is at the center of the matrix
                    int smp_col = src_col - radius + m_col;

                    if (smp_col < 0)
                        //If sample is left to first pixel of the row we sample first pixel
                        smp_col = 0;
                    else
                        //If sample is right to last pixel of the samples row we sample last pixel of the row
                        if (smp_col >= src_data[0].Length)
                        smp_col = src_data[0].Length - 1;

                    double smp_brightness_weight;

                    if (src_data[src_row][src_col].Avg >= src_data[smp_row][smp_col].Avg)
                        //If sample is darker than target pixel we get sampe brightness weight from table
                        smp_brightness_weight = brightness_weights[src_data[smp_row][smp_col].Avg];
                    else
                        //If sample is brighter than target pixel we discard its luminance
                        smp_brightness_weight = 0.0;

                    //Combining brightness and distance weights
                    double combined_weight = dist_matrix[m_row][m_col] * smp_brightness_weight;

                    //Calculating resulting weighted brightness value
                    weighted_samples[m_row][m_col] = src_data[smp_row][smp_col].Scale(combined_weight);

                    //Saving combined weight for averaging
                    cmb_weights_sum += combined_weight;
                }//Matrix row pixels iterations end
            }//Matrix rows iterations end. -----------

            //Target pixel itself should not be weighted by its brightness
            //Restoring target pixel not weighted luminance
            weighted_samples[m_size / 2][m_size / 2] = src_data[src_row][src_col].Scale(dist_matrix[m_size / 2][m_size / 2]);
            //Replacing target combined weight by simply distance weight (it will be equal to 1 because distance matrix is normalized when created)
            cmb_weights_sum += (1 - brightness_weights[src_data[src_row][src_col].Avg]) * dist_matrix[m_size / 2][m_size / 2];

            return cmb_weights_sum;
        }



        /// <summary>
        /// Simulates DotGain for RGB8 image.
        /// </summary>
        /// <param name="source">Source image object.</param>
        /// <param name="strength">Effect strength.</param>
        /// <param name="spread">Effect spread.</param>
        /// <param name="scaling_factor">Scaling factor this effect is intended for.</param>
        /// <returns>Resulting image.</returns>
        public static RawImage_RGB8 SimulateDotGain(RawImage_RGB8 source, int strength, int spread, double scaling_factor) {
            //Resulting image object
            RawImage_RGB8 res_img = new RawImage_RGB8(source.Height, source.Width);

            //Min blur is 0.5/scaling_f
            //Max blur is 2/scaling_f
            //Spread step is 0.015
            double radius = (0.5 + spread * 0.015) / scaling_factor;

            //Creating distance weight matrix
            double[][] dist_matrix = CreateDistanceWeightsMatrix(radius);

            //Creating brightness weights array
            double[] brightness_weights = CreateBrightnessWeights(strength);

            //Processing rows in parallel
            Task[] row_tasks = new Task[source.Height];
            for (int src_row = 0; src_row < source.Height; src_row++) {
                //Setting row processing parameters
                ProcessRowParams p = new ProcessRowParams();
                p.src_img = source;
                p.src_row = src_row;
                p.res_img = res_img;
                p.dist_matrix = dist_matrix;
                p.brightness_weights = brightness_weights;

                //Starting row task
                row_tasks[src_row] = Task.Factory.StartNew(ProcessRow, p);
            }
            //Waiting all row tasks to finish
            Task.WaitAll(row_tasks);

            //Creating image object and returning the result
            return res_img;
        }
    }
}
