using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzRUtility
{
    public class Img3dData
    {
        public int Width { get; set; }

        public int Height { get; set; }

        public double XResolution { get; set; }

        public double YResolution { get; set; }

        public double ZResolution { get; set; }


        public double XOffset { get; set; }

        public double YOffset { get; set; }

        public double ZMin { get; set; }

        public double ZMax { get; set; }


        public float[] ZData { get; set; }

        public byte[] IntensityData { get; set; }

        public byte[] ScatterData { get; set; }
    }
}
