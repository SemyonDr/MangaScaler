using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaScaler.Model {
    internal class DotGain {


        /// <summary>
        /// Uses distance weights array to create distance weights 2d matrix.
        /// </summary>
        /// <param name="dist_weights">Distance weights array.</param>
        /// <returns>Distance weights matrix.</returns>
        internal static double[][] CreateDistanceWeightsMatrix(double rad) {
            double[] dist_weights = GFunc.CreateDistanceWeights(rad);

            int size = dist_weights.Length * 2 - 1;
            double[][] matrix = new double[size][];

            //Full row of distance weights for radius
            double[] weights_full = new double[size];
            //Copying central value
            weights_full[dist_weights.Length - 1] = dist_weights[0];
            //Copying weights to the right half and in reverse order to the left half
            for (int w = 1; w < dist_weights.Length; w++) {
                weights_full[dist_weights.Length - 1 - w] = dist_weights[w];
                weights_full[dist_weights.Length - 1 + w] = dist_weights[w];
            }

            //Initializing rows of the matrix
            for (int row = 0; row < size; row++) {
                matrix[row] = new double[size];
                for (int col = 0; col < size; col++)
                    matrix[row][col] = weights_full[col];
            }

            //Sum of all weights in the matrix. Used for normalization.
            double sum = 0.0;

            //Applying weights again to the columns of the matrix
            //and keeping sum of every matrix element at the same time
            for (int col = 0; col < size; col++) {
                for (int row = 0; row < size; row++) {
                    matrix[row][col] *= weights_full[row];
                    sum += matrix[row][col];
                }
            }

            //Normalizing matrix values so weight of central value is 1
            double norm_factor = 1 / matrix[size / 2][size / 2];
            for (int row = 0; row < size; row++)
                for (int col = 0; col < size; col++)
                    matrix[row][col] *= norm_factor;

            //Returning the result
            return matrix;
        }




        /// <summary>
        /// This method assigns weight for 256 (0..255) brightness value steps using gaussian function.
        /// In another words brightness weights are pre-calculated and stored in an array for fast retrieval.
        /// </summary>
        /// <param name="param">DotGain Strength parameter. With increase in value brighter and brighter pixels will be affected. Default recommended value is 50.</param>
        /// <returns>Array of 256 weights for brightness.</returns>
        internal static double[] CreateBrightnessWeights(int param) {
            //We set sigma from 0.25 up to 1.1
            //And we scale brighness values [0..255]
            //to lay in [0.0..5.1] by dividing them by 5 (or multiplying by 0.2)
            //At this scale if sigma is 0.25 gauss function value drops off around 1, so blur will affect only pixels with brightness [0..50-55]
            //If sigma is 1.1 gauss function drops off aroung 4 so blur will affect brightness values up to 200 with more gentle slope.
            //Param is step of distance between 0.25 and 1.1. Every 1 step of param is 0.0085 increase of sigma,
            //Therefore sigma value is 0.25+param*0.0085
            //Default value for param is 50, therefore default value for sigma is 0.675 with dropoff at 2.5, or brightness 127
            double sigma = 0.25 + param * 0.085;

            //Luminance weights array - result
            double[] lw = new double[256];

            double sum = 0;
            double total = 0;
            double first = GFunc.GaussianFunction(0.0, sigma); ;

            for (int i = 0; i < 256; i++) {
                //Taking 4 samples for each stop
                sum = first; //GF(i,s) is already calculated on previous step
                sum += GFunc.GaussianFunction(((double)i + 0.25) * 0.2, sigma);
                sum += GFunc.GaussianFunction(((double)i + 0.75) * 0.2, sigma);
                first = GFunc.GaussianFunction(((double)i + 1.0) * 0.2, sigma); //Updating for next step
                sum += first;

                //Saving average
                lw[i] = sum / 4;

                total += lw[i];
            }

            //Gaussian function values are normalized such that value for brightness 0 is 1
            double normalization_factor = 1 / lw[0];
            for (int i = 0; i < 256; i++) {
                lw[i] = lw[i] * normalization_factor;
            }

            return lw;
        }
    }
}
