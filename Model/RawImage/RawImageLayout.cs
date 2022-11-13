using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaScaler.Model {
    /// <summary>
    /// Order of pixel components and bit depth of every component.
    /// </summary>
    internal enum RawImageLayout {
        G8,
        G16,
        GA8,
        GA16,
        RGB8,
        RGB16,
        RGBA8,
        RGBA16
    }
}
