using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ScreenVersusWpf
{
    /// <summary>
    /// Class containing dpi and scale information related to a specific source. 
    /// A Dpi can be retrieved from the entire system, a specific monitor, or even a window.
    /// </summary>
    public class Dpi : IEquatable<Dpi>
    {
        /// <summary>
        /// Provides access to a default DPI instance that is always 96-96
        /// </summary>
        public readonly static Dpi Default = new Dpi(96, 96);

        /// <summary>
        /// The horizontal DPI
        /// </summary>
        public int DpiX { get; }

        /// <summary>
        /// The vertical DPI
        /// </summary>
        public int DpiY { get; }

        /// <summary>
        /// The horizontal scale. This is a ratio of the current DPI over 96.
        /// </summary>
        public double ScaleX => DpiX / 96d;

        /// <summary>
        /// The vertical scale. This is a ratio of the current DPI over 96.
        /// </summary>
        public double ScaleY => DpiY / 96d;

        
        internal Dpi(int dpiX, int dpiY)
        {
            DpiX = dpiX;
            DpiY = dpiY;
        }

        public bool Equals(Dpi other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return DpiX == other.DpiX && DpiY == other.DpiY;
        }
    }
}
