using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaScaler.Model {
    /// <summary>
    /// Helper class that represents bounds of group of pixels 
    /// in a source row or column that correspond to one scaled pixel.
    /// Can be advanced trought source array.
    /// </summary>
    public class PixelBin {
        /// <summary>
        /// Index of resulting pixel that this pixel group corresponds to.
        /// </summary>
        public int Target { get; private set; }

        /// <summary>
        /// Weight (0 to 1) of last pixel in the group.
        /// </summary>
        public double Margin { get; private set; }
        /// <summary>
        /// Weight (0 to 1) of first pixel in the next group.
        /// </summary>
        public double MarginNext { get; private set; }

        /// <summary>
        /// Linear size of the bin in pixels (may be not integer).
        /// </summary>
        public double Size { get; private set; }

        /// <summary>
        /// Position of the opening boundary in source array.
        /// May be not integer.
        /// </summary>
        public double Pos { get; private set; }

        /// <summary>
        /// Index of the last pixel of the bin in source array.
        /// </summary>
        public int Last { get; private set; }

        /// <summary>
        /// Creates instance of pixel bin set to the beginning of the source array.
        /// </summary>
        /// <param name="src_size">Size of source array in pixels.</param>
        /// <param name="trg_size">Size of resulting array in pixels.</param>
        public PixelBin(int src_size, int trg_size) {
            //Calculating size of the bin
            Size = (double)src_size / (double)trg_size;
            //Setting bin to the beginning.
            Reset();
        }

        /// <summary>
        /// Returns true if pixel with provided index is the last one in this bin.
        /// </summary>
        /// <param name="src_index"></param>
        /// <returns></returns>
        public bool OnMargin(int src_index) {
            if (src_index == Last)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Advances this bin by one step trough source array.
        /// </summary>
        public void Advance() {
            Target++;
            Pos = Size * Target;
            double pos_next = Size * (Target + 1);

            //Fixed precise integers problem with rounding, because we need only 3-4 digits after dot anyway
            Margin = Math.Round(pos_next, 4) % 1;
            if (Margin == 0.0) {
                Margin = 1.0;
                MarginNext = 0.0;
            }
            else
                MarginNext = 1 - Margin;

            Last = (int)(Math.Ceiling(Math.Round(Size * (Target + 1), 4))) - 1;
        }

        /// <summary>
        /// Sets this bin to the beginning of the source array.
        /// </summary>
        public void Reset() {
            Target = 0;
            Pos = 0;

            Margin = Math.Round(Size, 4) % 1;
            if (Margin == 0.0) {
                Margin = 1.0;
                MarginNext = 0.0;
            }
            else
                MarginNext = 1 - Margin;

            Last = (int)Math.Ceiling(Math.Round(Size, 4)) - 1;
        }
    }
}
