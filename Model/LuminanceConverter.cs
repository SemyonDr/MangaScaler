using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaScaler.Model {
    /// <summary>
    /// Converts numerical values of RGB pixel components to normalized luminance and back using sRGB standart electro-optical transfer function.
    /// Uses pre-calculated table. Shold be used if Bit Depth of pixel is 16, or less.
    /// </summary>
    /// <remarks>
    /// This class uses pre-calculated table to convert between
    /// numerical value of RGB component and percieved luminance for bit depth 16 bit or less.
    /// sRGB electro-optical transfer function is used for converting numerical value to luminance.
    /// Luminance value is normalized [0.0-1.0].
    /// </remarks>
    internal class LuminanceConverter {
        /* Table used for conversion.
         * If f(x) is a conversion function between numerical value and luminance,
         * then for different BitDepth:
         * table[i] = f(i).
         * Table is constructed on converter initialization. */
        double[] table;

        //Bit depth of pixel components this converter is for.
        public byte cmpBitDepth { get; private set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="cmpBitDepth">bitDepth of pixel component this converter is for.</param>
        public LuminanceConverter(byte cmpBitDepth) {
            //Saving bit depth parameter
            this.cmpBitDepth = cmpBitDepth;

            //Building table
            BuildTable(cmpBitDepth);
        }

        /// <summary>
        /// Builds conversion table.
        /// </summary>
        /// <remarks>
        /// Formula used for conversion is
        /// sRGB electro-optical transfer function, defined as follows:
        /// We denote x-bit numerical value of RGB component as n=[0..2^x-1].
        /// We define Signal as (n / 2^x-1). (normalized numerical value).
        /// Then the formula is
        /// Luminance = | Signal/12.92                    if Signal <= 0.04045
        ///             | (Signal+0.055/(1+0.055))^2.4    if Signal >  0.04045
        /// </remarks>
        /// <param name="cmpBitDepth">Bit depth of pixel component.</param>
        private void BuildTable(byte cmpBitDepth) {
            //Number of different values that can be stored in variable with given bit depth
            //2^BitDepth
            int bitWidth = 1 << cmpBitDepth;

            //Creating conversion table of size 2^bitDepth
            table = new double[bitWidth];

            //First we define first numerical value that produces signal bigger than 0.04045
            // t = LowerBound(0.04045*(2^bitDepth-1))+1
            int treshold = (int)(0.04045f * (bitWidth - 1)) + 1;

            //For numerical values lower that treshold signal is less than 0.04045.
            //We use formula Luminance = n*c where c = 1/(12.92*(bitWidth-1)) (for example for bd=8 c = 1/(12.92*255))
            double c = 1 / (12.92 * (bitWidth - 1) - 1);
            for (int n = 0; n < treshold; n++)
                this.table[n] = n * c;

            //For other numerical values we use formula:
            //Luminance = ((n+c1)*c2)^2.4 where
            //  c1 = 0.055*(2^bitDepth-1)
            //  c2 = 1/((2^bitDepth-1)*1.055)
            double c1 = 0.055 * (bitWidth - 1);
            double c2 = 1 / ((bitWidth - 1) * 1.055);

            for (int n = treshold; n < bitWidth; n++)
                this.table[n] = Math.Pow((n + c1) * c2, 2.4);
        }



        /// <summary>
        /// Converts pixel component numerical value to normalized luminance value [0.0..1.0].
        /// </summary>
        /// <param name="cmp_value">Component numerical value.</param>
        /// <returns>Normalized luminance for this component value.</returns>
        /// <remarks>
        /// Uses component value to retrieve data from the table directly.
        /// </remarks>
        public double ComponentToLuminance(int cmpValue) {
            //We use conversion table directly to retrieve value
            return table[cmpValue];
        }


        /// <summary>
        /// Converts luminance value of pixel component to numerical value.
        /// </summary>
        /// <param name="luminance"></param>
        /// <returns>Numerical value byte array in Big Endian byte order.</returns>
        public int LuminanceToComponent(double luminance) {
            //We use binary search to retrieve closest numerical value as integer (rounded to closest)
            return NumericalSearchRecursive(luminance, 0, table.Length - 1); ;
        }


        /// <summary>
        /// Recursively searches for nearest index in the conversion sub-table that contains luminance value closest to given luminance.
        /// </summary>
        /// <param name="luminance">Luminance value to convert.</param>
        /// <param name="left">First index of sub-array.</param>
        /// <param name="right">Last index of sub-array.</param>
        /// <returns></returns>
        private int NumericalSearchRecursive(double luminance, int left, int right) {

            int mid = left + (right - left + 1) / 2 - 1; //Top index of left half. If there is 16 elements from 0 to 15 it will be 7.

            //Recursion stops when there is only two elements
            if (right == left + 1) {
                //Determining what direction to round to
                //This is decided by comparing deltas between given luminance and left and right values
                //[left_value] <-delta_left-> [luminance] <-delta_right-> [right_value]
                //Bias is for the left side
                double delta_left = luminance - table[left];
                double delta_rigth = table[right] - luminance;
                if (delta_left <= delta_rigth)
                    return left;
                else
                    return right;
            }

            //If there is more than two elements
            //we continue recursive calls
            if (luminance <= table[mid])
                //Choosing left side
                return NumericalSearchRecursive(luminance, left, mid);
            else
                //Choosing right side
                return NumericalSearchRecursive(luminance, mid + 1, right);
        }
    }
}
