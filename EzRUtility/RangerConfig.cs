using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzRUtility
{
    public class RangerConfig
    {
        public string IPAddress { get; set; } = "192.168.0.66";


        public string CameraID { get; set; } = "Camera1";

        public string CameraName { get; set; } = "Ranger3";

        public string ParamCSVFile { get; set; }

        public string CalibrationFile { get; set; }

        public string DATFile { get; set; }

        public string EvarFile { get; set; }

        public int ExtractReg_OffsetY { get; set; } = 0;

        public int ExtractReg_Height { get; set; } = 0;

        public int Exposure { get; set; } = 100;

        public int Threshold { get; set; } = 20;

        public int ScanHeight { get; set; } = 1024;

        public bool Img3DFormat_ReflectanceOn { get; set; } = true;

        public bool Img3DFormat_ScatterOn { get; set; } = true;

        public bool ProfileTrig_EncoderOn { get; set; } = true;

        public double  ProfileTrig_EncoderResolution { get; set; } = 1.0;

        public int ProfileTrig_EncoderPulseDivider { get; set; } = 1;

        public bool ImgTrig_On { get; set; } = true;

        public bool ImgTrig_RisingEdge { get; set; } = true;

        public bool Calib_OutputCalibImg { get; set; } = true;

        public int Calib_RectificationWidth { get; set; } = 2560;

        public int Calib_RectificationMethod { get; set; } = 0;

        public double Calib_RectificationSpread { get; set; } = 1.2;

        public bool Calib_LockRectificationWidth { get; set; } = false;

        public double YResolution { get; set; } = 1.0;

    }
}
