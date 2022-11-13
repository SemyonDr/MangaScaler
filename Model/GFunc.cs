using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaScaler.Model {

    /// <summary>
    /// This class represents methods for getting values of Gaussian function.
    /// </summary>
    internal class GFunc {
        /// <summary>
        /// Returns value of Gaussian Function with given sigma at position x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="sigma"></param>
        /// <returns>Function value.</returns>
        public static double GaussianFunction(double x, double sigma) {
            double power = -(x * x) / (2 * sigma * sigma);
            double divisor = 2.506628274631000502415765284811; // Sqrt(2*PI)
            return Math.Pow(Math.E, power) / (sigma * divisor);
        }

        /// <summary>
        /// Selects appropriate sigma and uses Gaussian function to create array of weights for given radius.
        /// </summary>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static double[] CreateDistanceWeights(double radius) {
            //We calculate sigma such as value of gauss function at x=rad will be "cutoff" times smaller than value at x=0
            //Formula for sigma comes from solving for sigma f(0,s) * 1/cutoff = f(r,s)
            //sigma = r/sqrt(2ln(cutoff)).
            //We choose "cutoff" value depending on radius, because shape of a gauss curve depends on this.
            //Empirically we establish 5 sections of radius values with different last (cutoff) values.
            //We define cutoff by exponent: last=e^p for each segment

            double sigma;
            double cutoff;

            //Calculating cutoff fraction (sigma will have value such that at f(rad) = f(0)/cutoff (equality is not strict))
            if (0.1 <= radius && radius <= 1.0)
                cutoff = Math.Exp(10);
            else {
                if (1 < radius && radius <= 2.5)
                    cutoff = Math.Exp(8);
                else {
                    if (2.5 < radius && radius <= 5)
                        cutoff = Math.Exp(6.5);
                    else {
                        if (5 < radius && radius <= 8)
                            cutoff = Math.Exp(6);
                        else
                            //For all radius bigger than 8 we chose cutoff to be 255.
                            cutoff = 255;
                    }
                }
            }

            //Calculating sigma value
            sigma = radius / (Math.Sqrt(2 * Math.Log(cutoff)));

            //Creating weights array for given radius
            int weights_len = (int)Math.Ceiling(radius) + 1;
            double[] weights = new double[weights_len];

            //Now we find average value of gaussian function for each pixel of the radius.
            //For pixel with index of i we should find average value of the function within x=[i..i+1].
            //We find average value approximately by taking samples of the function and averaging them arithmetically.
            //For example we sampled pixel i five times i.e. found five values of f(x) when x=[i..i+1], then average is (f(x1)+f(x2)+..+f(x5))/5

            //Number of samples of gaussian function for every element (pixel) of weights array
            int[] num_samples = new int[weights_len];

            //Initializing arrays
            for (int i = 0; i < weights_len; i++) {
                weights[i] = 0.0;
                num_samples[i] = 0;
            }

            //Now we should decide at which points (x values) we take function samples.
            //We store their list in this array.
            double[] sampling_points;

            //Simple algorithm is used for deciding points to sample. Algorithm guarantees that every function will be sampled at least once for every pixel.
            if (radius > 50)
                //If radius is big enough we sample gaussian function once for every pixel. 
                //With big radius sigma is also chosen to be big, so slope of the function is relatively flat and one sample per pixel is enough.
                sampling_points = new double[weights_len];
            else
                //If radius is smaller than 50 we perform 51 samples for every radius (50+point 0).
                //For small radius sigma is smaller, and slope is steeper, but it is kind of shrinked version of function with big sigma, so with the same
                //sampling rate we should get similar accuracy.
                sampling_points = new double[50 + 1];

            //We choose sampling points to be with fixed step between one another.
            //Improvement of this algorithm might be made by sampling more at the beginning and the end of a function since in the middle
            //it is almost linear and can be well approximated with fewer samples.
            double sampling_step = ((double)weights_len) / (sampling_points.Length - 1); //Dividing radius to equal steps
            for (int i = 0; i < sampling_points.Length; i++) {
                sampling_points[i] = sampling_step * i;
            }

            //Taking samples
            for (int i = 0; i < sampling_points.Length; i++) {
                //Target pixel of the radius
                int target;

                if (Math.Round(sampling_points[i], 4) % 1 == 0.0 && i != 0)
                    target = (int)sampling_points[i] - 1;
                else
                    target = (int)sampling_points[i];

                //Accumulating sample
                weights[target] += GaussianFunction(sampling_points[i], sigma); ;
                //Counting sample for target pixel
                num_samples[target]++;
            }

            //Taking average value of the samples for every pixel
            for (int i = 0; i < weights_len; i++)
                weights[i] = weights[i] / (double)num_samples[i];

            //Returning the result
            return weights;
        }//Created distance weights end



        /// <summary>
        /// Selects appropriate sigma and uses Gaussian function to create array of weights for given radius.
        /// Values are normalized so sum of weights in 1D convolution matrix is 1;
        /// </summary>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static double[] CreateDistanceWeights_Normalized(double radius) {
            //Creating not normalized weights
            double[] weights = CreateDistanceWeights(radius);

            //Normalizing weights so their sum across 1D convolution matrix is 1.0
            //We find sum of the values
            double weights_sum = weights[0];
            for (int i = 1; i < weights.Length; i++)
                weights_sum += 2 * weights[i];

            //Normalizing
            double norm_factor = 1 / weights_sum;
            for (int i = 0; i < weights.Length; i++)
                weights[i] *= norm_factor;

            return weights;
        }


    }
}
