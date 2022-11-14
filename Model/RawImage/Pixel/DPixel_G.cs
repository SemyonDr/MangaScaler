using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaScaler.Model {
    internal class DPixel_G {
        //------------------------------------------
        //PUBLIC FIELDS
        //------------------------------------------
        public double Gr;

        //------------------------------------------
        //CONSTRUCTORS
        //------------------------------------------

        /// <summary>
        /// Default constructor with default values.
        /// </summary>
        public DPixel_G() {

        }

        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        /// <param name="r">Red component.</param>
        /// <param name="g">Green component.</param>
        /// <param name="b">Blue component.</param>
        public DPixel_G(double gr) {
            Gr = gr;
        }

        //------------------------------------------
        //PUBLIC METHODS
        //------------------------------------------
        /// <summary>
        /// Returns new DPixel with each component multiplied by scaling factor.
        /// </summary>
        /// <param name="scaling_factor">Scaling factor.</param>
        /// <returns>Resulting pixel.</returns>
        public DPixel_G Scale(double scaling_factor) {
            return new DPixel_G(Gr * scaling_factor);
        }

        public Pixel_G8 ToPixel_G8() {
            return new Pixel_G8((byte)Math.Round(Gr));
        }

        //------------------------------------------
        //OPERATORS
        //------------------------------------------
        public static DPixel_G operator +(DPixel_G a, DPixel_G b) {
            return new DPixel_G(a.Gr + b.Gr);
        }
    }
}
