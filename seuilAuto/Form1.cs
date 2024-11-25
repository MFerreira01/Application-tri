using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;

using System.Runtime.InteropServices;
using libImage;
using gige;

namespace seuilAuto
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }
        gige.IDevice m_device;
        Rectangle m_rect;
        PixelFormat m_pixelFormat;
        UInt32 m_pixelType;

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void OnLoad(object sender, EventArgs e)
        {
            bool cameraConnected = false;
            

            // initialize GigEVision API
            gige.GigEVisionSDK.InitGigEVisionAPI();
            gige.IGigEVisionAPI gigeVisionApi = gige.GigEVisionSDK.GetGigEVisionAPI();


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
                label1.BackColor = Color.Orange;
            }
        }

        private void buttonOuvrir_Click(object sender, EventArgs e)
        {
            if (ouvrirImage.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Bitmap bmp;
                    Image img = Image.FromFile(ouvrirImage.FileName);
                    bmp = new Bitmap(img);

                    imageDepart.Width = bmp.Width;
                    imageDepart.Height = bmp.Height;
                    // pour centrer image dans panel
                    if (imageDepart.Width < panel1.Width)
                        imageDepart.Left = (panel1.Width - imageDepart.Width) / 2;

                    if (imageDepart.Height < panel1.Height)
                        imageDepart.Top = (panel1.Height - imageDepart.Height) / 2;

                    imageDepart.Image = bmp;

                    imageSeuillee.Hide();
                    valeurSeuilAuto.Hide();
                }
                catch
                {
                    MessageBox.Show("erreur !");
                }
            }
        }

        private void seuillageAuto_Click(object sender, EventArgs e)
        {
            // traitement donc transférer data bmp vers C++

            imageSeuillee.Show();
            valeurSeuilAuto.Show();

            Bitmap bmp = new Bitmap(imageDepart.Image);
            ClImage Img = new ClImage();

            unsafe
            {
                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                Img.objetLibDataImgPtr(1, bmpData.Scan0, bmpData.Stride, bmp.Height, bmp.Width);
                // 1 champ texte retour C++, le seuil auto
                bmp.UnlockBits(bmpData);
            }

            valeurSeuilAuto.Text = Img.objetLibValeurChamp(0).ToString();

            imageSeuillee.Width = bmp.Width;
            imageSeuillee.Height = bmp.Height;

            // pour centrer image dans panel
            if (imageSeuillee.Width < panel1.Width)
                imageSeuillee.Left = (panel1.Width - imageSeuillee.Width) / 2;

            if (imageSeuillee.Height < panel1.Height)
                imageSeuillee.Top = (panel1.Height - imageSeuillee.Height) / 2;

            // transférer C++ vers bmp
            imageSeuillee.Image = bmp;
        }

        private void boutStart_Click(object sender, EventArgs e)
        {
            // PC1 : traitement
            // prendre l'image de la caméra

            // étape 1. Vérifier que la caméra est bien connectée

            OnLoad(null, EventArgs.Empty);

            if (m_device != null && m_device.IsConnected() && !m_device.IsBufferEmpty())
            {
                gige.IImageInfo imageInfo = null;
                m_device.GetImageInfo(ref imageInfo);

                if (imageInfo != null)
                {
                    Bitmap bitmap = (Bitmap)pbImgCam.Image;
                    BitmapData bd = null;

                    // Convert and display image
                    ImageUtils.CopyToBitmap(imageInfo, ref bitmap, ref bd, ref m_pixelFormat, ref m_rect, ref m_pixelType);

                    if (bitmap != null)
                    {
                        pbImgCam.Image = bitmap;
                        pbImgCam.SizeMode = PictureBoxSizeMode.Zoom;
                    }

                    // Release resources
                    if (bd != null) bitmap.UnlockBits(bd);
                    m_device.PopImage(imageInfo);
                }
            }

            // faire l'histogramme

            // prendre la décision selon l'histogramme



            // envoyer le verdict à PC2
        }
    }
}
