using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sick.EasyRanger;
using Sick.EasyRanger.Base;
using Sick.GenIStream;
using Sick.StreamUI.ImageFormat;
using System.IO;
using System.Windows;
using EzRViewer;
using Sick.EasyRanger.Controls;
using System.Net;
using System.Windows.Threading;

namespace EzRDemo
{
    public partial class MainForm : Form
    {
        public delegate void UpdateListContext(string context);
        UpdateListContext m_UpdateListContext;

        //图像显示控件
        //ViewerWindowControl viwerWCtl = null;
        Logger logView = null;

        //ViewerWindowControl图像显示依赖
        public EzRViewerCtl ezRViewerAttach = new EzRViewerCtl();
        //2D显示控件
        View2DControl viewer2D_A;//显示控件A
        View2DControl viewer2D_B;//显示控件B
        //相机对象接口
        public ICameraDevice cam;

        readonly CameraDiscovery _discovery;
        public ICamera Camera { get; private set; }
        public FrameGrabber Grabber { get; set; }

        //EasyRanger 对象
        public ProcessingEnvironment easyRanger = new ProcessingEnvironment();

        public bool IsStarted => Grabber?.IsStarted ?? false;
        public bool IsStopped => IsConnected && !IsStarted;
        public bool IsConnected => Camera?.IsConnected ?? false;
        public bool IsDisconnected => !IsConnected;

        public MainForm()
        {
            InitializeComponent();
            var path = Environment.GetEnvironmentVariable("SICK_EASYRANGER_ROOT");
            _discovery = CameraDiscovery.CreateFromProducerFile($@"{path}\SICKGigEVisionTL.cti");
        }

        //Log记录数据
        public void OnUpdatListContext(string msg)
        {
            if (listBoxLog.InvokeRequired)
            {
                this.BeginInvoke(new UpdateListContext(OnUpdatListContext), msg);
            }
            else
            {
                if (listBoxLog.Items.Count < 2000)
                {
                    listBoxLog.Items.Add(DateTime.Now.ToString("HH:mm:ss:ffff:  ") + msg);
                }
                else
                {

                    listBoxLog.Items.RemoveAt(0);
                    listBoxLog.Items.Add(DateTime.Now.ToString("HH:mm:ss:ffff:  ") + msg);
                }
                //Log.WriteLog(msg);
                listBoxLog.SelectedIndex = listBoxLog.Items.Count - 1;
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //viwerWCtl = new ViewerWindowControl();
            logView = Logger.Instance();
            OnUpdatListContext("程序启动");
            viewer2D_A = new View2DControl();
            viewer2D_B = new View2DControl();
            elementHost2.Child = viewer2D_A;
            elementHost3.Child = viewer2D_B;
            //viewer2D_A.ShowInformation = Visibility.Hidden;
            //viewer2D_B.ShowInformation = Visibility.Hidden;
            viewer2D_A.FontSize = 8.0;
            viewer2D_B.FontSize = 8.0;
            //viwerWCtl.view2D.Visibility = Visibility.Visible;
            //viwerWCtl.view3D.Visibility = System.Windows.Visibility.Hidden;
            easyRanger.Load("EzRProg.env");

            viewer2D_A.Environment = easyRanger;
            viewer2D_B.Environment = easyRanger;

            var discoveredCameras = _discovery.ScanForCameras();
            if (discoveredCameras.Count == 0)
            {
                OnUpdatListContext("No cameras found");
                return;
            }
            else
            {
                OnUpdatListContext("Found "+ discoveredCameras.Count.ToString()+ " Camera");
                foreach(DiscoveredCamera cam in discoveredCameras)
                {
                    OnUpdatListContext("SN: " + cam.SerialNumber + " IP: " + cam.IpAddress.ToString()+ " Name: " + cam.UserDefinedName);
                }
            }
        }
        public string CalibPath = "";
        public string CamCfgPath = "";

        //加载相机配置文件路径
        private void btnLoadCamCfg_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All Files(*.*)|*.*|csv Files(*.csv)|*.csv";
            dialog.InitialDirectory = Directory.GetCurrentDirectory();

            if (DialogResult.OK == dialog.ShowDialog())
            {
                CamCfgPath = dialog.FileName;
                txtCamCfgPath.Text = dialog.FileName;
            }
        }
        //加载标定文件路径
        private void btnLoadCalib_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "json Files(*.json)|*.json|XML Files(*.xml)|*.xml";
            dialog.InitialDirectory = Directory.GetCurrentDirectory();

            if (DialogResult.OK == dialog.ShowDialog())
            {
                CalibPath = dialog.FileName;
                txtCamCalibPath.Text = dialog.FileName;

            }
        }

        //相机连接
        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                Camera = _discovery.ConnectTo(IPAddress.Parse(txtCameraName.Text));
                Camera.ImportParametersFromCsvData(txtCamCfgPath.Text);
                //Camera.GetCameraParameters().DeviceScanType.Set(DeviceScanType.LINESCAN_3D);
                //cam.CalibrationFile = txtCamCalibPath.Text; //标定文件
                //cam.ConfigurationFile = txtCamCfgPath.Text; //相机配置文件

                //cam.Connect();
                //cam.YResolution = (float)Convert.ToDouble(txtYResolution.Text); //Y方向分辨率
                //cam.RectificationWidth = Convert.ToInt32(txtImageWidth.Text);
                //cam.OutputMode = OutputType.CalibratedAndRectified; //标定输出图像模式
                //cam.RectificationMethod = RectificationMethods.TopMost; //图像校正方法
                Grabber?.Dispose();
                Grabber = Camera?.CreateFrameGrabber(10);
                Grabber?.Start();
                // Register to FrameReceived  event
                Grabber.FrameReceived += GrabberOnFrameReceived;
            }
            catch(Exception ee)
            {
                System.Windows.Forms.MessageBox.Show(ee.Message);
            }
            
        }
        //相机断开连接
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            try
            {
                cam.Stop();
                cam.Disconnect();
            }
            catch (Exception ee)
            {
                System.Windows.Forms.MessageBox.Show(ee.Message);
            }
        }
        private void GrabberOnFrameReceived(Sick.GenIStream.IFrame frame)
        {
            if (!frame.IsIncomplete())
            {
                this.BeginInvoke(new Action(() =>
                {
                    HandleFrame(frame);
                }));
            }
        }

        private void HandleFrame(Sick.GenIStream.IFrame frame)
        {
            //Add the frame to the Environment, 
            //this will creates an image variable with the name "Image".
            FromGenIStreamFrameConverter.AddFrameToEnvironment(frame, "Image", easyRanger);
            IStepProgram program = easyRanger.GetStepProgram(0);
            program.RunFromBeginning();

            viewer2D_A.DrawImage("Image", SubComponent.Range);

            // Access the image components
            //Sick.EasyRanger.Base.IFrame image = easyRanger.GetFrame("Image");
        }

        //加载图像处理任务
        private void btnLoadEnv_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = Directory.GetCurrentDirectory();

            if (DialogResult.OK == dialog.ShowDialog())
            {

                easyRanger.Load(dialog.FileName);
                //easyRanger.Save()
                //easyRanger.SaveImageToFile(@"D:\Img", "Image", true, false);
            }
        }


        //运行图像处理任务，显示结果图像，获取数据
        private void btnRunOnce_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime startTime = DateTime.Now;

                easyRanger.GetStepProgram("Main").RunFromBeginning(); //运行Main图像处理任务

                //viwerWCtl.CheckVariable("Image");
                ////viwerWCtl.view3D.Visibility = Visibility.Hidden;
                //viwerWCtl.RedrawAllVariables();
                //System.Windows.Forms.MessageBox.Show("运行所需要时间： " + (DateTime.Now - startTime).ToString());

                //2D显示
                viewer2D_A.ClearAll();
                viewer2D_B.ClearAll();
                viewer2D_A.DrawImage("Image", SubComponent.Intensity);
                viewer2D_B.DrawImage("Image", SubComponent.Intensity);

                ////获取平面度数据计算结果
                //double[] Dist = easyRanger.GetDouble("Tilt_Frame1");
                //txtResultDist.Text = Dist[0].ToString();
                //double[] Score = easyRanger.GetDouble("MatchScore");
                //txtScore.Text = Score[0].ToString();
            }
            catch (Exception ee)
            {
                System.Windows.Forms.MessageBox.Show(ee.Message);
            }

        }

        public void RunEzRProg()
        {
            try
            {
                easyRanger.GetStepProgram("Main").RunFromBeginning(); //运行Main图像处理任务


                ////获取平面度数据计算结果
                double[] Dist = easyRanger.GetDouble("Tilt_Frame1");
                txtResultDist.Text = Dist[0].ToString();

                double[] Score = easyRanger.GetDouble("MatchScore");
                txtScore.Text = Score[0].ToString();
                //MessageBox.Show(FlatnessB[0].ToString());
            }
            catch (Exception ee)
            {
                System.Windows.Forms.MessageBox.Show(ee.Message);
            }
        }

        //设置EzR图像处理程序任务中的工具参数
        public void SetInputPara()
        {
            //设置Main程序中，第一个工具，第一个输入参数的值
            easyRanger.GetStepProgram("Main").StepList[0].ArgumentList[0].Value = "";
        }


        //public void SetCamPara()
        //{
        //    CameraId cameraId = new CameraId("Camera1");
        //    CameraDevice cam = new CameraDevice(cameraId, easyRanger);
        //    cam.SetParameter("参数名称","参数值");
        //}

        /// <summary>
        /// 获取EzR子程序列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetProgram()
        {
            List<string> ListProg = new List<string>();
            foreach (StepProgram stp in easyRanger.Programs)
            {
                ListProg.Add(stp.Name);
            }
            return ListProg;
        }

        /// <summary>
        /// 获取子程序Step列表
        /// </summary>
        /// <param name="ProgName"></param>
        /// <returns></returns>
        public List<string> GetStepList(string ProgName)
        {
            List<string> ListStepName = new List<string>();
            StepProgram stp = (StepProgram)easyRanger.GetStepProgram(ProgName);
            
            foreach (Step step in stp.StepList)
            {
                ListStepName.Add(stp.Name);
            }
            return ListStepName;
        }



        private void btnSaveEzR_Click(object sender, EventArgs e)
        {
            if(DialogResult.Yes == System.Windows.Forms.MessageBox.Show("确定是否保存EzR图像处理程序？", "保存将覆盖原文件", MessageBoxButtons.YesNo))
            {
                easyRanger.Save("EzRProg.env");
            }
        }

        private void chkRunContinuous_CheckedChanged(object sender, EventArgs e)
        {
            if(chkRunContinuous.Checked)
            {
                chkRunContinuous.Text = "停止";
                runTimer.Start();
            }
            else
            {
                if(runTimer.Enabled)
                {
                    chkRunContinuous.Text = "持续运行";
                    runTimer.Stop();
                }
            }
        }

        private void runTimer_Tick(object sender, EventArgs e)
        {
            if(cam.AvailableBuffers>0 )
            {
                RunEzRProg();
            }
        }

        private void btnTeachROI_Click(object sender, EventArgs e)
        {
            if(DialogResult.Yes == System.Windows.Forms.MessageBox.Show("确定是否调整示教区域TeachROI？", "确认后勾选Edit Variable再调整区域", MessageBoxButtons.YesNo))
            {
                //viwerWCtl.ClearAllDrawing();
                //viwerWCtl.CheckVariable("Image");
                //viwerWCtl.CheckVariable("TeachROI");
                //viwerWCtl.RedrawAllVariables();
            }

        }

        private void btnDrawReg_Click(object sender, EventArgs e)
        {
            //easyRanger.GetStepProgram("Define").StepList[0].ArgumentList[0].Value = "";
            easyRanger.GetStepProgram("Define").StepList[0].ResultList[0].Value = "UserRegion";
            easyRanger.GetStepProgram("Define").RunFromBeginning();
            //viwerWCtl.Environment = easyRanger;
            //viwerWCtl.UnCheckAll();
            //viwerWCtl.CheckVariable("Image");
            //viwerWCtl.CheckVariable("UserRegion");
            ////viwerWCtl.CheckCreateAndEditButtons();
            //viwerWCtl.RedrawAllVariables();


        }


    }
}
