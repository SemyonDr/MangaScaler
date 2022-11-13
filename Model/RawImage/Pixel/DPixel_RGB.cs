using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaScaler.Model {

    /// <summary>
    /// Double pixel with RGB layout.
    /// </summary>
    internal class DPixel_RGB : DPixel {
        //------------------------------------------
        //PUBLIC FIELDS
        //------------------------------------------
        public double R;
        public double G;
        public double B;

        //------------------------------------------
        //PUBLIC PROPERTIES
        //------------------------------------------
        public double Avg {
            get { return (R + G + B) / 3.0; }
            private set { }
        }

        //------------------------------------------
        //CONSTRUCTORS
        //------------------------------------------

        /// <summary>
        /// Default constructor with default values.
        /// </summary>
        public DPixel_RGB() { 
        
        }

        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        /// <param name="r">Red component.</param>
        /// <param name="g">Green component.</param>
        /// <param name="b">Blue component.</param>
        public DPixel_RGB(double r, double g, double b) { 
            R = r;
            G = g;
            B = b;
        }

        //------------------------------------------
        //PUBLIC METHODS
        //------------------------------------------
        /// <summary>
        /// Returns new DPixel with each component multiplied by scaling factor.
        /// </summary>
        /// <param name="scaling_factor">Scaling factor.</param>
        /// <returns>Resulting pixel.</returns>
        public DPixel_RGB Scale(double scaling_factor) {
            return new DPixel_RGB(R * scaling_factor, G * scaling_factor, B * scaling_factor);
        }

        public Pixel_RGB8 ToPixel_RGB8() { 
            return new Pixel_RGB8((byte)Math.Round(R), (byte)Math.Round(G), (byte)Math.Round(B));
        }

        //------------------------------------------
        //OPERATORS
        //------------------------------------------
        public static DPixel_RGB operator +(DPixel_RGB a, DPixel_RGB b) {
            return new DPixel_RGB(a.R + b.R, a.G + b.G, a.B + b.B);
        }
    }
}
