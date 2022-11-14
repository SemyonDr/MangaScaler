using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaScaler.Model {
    internal class Pixel_G8 {

        //------------------------------------------
        //PUBLIC FIELDS
        //------------------------------------------

        /// <summary>
        /// Color components 8 bit per component.
        /// </summary>
        public byte Gr;

        //------------------------------------------
        //PUBLIC CONSTRUCTOR
        //------------------------------------------

        /// <summary>
        /// Default constructor with default values.
        /// </summary>
        public Pixel_G8() {

        }

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="r">Red component.</param>
        /// <param name="g">Green component.</param>
        /// <param name="b">Blue component.</param>
        public Pixel_G8(byte gr) {
            Gr = gr;
        }



        //------------------------------------------
        //PUBLIC METHODS
        //------------------------------------------

        /// <summary>
        /// Converts this pixel to double pixel container.
        /// </summary>
        /// <returns></returns>
        public DPixel_G ToDouble() {
            return new DPixel_G((double)Gr);
        }

        /// <summary>
        /// Multiplies every component by scaling factor and returns result as double pixel.
        /// </summary>
        /// <param name="scale_factor">Scaling factor.</param>
        /// <returns>Double pixel object that contains result of multiplication.</returns>
        public DPixel_G Scale(double scaling_factor) {
            return new DPixel_G(Gr * scaling_factor);
        }
    }
}
