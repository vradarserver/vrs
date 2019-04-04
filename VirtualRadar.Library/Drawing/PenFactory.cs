using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using VrsDrawing = VirtualRadar.Interface.Drawing;

namespace VirtualRadar.Library.Drawing
{
    /// <summary>
    /// The singleton ImageSharp implementation of <see cref="Interface.Drawing.IPenFactory"/>.
    /// </summary>
    class PenFactory : VrsDrawing.IPenFactory
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        public VrsDrawing.IPen Black { get; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public VrsDrawing.IPen LightGray { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PenFactory()
        {
            Black =     new PenWrapper(new Pen<Rgba32>(new Rgba32(0,    0,      0),     1.0F));
            LightGray = new PenWrapper(new Pen<Rgba32>(new Rgba32(211,  211,    211),   1.0F));
        }
    }
}
