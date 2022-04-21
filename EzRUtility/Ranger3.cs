using Sick.EasyRanger;
using Sick.GenIStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzRUtility
{
    public class Ranger3: IGrabber3D
    {

        public RangerConfig Config { get { return _config; } set { _config = value; } }
        public Frame FrameData { get { return _frameData; } set { _frameData = value; } }

        ProcessingEnvironment env;

        RangerConfig _config = new RangerConfig();

        Frame _frameData = Frame.Create();

        public void Set()
        {

        }



        public bool Connect()
        {

            //env.Cameras[0].Connect();

            //env.Cameras[0].OutputMode = OutputType.CalibratedAndRectified;
            //env.Cameras[0].RectificationWidth = _config.RectificationWidth;
            //env.Cameras[0].YResolution = Convert.ToSingle(_config.YResolution);

            return true;

        }

        public bool Disconnect()
        {

            //env.Cameras[0].Disconnect();
            return true;
        }

        public IFrame Grab(double yresolution, int timeout = 100000, bool Online = true)

        {

            string bankid = "image";

            var task = Task.Factory.StartNew(() =>
            {
                env.Cameras[0].Grab(bankid, timeout);


            });

            Task.WaitAll(new Task[] { task });

            //var buf = env.GetImageBank(bankid);

            //_frameData.Width = buf.Info.Width;

            //_frameData.Height = buf.Info.Height;

            //_frameData.ZMin = buf.Info.RangeMin;

            //_frameData.ZMax = buf.Info.RangeMax;

            //_frameData.XResolution = buf.Info.XResolution;

            //_frameData.YResolution = buf.Info.YResolution;

            //_frameData.ZResolution = 1.0;

            //_frameData.ZData = buf.range;

            //_frameData.IntensityData = buf.intensity;

            return _frameData;


        }

        public bool Init()
        {

            if (env == null)
            {
                env = new ProcessingEnvironment();

            }


            //foreach (var item in env.Cameras)
            //{
            //    item.Disconnect();
            //}

            //env.Cameras.Clear();


            //ICameraDevice cam = env.CreateCameraDevice("Camera1", new Sick.EasyRanger.Basic.CameraId(_config.CameraID), Sick.EasyRanger.Base.CameraType.Ranger3);



            //cam.IP = _config.IPAddress;



            //cam.ConfigurationFile = _config.ParamCSVFile;

            //cam.CalibrationFile = _config.CalibrationFile;


            return true;
        }

        public bool LoadParams(RangerConfig _config)
        {
            return true;
        }

        public bool SetParams(RangerConfig _config)
        {
            return true;
        }

        public bool Start()
        {
            //env.Cameras[0].Start();

            return true;
        }

        public bool Stop()
        {
            //env.Cameras[0].Stop();

            return true;
        }

    }
}
