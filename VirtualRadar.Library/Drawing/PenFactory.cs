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

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="alpha"></param>
        /// <param name="strokeWidth"></param>
        /// <returns></returns>
        public VrsDrawing.IPen CreatePen(int red, int green, int blue, int alpha, float strokeWidth)
        {
            return new PenWrapper(
                new Pen<Rgba32>(
                    new Rgba32(red, green, blue, alpha),
                    strokeWidth
                )
            );
        }
    }
}
