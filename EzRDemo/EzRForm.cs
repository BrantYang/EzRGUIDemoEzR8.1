using EzRUtility;
using Sick.EasyRanger.Base;
using Sick.EasyRanger.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace EzRDemo
{
    public partial class EzRForm : Form
    {
        public delegate void UpdateListContext(string context);
        UpdateListContext m_UpdateListContext;
        //2D显示控件
        View2DControl viewer2D_A;//显示控件A
        View2DControl viewer2D_Config;//显示控件B

        View3DControl viewer3D; //3D显示控件
        RangerConfig cam3dCfg = new RangerConfig();

        EzRProgram ezrProg = null;
        Ranger3 cam3d = new Ranger3();
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

        public EzRForm()
        {
            InitializeComponent();
        }

        private void EzRForm_Load(object sender, EventArgs e)
        {
            ezrProg = new EzRProgram("EzRProg.env");
            OnUpdatListContext("程序启动");
            viewer2D_A = new View2DControl();
            viewer2D_Config = new View2DControl();
            viewer3D = new View3DControl();
            elementHost1.Child = viewer2D_Config;
            elementHost2.Child = viewer2D_A;
            elementHost3.Child = viewer3D;

            //viewer2D_A.ShowInformation = Visibility.Hidden;
            //viewer2D_B.ShowInformation = Visibility.Hidden;
            viewer2D_A.FontSize = 8.0;
            //viewer2D_B.FontSize = 8.0;
            //viwerWCtl.view2D.Visibility = Visibility.Visible;
            //viwerWCtl.view3D.Visibility = System.Windows.Visibility.Hidden;
            viewer2D_A.Environment = ezrProg.Env;
            viewer2D_Config.Environment = ezrProg.Env;
            viewer2D_Config.OnNewPoint += Viewer2D_Config_OnNewPoint;
            viewer2D_Config.OnNewRegion += Viewer2D_Config_OnNewRegion;
            viewer3D.Init(null, ezrProg.Env);

            viewer2D_Config.DrawImage("Image",SubComponent.Range);
        }

        private void Viewer2D_Config_OnNewRegion(object sender, System.Windows.Point[] firstCorners, System.Windows.Point[] secondCorners, double[] angles)
        {
            ezrProg.Env.SetRois("FitReg", firstCorners, secondCorners, RoiType.Rectangle, angles);
            viewer2D_Config.DrawRoi("FitReg", -1, System.Windows.Media.Color.FromRgb(0, 0, 255));
//throw new NotImplementedException();
        }

        private void Viewer2D_Config_OnNewPoint(object sender, System.Windows.Point[] points)
        {

            throw new NotImplementedException();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            cam3dCfg.IPAddress = txtCamIP.Text;
            cam3dCfg.YResolution = Convert.ToDouble(txtYResolution.Text);
            cam3d.Config = cam3dCfg;
            if(!cam3d.Init())
            {
                System.Windows.Forms.MessageBox.Show("相机连接失败");
            }
            else
            {
                cam3d.Start();
                cam3d.ImgReceived += Image3DReceived;
                cam3d.CameraDisconnected += Camera3dDiconnect;
            }

        }
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            cam3d.Disconnect();
        }

        private void Image3DReceived(Sick.EasyRanger.Base.IFrame frame)
        {
            if (null != frame)
            {
                if(this.IsHandleCreated)
                {
                    this.BeginInvoke(new MethodInvoker(() =>
                    {
                        HandleFrame(frame);
                    }));
                }

            }
        }

        private void Camera3dDiconnect(string CameraID)
        {
            if (this.IsHandleCreated)
            {
                this.BeginInvoke(new MethodInvoker(() =>
                {
                    OnUpdatListContext("相机" + CameraID + "掉线");
                }));
            }

        }

        int Count = 0;
        private void HandleFrame(Sick.EasyRanger.Base.IFrame frame)
        {
            try
            {
                OnUpdatListContext("图像计数:" +( Count++).ToString());
                //Add the frame to the Environment, 
                //this will creates an image variable with the name "Image".
                ezrProg.Env.SetFrame("Image", frame);
                //IStepProgram program = easyRanger.GetStepProgram(0);
                OnUpdatListContext("运行结束,耗时ms："+(ezrProg.RunSubProgram("Main").ToString()));

                viewer2D_A.DrawImage("Image", SubComponent.Range);
                viewer3D.Draw("Image");

            }
            catch (Exception ee)
            {
                OnUpdatListContext(ee.Message);
            }


            // Access the image components
            //Sick.EasyRanger.Base.IFrame image = easyRanger.GetFrame("Image");
        }

        private void btnEditRegion_Click(object sender, EventArgs e)
        {
            //iewer2D_Config.EditCheckBox_Check();
            //viewer2D_Config.CreateRegion_Checked(null, null);
        }

        private void btnEditPoint_Click(object sender, EventArgs e)
        {
            viewer2D_Config.CreatePoints2D_Click(null, null);
        }
    }
}
