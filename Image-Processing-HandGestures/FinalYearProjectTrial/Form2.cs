using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Imaging;
using AForge.Imaging.Filters;
using System.Threading;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using SVM;
using System.IO;

namespace FinalYearProjectTrial
{
    public partial class Webcam : Form
    {
        int label = 0;
        public Webcam()
        {
            InitializeComponent();
        }

        private FilterInfoCollection CaptureDevice;
        private VideoCaptureDevice FinalFrame;
        
       

        private void Webcam_Load(object sender, EventArgs e)
        {
            CaptureDevice = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach(FilterInfo Device in CaptureDevice)
            {
                comboBox1.Items.Add(Device.Name);

            }
            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
            else
                MessageBox.Show("no device");
            FinalFrame = new VideoCaptureDevice();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.Items.Count == 0)
            {
                MessageBox.Show("no device");
            }
            else
            {
                FinalFrame = new VideoCaptureDevice(CaptureDevice[comboBox1.SelectedIndex].MonikerString);
                FinalFrame.NewFrame += new NewFrameEventHandler(FinalFrame_NewFrame);
                FinalFrame.Start();
            }
        }

        private void FinalFrame_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(FinalFrame.IsRunning==true)
            pictureBox2.Image = (Bitmap)pictureBox1.Image.Clone();
            Bitmap InputImage = (Bitmap)pictureBox2.Image;
            Rectangle Tile = new Rectangle(0, 0, InputImage.Width, InputImage.Height);
            BitmapData bitmapdata = InputImage.LockBits(Tile, ImageLockMode.ReadWrite, InputImage.PixelFormat);
            int formatsize = Bitmap.GetPixelFormatSize(bitmapdata.PixelFormat) / 8;
            var tempreg = new byte[bitmapdata.Width * bitmapdata.Height * formatsize];
            Marshal.Copy(bitmapdata.Scan0, tempreg, 0, tempreg.Length);

            System.Threading.Tasks.Parallel.Invoke(
                () =>
                {
                    multithread1(tempreg, 0, 0, bitmapdata.Width / 2, bitmapdata.Height / 2, bitmapdata.Width, formatsize);
                },
                () =>
                {
                    multithread1(tempreg, 0, bitmapdata.Height / 2, bitmapdata.Width / 2, bitmapdata.Height, bitmapdata.Width, formatsize);
                },
                () =>
                {
                    multithread1(tempreg, bitmapdata.Width / 2, 0, bitmapdata.Width, bitmapdata.Height / 2, bitmapdata.Width, formatsize);
                },
                () =>
                {
                    multithread1(tempreg, bitmapdata.Width / 2, bitmapdata.Height / 2, bitmapdata.Width, bitmapdata.Height, bitmapdata.Width, formatsize);
                }
            );

            Marshal.Copy(tempreg, 0, bitmapdata.Scan0, tempreg.Length);
            InputImage.UnlockBits(bitmapdata);

            Grayscale grayfilter = new Grayscale(0.2125, 0.7154, 0.0721);//GrayscaleBT709 grayfilter=new GrayscaleBT709();
            Dilatation dilatefilter = new Dilatation();
            Erosion erodefilter = new Erosion();
            InputImage = grayfilter.Apply((Bitmap)InputImage);
            InputImage = dilatefilter.Apply((Bitmap)InputImage);
            InputImage = erodefilter.Apply((Bitmap)InputImage);
            //Opening openfilter = new Opening();
            //InputImage=openfilter.Apply((Bitmap)InputImage);
            //Closing closefilter = new Closing();
            //InputImage=closefilter.Apply((Bitmap)InputImage);

            ExtractBiggestBlob blob = new ExtractBiggestBlob();
            InputImage = blob.Apply(InputImage);
            int cordx = blob.BlobPosition.X;
            int cordy = blob.BlobPosition.Y;

            Bitmap source = new Bitmap(pictureBox1.Image);
            Bitmap destination = new Bitmap(InputImage);
            var sourcerectangle = new Rectangle(0, 0, source.Width, source.Height);
            var destinationrectangle = new Rectangle(0, 0, destination.Width, destination.Height);
            var sourcedata = source.LockBits(sourcerectangle, ImageLockMode.ReadWrite, source.PixelFormat);
            var destinationdata = destination.LockBits(destinationrectangle, ImageLockMode.ReadWrite, destination.PixelFormat);
            var sourcedepth = Bitmap.GetPixelFormatSize(sourcedata.PixelFormat) / 8;
            var destinationdepth = Bitmap.GetPixelFormatSize(destinationdata.PixelFormat) / 8;
            var source1 = new byte[sourcedata.Width * sourcedata.Height * sourcedepth];
            var destination1 = new byte[destinationdata.Width * destinationdata.Height * destinationdepth];
            Marshal.Copy(sourcedata.Scan0, source1, 0, source1.Length);
            Marshal.Copy(destinationdata.Scan0, destination1, 0, destination1.Length);

            System.Threading.Tasks.Parallel.Invoke(
                () =>
                {
                    multithread2(source1, destination1, cordx, 0, cordy, 0, cordx + (destinationdata.Width / 2), destinationdata.Width / 2, cordy + (destinationdata.Height / 2), destinationdata.Height / 2, sourcedata.Width, destinationdata.Width, sourcedepth, destinationdepth);
                },
                () =>
                {
                    multithread2(source1, destination1, cordx + (destinationdata.Width / 2), destinationdata.Width / 2, cordy, 0, cordx + (destinationdata.Width), destinationdata.Width, cordy + (destinationdata.Height / 2), destinationdata.Height / 2, sourcedata.Width, destinationdata.Width, sourcedepth, destinationdepth);
                },
                () =>
                {
                    multithread2(source1, destination1, cordx, 0, cordy + (destinationdata.Height / 2), destinationdata.Height / 2, cordx + (destinationdata.Width / 2), destinationdata.Width / 2, cordy + (destinationdata.Height), destinationdata.Height, sourcedata.Width, destinationdata.Width, sourcedepth, destinationdepth);
                },
                () =>
                {
                    multithread2(source1, destination1, cordx + (destinationdata.Width / 2), destinationdata.Width / 2, cordy + (destinationdata.Height / 2), destinationdata.Height / 2, cordx + (destinationdata.Width), destinationdata.Width, cordy + (destinationdata.Height), destinationdata.Height, sourcedata.Width, destinationdata.Width, sourcedepth, destinationdepth);
                }
            );

            Marshal.Copy(source1, 0, sourcedata.Scan0, source1.Length);
            Marshal.Copy(destination1, 0, destinationdata.Scan0, destination1.Length);
            source.UnlockBits(sourcedata);
            destination.UnlockBits(destinationdata);
            InputImage = destination;

            InputImage = grayfilter.Apply((Bitmap)InputImage);
            CannyEdgeDetector edgesoutline = new CannyEdgeDetector();
            InputImage = edgesoutline.Apply(InputImage);
            pictureBox2.Image = InputImage;

            Bitmap blocks = new Bitmap(InputImage);
            int[] numofedges = new int[100];
            double[] normalized = new double[400];
            String alphabet = null;
            int total = 0;
            int sq = 1;
            for (int p = 1; p <= 8; p++)
            {
                for (int q = 1; q <= 8; q++)
                {
                    for (int x = (p - 1) * blocks.Width / 8; x < (p * blocks.Width / 8); x++)
                    {
                        for (int y = (q - 1) * blocks.Height / 8; y < (q * blocks.Height / 8); y++)
                        {
                            Color colorPixel = blocks.GetPixel(x, y);

                            int r = colorPixel.R;
                            int g = colorPixel.G;
                            int b = colorPixel.B;

                            if (r != 0 & g != 0 & b != 0)
                                numofedges[sq]++;
                        }

                    }
                    sq++;
                }
            }

            for (sq = 1; sq <= 64; sq++)
                total = total + numofedges[sq];
            for (sq = 1; sq <= 64; sq++)
            {
                normalized[sq] = (double)numofedges[sq] / total;
                alphabet = alphabet + " " + sq.ToString() + ":" + normalized[sq].ToString();
            }
            File.WriteAllText(@"datasets\testalpha.txt", label.ToString() + alphabet + Environment.NewLine);

            Problem train = Problem.Read(@"datasets\trainedset.txt");
            Problem test = Problem.Read(@"datasets\testalpha.txt");
            Parameter parameter = new Parameter();
            parameter.C = 32;
            parameter.Gamma = 8;
            Model model = Training.Train(train, parameter);
            Prediction.Predict(test, @"datasets\result.txt", model, false);
            int value = Convert.ToInt32(File.ReadAllText(@"datasets\result.txt"));
            String res = null;
            res = res + (char)(value + 65);
            label1.Text = res;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if(FinalFrame.IsRunning==true)
            FinalFrame.Stop();
        }
        async Task PutTaskDelay()
        {
            await Task.Delay(4000);
        }

        

        private async void button3_Click(object sender, EventArgs e)
        {
            top:
            await PutTaskDelay();
            if (FinalFrame.IsRunning == true)
            {
                pictureBox2.Image = (Bitmap)pictureBox1.Image.Clone();
                Bitmap InputImage = (Bitmap)pictureBox2.Image;
                Rectangle Tile = new Rectangle(0, 0, InputImage.Width, InputImage.Height);
                BitmapData bitmapdata = InputImage.LockBits(Tile, ImageLockMode.ReadWrite, InputImage.PixelFormat);
                int formatsize = Bitmap.GetPixelFormatSize(bitmapdata.PixelFormat) / 8;
                var tempreg = new byte[bitmapdata.Width * bitmapdata.Height * formatsize];
                Marshal.Copy(bitmapdata.Scan0, tempreg, 0, tempreg.Length);

                System.Threading.Tasks.Parallel.Invoke(
                    () =>
                    {
                        multithread1(tempreg, 0, 0, bitmapdata.Width / 2, bitmapdata.Height / 2, bitmapdata.Width, formatsize);
                    },
                    () =>
                    {
                        multithread1(tempreg, 0, bitmapdata.Height / 2, bitmapdata.Width / 2, bitmapdata.Height, bitmapdata.Width, formatsize);
                    },
                    () =>
                    {
                        multithread1(tempreg, bitmapdata.Width / 2, 0, bitmapdata.Width, bitmapdata.Height / 2, bitmapdata.Width, formatsize);
                    },
                    () =>
                    {
                        multithread1(tempreg, bitmapdata.Width / 2, bitmapdata.Height / 2, bitmapdata.Width, bitmapdata.Height, bitmapdata.Width, formatsize);
                    }
                );

                Marshal.Copy(tempreg, 0, bitmapdata.Scan0, tempreg.Length);
                InputImage.UnlockBits(bitmapdata);

                Grayscale grayfilter = new Grayscale(0.2125, 0.7154, 0.0721);//GrayscaleBT709 grayfilter=new GrayscaleBT709();
                Dilatation dilatefilter = new Dilatation();
                Erosion erodefilter = new Erosion();
                InputImage = grayfilter.Apply((Bitmap)InputImage);
                InputImage = dilatefilter.Apply((Bitmap)InputImage);
                InputImage = erodefilter.Apply((Bitmap)InputImage);
                //Opening openfilter = new Opening();
                //InputImage=openfilter.Apply((Bitmap)InputImage);
                //Closing closefilter = new Closing();
                //InputImage=closefilter.Apply((Bitmap)InputImage);

                ExtractBiggestBlob blob = new ExtractBiggestBlob();
                InputImage = blob.Apply(InputImage);
                int cordx = blob.BlobPosition.X;
                int cordy = blob.BlobPosition.Y;

                Bitmap source = new Bitmap(pictureBox1.Image);
                Bitmap destination = new Bitmap(InputImage);
                var sourcerectangle = new Rectangle(0, 0, source.Width, source.Height);
                var destinationrectangle = new Rectangle(0, 0, destination.Width, destination.Height);
                var sourcedata = source.LockBits(sourcerectangle, ImageLockMode.ReadWrite, source.PixelFormat);
                var destinationdata = destination.LockBits(destinationrectangle, ImageLockMode.ReadWrite, destination.PixelFormat);
                var sourcedepth = Bitmap.GetPixelFormatSize(sourcedata.PixelFormat) / 8;
                var destinationdepth = Bitmap.GetPixelFormatSize(destinationdata.PixelFormat) / 8;
                var source1 = new byte[sourcedata.Width * sourcedata.Height * sourcedepth];
                var destination1 = new byte[destinationdata.Width * destinationdata.Height * destinationdepth];
                Marshal.Copy(sourcedata.Scan0, source1, 0, source1.Length);
                Marshal.Copy(destinationdata.Scan0, destination1, 0, destination1.Length);

                System.Threading.Tasks.Parallel.Invoke(
                    () =>
                    {
                        multithread2(source1, destination1, cordx, 0, cordy, 0, cordx + (destinationdata.Width / 2), destinationdata.Width / 2, cordy + (destinationdata.Height / 2), destinationdata.Height / 2, sourcedata.Width, destinationdata.Width, sourcedepth, destinationdepth);
                    },
                    () =>
                    {
                        multithread2(source1, destination1, cordx + (destinationdata.Width / 2), destinationdata.Width / 2, cordy, 0, cordx + (destinationdata.Width), destinationdata.Width, cordy + (destinationdata.Height / 2), destinationdata.Height / 2, sourcedata.Width, destinationdata.Width, sourcedepth, destinationdepth);
                    },
                    () =>
                    {
                        multithread2(source1, destination1, cordx, 0, cordy + (destinationdata.Height / 2), destinationdata.Height / 2, cordx + (destinationdata.Width / 2), destinationdata.Width / 2, cordy + (destinationdata.Height), destinationdata.Height, sourcedata.Width, destinationdata.Width, sourcedepth, destinationdepth);
                    },
                    () =>
                    {
                        multithread2(source1, destination1, cordx + (destinationdata.Width / 2), destinationdata.Width / 2, cordy + (destinationdata.Height / 2), destinationdata.Height / 2, cordx + (destinationdata.Width), destinationdata.Width, cordy + (destinationdata.Height), destinationdata.Height, sourcedata.Width, destinationdata.Width, sourcedepth, destinationdepth);
                    }
                );

                Marshal.Copy(source1, 0, sourcedata.Scan0, source1.Length);
                Marshal.Copy(destination1, 0, destinationdata.Scan0, destination1.Length);
                source.UnlockBits(sourcedata);
                destination.UnlockBits(destinationdata);
                InputImage = destination;

                InputImage = grayfilter.Apply((Bitmap)InputImage);
                CannyEdgeDetector edgesoutline = new CannyEdgeDetector();
                InputImage = edgesoutline.Apply(InputImage);
                pictureBox2.Image = InputImage;

                Bitmap blocks = new Bitmap(InputImage);
                int[] numofedges = new int[100];
                double[] normalized = new double[400];
                String alphabet = null;
                int total = 0;
                int sq = 1;
                for (int p = 1; p <= 8; p++)
                {
                    for (int q = 1; q <= 8; q++)
                    {
                        for (int x = (p - 1) * blocks.Width / 8; x < (p * blocks.Width / 8); x++)
                        {
                            for (int y = (q - 1) * blocks.Height / 8; y < (q * blocks.Height / 8); y++)
                            {
                                Color colorPixel = blocks.GetPixel(x, y);

                                int r = colorPixel.R;
                                int g = colorPixel.G;
                                int b = colorPixel.B;

                                if (r != 0 & g != 0 & b != 0)
                                    numofedges[sq]++;
                            }

                        }
                        sq++;
                    }
                }

                for (sq = 1; sq <= 64; sq++)
                    total = total + numofedges[sq];
                for (sq = 1; sq <= 64; sq++)
                {
                    normalized[sq] = (double)numofedges[sq] / total;
                    alphabet = alphabet + " " + sq.ToString() + ":" + normalized[sq].ToString();
                }
                File.WriteAllText(@"datasets\testalpha.txt", label.ToString() + alphabet + Environment.NewLine);

                Problem train = Problem.Read(@"datasets\trainedset.txt");
                Problem test = Problem.Read(@"datasets\testalpha.txt");
                Parameter parameter = new Parameter();
                parameter.C = 32;
                parameter.Gamma = 8;
                Model model = Training.Train(train, parameter);
                Prediction.Predict(test, @"datasets\result.txt", model, false);
                int value = Convert.ToInt32(File.ReadAllText(@"datasets\result.txt"));
                String res = null;
                res = res + (char)(value + 65);
                label1.Text = res;

            }
            goto top;
        }
      

        private void button5_Click(object sender, EventArgs e)
        {
            new MainPage().Show();
            this.FinalFrame.Stop();
            this.Hide();
        }
        public void multithread1(byte[] reg, int x, int y, int m, int n, int length, int size)
        {
            for (int i = x; i < m; i++)
            {
                for (int j = y; j < n; j++)
                {
                    var displacement = ((j * length) + i) * size;
                    var r = reg[displacement + 2];
                    var g = reg[displacement + 1];
                    var b = reg[displacement + 0];
                    if (r >= 45 & r <= 255 & g > 34 & g <= 229 & b >= 15 & b <= 200 & r - g >= 11 & r - b >= 15 & g - b >= 4 & r > g & r > b & g > b)
                        reg[displacement + 0] = reg[displacement + 1] = reg[displacement + 2] = 255;
                    else
                        reg[displacement + 0] = reg[displacement + 1] = reg[displacement + 2] = 0;
                }
            }

        }

        public void multithread2(byte[] srcbuf, byte[] dstbuf, int srcx, int dstx, int srcy, int dsty, int srcex, int dstex, int srcey, int dstey, int srcwidth, int dstwidth, int srcdepth, int dstdepth)
        {

            for (int i = srcx, m = dstx; (i < srcex & m < dstex); i++, m++)
            {
                for (int j = srcy, n = dsty; (j < srcey & n < dstey); j++, n++)
                {
                    var offset = ((j * srcwidth) + i) * srcdepth;
                    var offset1 = ((n * dstwidth) + m) * dstdepth;

                    var srcB = srcbuf[offset + 0];
                    var srcG = srcbuf[offset + 1];
                    var srcR = srcbuf[offset + 2];
                    var dstB = dstbuf[offset1 + 0];
                    var dstG = dstbuf[offset1 + 1];
                    var dstR = dstbuf[offset1 + 2];
                    if (dstR != 0 & dstG != 0 & dstB != 0)
                    {
                        dstbuf[offset1 + 0] = srcbuf[offset + 0];
                        dstbuf[offset1 + 1] = srcbuf[offset + 1];
                        dstbuf[offset1 + 2] = srcbuf[offset + 2];
                    }
                }
            }
        }


    }
}
