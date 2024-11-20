using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using System.Drawing;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using GigEVisionSDK_NET;
using gige;
using System.Reflection.Emit;


namespace libImage
{
    public class ClImage
    {
        public IntPtr ClPtr;

        public ClImage()
        {
            ClPtr = IntPtr.Zero;
        }

        ~ClImage()
        {
            if (ClPtr != IntPtr.Zero)
                ClPtr = IntPtr.Zero;
        }


        // va-et-vient avec constructeur C#/C++
        // obligatoire dans toute nouvelle classe propre à l'application

        [DllImport("libImage.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr objetLib();

        public IntPtr objetLibPtr()
        {
            ClPtr = objetLib();
            return ClPtr;
        }

        [DllImport("libImage.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr objetLibDataImg(int nbChamps, IntPtr data, int stride, int nbLig, int nbCol);

        public IntPtr objetLibDataImgPtr(int nbChamps, IntPtr data, int stride, int nbLig, int nbCol)
        {
            ClPtr = objetLibDataImg(nbChamps,data, stride, nbLig, nbCol);
            return ClPtr;
        }

        [DllImport("libImage.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double valeurChamp(IntPtr pImg, int i);

        public double objetLibValeurChamp(int i)
        {
            return valeurChamp(ClPtr, i);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            bool cameraConnected = false;

            // initialize GigEVision API
            gige.GigEVisionSDK.InitGigEVisionAPI();
            gige.IGigEVisionAPI gigeVisionApi = gige.GigEVisionSDK.GetGigEVisionAPI();

            if (!gigeVisionApi.IsUsingKernelDriver())
            {
                Text = Text + " (Warning: Smartek Filter Driver not loaded.)";
            }

            // discover all devices on network
            gigeVisionApi.FindAllDevices(3.0);
            gige.IDevice[] devices = gigeVisionApi.GetAllDevices();

            if (devices.Length > 0)
            {
                // take first device in list
                m_device = devices[0];

                // uncomment to use specific model
                //for (int i = 0; i < devices.Length; i++)
                //{
                //    if (devices[i].GetModelName() == "GC652M")
                //    {
                //        m_device = devices[i];
                //    }
                //}

                // to change number of images in image buffer from default 10 images 
                // call SetImageBufferFrameCount() method before Connect() method
                //m_device.SetImageBufferFrameCount(20);

                if (m_device != null && m_device.Connect())
                {
                    label1.Text = "Camera address:";
                    label2.Text = Common.IpAddrToString(m_device.GetIpAddress());

                    // disable trigger mode
                    bool status = m_device.SetStringNodeValue("TriggerMode", "Off");
                    // set continuous acquisition mode
                    status = m_device.SetStringNodeValue("AcquisitionMode", "Continuous");
                    // start acquisition
                    status = m_device.SetIntegerNodeValue("TLParamsLocked", 1);
                    status = m_device.CommandNodeExecute("AcquisitionStart");
                    timer1.Enabled = true;
                    cameraConnected = true;
                }
            }

            if (!cameraConnected)
            {
                label1.Text = "No camera connected";
            }
        }
    }
}
