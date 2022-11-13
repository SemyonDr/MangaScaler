using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaScaler.Model {

    /// <summary>
    /// Represent pixel of RGB 8 bit per component image.
    /// </summary>
    internal class Pixel_RGB8 : Pixel {
        //------------------------------------------
        //PUBLIC FIELDS
        //------------------------------------------

        /// <summary>
        /// Color components 8 bit per component.
        /// </summary>
        public byte R;
        public byte G;
        public byte B;

        //------------------------------------------
        //PUBLIC PROPERTIES
        //------------------------------------------

        /// <summary>
        /// Average brightness across 3 components.
        /// </summary>
        public byte Avg {
            get { return (byte)((int)(R + G + B) / 3); }
            private set { }
        }

        //------------------------------------------
        //PUBLIC CONSTRUCTOR
        //------------------------------------------

        /// <summary>
        /// Default constructor with default values.
        /// </summary>
        public Pixel_RGB8() {

        }

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="r">Red component.</param>
        /// <param name="g">Green component.</param>
        /// <param name="b">Blue component.</param>
        public Pixel_RGB8(byte r, byte g, byte b) { 
            R = r;
            G = g;
            B = b;
        }



        //------------------------------------------
        //PUBLIC METHODS
        //------------------------------------------

        /// <summary>
        /// Converts this pixel to double pixel container.
        /// </summary>
        /// <returns></returns>
        public DPixel_RGB ToDouble() {
            DPixel_RGB pd = new DPixel_RGB();
            pd.R = (double)R;
            pd.G = (double)G;
            pd.B = (double)B;

            return pd;
        }

        /// <summary>
        /// Multiplies every component by scaling factor and returns result as double pixel.
        /// </summary>
        /// <param name="scale_factor">Scaling factor.</param>
        /// <returns>Double pixel object that contains result of multiplication.</returns>
        public DPixel_RGB Scale(double scaling_factor) {
            return new DPixel_RGB(R * scaling_factor, G * scaling_factor, B * scaling_factor);
        }
    }//Class end

}
