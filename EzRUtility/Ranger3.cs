using Sick.EasyRanger;
using Sick.EasyRanger.Base;
using Sick.GenIStream;
using Sick.StreamUI.ImageFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using IFrame = Sick.GenIStream.IFrame;

namespace EzRUtility
{
    public class Ranger3: IGrabber3D
    {
        public event ImageCallback ImgReceived;

        public delegate void ImageCallback(Sick.EasyRanger.Base.IFrame frame);

        public event DisconnectCallback CameraDisconnected;

        public delegate void DisconnectCallback(string CameraID);

        public RangerConfig Config { get { return _config; } set { _config = value; } }
        public Frame FrameData { get { return _frameData; } set { _frameData = value; } }

        ProcessingEnvironment env;

        private  CameraDiscovery _discovery;
        public ICamera Camera { get; private set; }

        public FrameGrabber Grabber { get; set; }

        RangerConfig _config = new RangerConfig();

        Frame _frameData = Frame.Create();

        public Ranger3()
        {
            //Init();
        }


        public bool Init()
        {
            try
            {
                var path = Environment.GetEnvironmentVariable("SICK_EASYRANGER_ROOT");
                _discovery = CameraDiscovery.CreateFromProducerFile($@"{path}\SICKGigEVisionTL.cti");
                if (env == null)
                {
                    env = new ProcessingEnvironment();
                    IStepProgram sp = env.CreateStepProgram("Filter Image");
                    var filterStep = sp.GetStep(sp.CreateStep("Image Filtering", "Filter"));
                    filterStep.SetArgument("Source Image", "Image");
                    filterStep.SetArgument("Size X", "5.0");
                    filterStep.SetResult("Destination Image", "FilteredImage");
                }
                var discoveredCameras = _discovery.ScanForCameras();
                if (discoveredCameras.Count > 0)
                {
                    Camera = _discovery.ConnectTo(IPAddress.Parse(_config.IPAddress));

                    if((Camera==null)|| (!Camera.IsConnected))
                    {
                        return false;
                    }
                    else
                    {
                        Grabber?.Dispose();
                        Grabber = Camera?.CreateFrameGrabber();

                        //ConfigurationResult cfgResult = Camera.ImportParametersFromCsvData(_config.ParamCSVFile);
                        return true;
                    }
                }
                else
                {
                    return discoveredCameras.Count == 0 ? false : true;
                }
            }
            catch (Exception ee)
            {
                return false;
            }
        }

        public bool Connect()
        {
            try
            {
                if (!Camera.IsConnected)
                {
                    Camera = _discovery.ConnectTo(IPAddress.Parse(_config.IPAddress));
                    Grabber?.Dispose();
                    Grabber = Camera?.CreateFrameGrabber();
                    //Grabber.FrameReceived += GrabberOnFrameReceived;
                    //Camera.Disconnected += RangerDisconnected;

                }
                else
                {
                    Camera.Disconnect();
                    Camera = _discovery.ConnectTo(IPAddress.Parse(_config.IPAddress));
                    Grabber?.Dispose();
                    Grabber = Camera?.CreateFrameGrabber();
                    //Grabber.FrameReceived += GrabberOnFrameReceived;
                    //Camera.Disconnected += RangerDisconnected;
                }

                return Camera.IsConnected;
            }
            catch (Exception)
            {

                return false;
            }

        }

        public bool Start()
        {
            try
            {
                if (Grabber != null)
                {
                    Grabber.Start();
                    // Register to FrameReceived  event
                    Grabber.FrameReceived += GrabberOnFrameReceived;
                    Camera.Disconnected += RangerDisconnected;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }

        public bool Stop()
        {
            if (Grabber != null)
            {
                Grabber.Stop();
                //Grabber.FrameReceived -= GrabberOnFrameReceived;
                //Camera.Disconnected -= RangerDisconnected;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Disconnect()
        {

            Stop();
            Camera?.Disconnect();
            Camera?.Dispose();
            Camera = null;
            return true;
        }

        public Sick.EasyRanger.Base.IFrame Grab(bool Online = true)
        {
            //ImageBuffer imgBuffer = env.GetImageBuffer("Image");
            //env.SetFrame(string Name, IFrame image)
            //Frame frame2 = ToGenIStreamFrameConverter.ToGenIStreamFrame(env.GetFrame("Image"), inFrame);
            try
            {
                if (Online)
                {
                    if (env.ImageAvailable("Image"))
                    {
                        return env.GetFrame("Image");
                    }
                    return null;
                }
                else
                {
                    env.LoadImageFile(_config.DATFile, "Image", (float)_config.YResolution);
                    return env.GetFrame("Image");
                }
            }
            catch (Exception)
            {
                return null;
            }

            //For Trispector
            //var task = Task.Factory.StartNew(() =>
            //{
            //    env.Cameras[0].Grab(bankid, timeout);

            //});

            //Task.WaitAll(new Task[] { task });

            //var buf = env.GetImageBank(bankid);

        }


        public bool LoadParams(RangerConfig _config)
        {
            ConfigurationResult cfgResult = Camera.ImportParametersFromCsvData(_config.ParamCSVFile);
            return true;
        }

        public bool SetParams(RangerConfig _config)
        {
            //Camera.ExportParametersToFile(string filePath);
            //RegionParameters regionParameters = Camera.GetCameraParameters().Region(RegionId.REGION_0);
            //RegionParameters regionParameters2 = Camera.GetCameraParameters().Region(RegionId.REGION_1);
            //Camera.GetCameraParameters().Region.ExposureTime.Set();
            return true;
        }

        public bool SetExposure(int Exp)
        {
            try
            {
                RegionParameters regionParameters1 = Camera.GetCameraParameters().Region(RegionId.REGION_0);
                RegionParameters regionParameters2 = Camera.GetCameraParameters().Region(RegionId.REGION_1);
                regionParameters1.ExposureTime.Set((float)Exp);
                regionParameters2.ExposureTime.Set((float)Exp);
                return true;
            }
            catch (Exception)
            {
                return false;

            }

        }

        public bool SetScanHeight(int ScanHeight)
        {
            try
            {
                RegionParameters regionParameters1 = Camera.GetCameraParameters().Region(RegionId.SCAN_3D_EXTRACTION_1);
                regionParameters1.Height.Set((uint)ScanHeight);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public int GetExposure()
        {
            return (int)(Camera.GetCameraParameters().Region(RegionId.REGION_1)).ExposureTime.Get();
        }

        public int GetScanHeight()
        {
            return (int)(Camera.GetCameraParameters().Region(RegionId.SCAN_3D_EXTRACTION_1)).Height.Get();
        }

        private void GrabberOnFrameReceived(IFrame frame)
        {
            if ((!frame.IsIncomplete())&& (null != ImgReceived))
            {
                FromGenIStreamFrameConverter.AddFrameToEnvironment(frame.Copy(), "Image", env);
                
                ImgReceived(FromGenIStreamFrameConverter.ToBaseFrame(frame));
            }
        }

        private void RangerDisconnected(string CameraID)
        {
            if (null != CameraDisconnected)
            {
                CameraDisconnected(CameraID);
            }
        }

    }
}
