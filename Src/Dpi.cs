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
        public readonly static Dpi Default = new Dpi(96, 96);

        public int DpiX { get; }
        public int DpiY { get; }

        public double ScaleX => DpiX / 96d;
        public double ScaleY => DpiY / 96d;

        public ScaleTransform UpScaleTransform => new ScaleTransform(ScaleX, ScaleY);
        public ScaleTransform DownScaleTransform => new ScaleTransform(1d / ScaleX, 1d / ScaleY);

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
