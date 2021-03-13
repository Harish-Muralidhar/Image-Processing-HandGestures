
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

    public partial class Import : Form
    {

        public Import()
        {
            InitializeComponent();
        }
        int label = 0;
        private void button3_Click(object sender, EventArgs e)
        {
            new MainPage().Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialogbox = new OpenFileDialog();
            dialogbox.Filter = "Choose Image(*.jpg;*.png;*.gif) | *.jpg;*.png;*.gif";
            if (dialogbox.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = System.Drawing.Image.FromFile(dialogbox.FileName);
            }
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            OpenFileDialog dialogbox = new OpenFileDialog();
            dialogbox.Filter = "Choose Image(*.jpg;*.png;*.gif) | *.jpg;*.png;*.gif";
            if (dialogbox.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = System.Drawing.Image.FromFile(dialogbox.FileName);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Bitmap InputImage = new Bitmap(pictureBox1.Image);
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



//references and different methods tested during coding to obtain best result 
/*SKIN DETECTION REFERENCEs INCLUDING MULTITHREADING- different methods tested
//Hue saturation
private void hSVSkinDetectionToolStripMenuItem_Click(object sender, EventArgs e)
{
float max = 0;
float h = 0, s = 0;
Bitmap image = new Bitmap(pictureBox1.Image);
for (int x = 0; x < image.Width / 2; x++)
{
for (int y = 0; y < image.Height / 2; y++)
{
Color colorPixel = image.GetPixel(x, y);
int r = colorPixel.R;
int g = colorPixel.G;
int b = colorPixel.B;
max = Math.Max(r, Math.Max(g, b));
float differenceMinMax = max - Math.Min(r, Math.Min(g, b));
if (max == r)
{
h = 60 * ((g - b) / differenceMinMax);
}
else if (max == g)
{
h = 60 * (((b - r) / differenceMinMax));
}
else if (max == b)
{
h = 60 * (((r - g) / differenceMinMax));
}
if (max != 0)
{
s = differenceMinMax / max;
}
else if (max == 0)
{
s = 0;
}
float v = max;
if (h >= 0 & h <= 500 & s >= 0.23 & s <= 0.68)
{
image.SetPixel(x, y, Color.White);
}
else
{
image.SetPixel(x, y, Color.Black);
}
pictureBox2.Image = (Bitmap)image;
}
}
for (int i = image.Width / 2; i < image.Width; i++)
{
for (int j = 0; j < image.Height / 2; j++)
{
Color colorPixel = image.GetPixel(i, j);
float r = colorPixel.R;
float g = colorPixel.G;
float b = colorPixel.B;
max = Math.Max(r, Math.Max(g, b));
float differenceMinMax = max - Math.Min(r, Math.Min(g, b));
if (max == r)
{
h = 60 * ((g - b) / differenceMinMax);
}
else if (max == g)
{
h = 60 * (((b - r) / differenceMinMax));
}
else if (max == b)
{
h = 60 * (((r - g) / differenceMinMax));
}
if (max != 0)
{
s = differenceMinMax / max;
}
else if (max == 0)
{
s = 0;
}
if (h >= 0 & h <= 500 & s >= 0.23 & s <= 0.68)
{
image.SetPixel(i, j, Color.White);
}
else
{
image.SetPixel(i, j, Color.Black);
}
pictureBox2.Image = (Bitmap)image;
}
}
for (int k = 0; k < image.Width / 2; k++)
{
for (int l = image.Height / 2; l < image.Height; l++)
{
Color colorPixel = image.GetPixel(k, l);
float r = colorPixel.R;
float g = colorPixel.G;
float b = colorPixel.B;
max = Math.Max(r, Math.Max(g, b));
float differenceMinMax = max - Math.Min(r, Math.Min(g, b));
if (max == r)
{
h = 60 * ((g - b) / differenceMinMax);
}
else if (max == g)
{
h = 60 * (((b - r) / differenceMinMax));
}
else if (max == b)
{
h = 60 * (((r - g) / differenceMinMax));
}
if (max != 0)
{
s = differenceMinMax / max;
}
else if (max == 0)
{
s = 0;
}
if (h >= 0 & h <= 500 & s >= 0.23 & s <= 0.68)
{
image.SetPixel(k, l, Color.White);
}
else
{
image.SetPixel(k, l, Color.Black);
}
pictureBox2.Image = (Bitmap)image;
}
}
for (int m = image.Width / 2; m < image.Width; m++)
{
for (int n = image.Height / 2; n < image.Height; n++)
{
Color colorPixel = image.GetPixel(m, n);
int r = colorPixel.R;
int g = colorPixel.G;
int b = colorPixel.B;
max = Math.Max(r, Math.Max(g, b));
float differenceMinMax = max - Math.Min(r, Math.Min(g, b));
if (max == r)
{
h = 60 * ((g - b) / differenceMinMax);
}
else if (max == g)
{
h = 60 * (((b - r) / differenceMinMax));
}
else if (max == b)
{
h = 60 * (((r - g) / differenceMinMax));
}
if (max != 0)
{
s = differenceMinMax / max;
}
else if (max == 0)
{
s = 0;
}
if (h >= 0 & h <= 500 & s >= 0.23 & s <= 0.68)
{
image.SetPixel(m, n, Color.White);
}
else
{
image.SetPixel(m, n, Color.Black);
}
pictureBox2.Image = (Bitmap)image;
}
}

    
//lock bits using bitmapdata available in the system
public void skindetection(object import)
{
Bitmap bmap = (Bitmap)import;
unsafe
{
BitmapData bitmapdata = bmap.LockBits(new Rectangle(0, 0, bmap.Width, bmap.Height), ImageLockMode.ReadWrite, bmap.PixelFormat);
int bytesperpixel = System.Drawing.Bitmap.GetPixelFormatSize(bmap.PixelFormat) / 8;
int heightinpixels = bitmapdata.Height;
int widthinbytes = bitmapdata.Width*bytesperpixel;
byte* ptrfirstpixel = (byte*)bitmapdata.Scan0;
Parallel.For(0, heightinpixels, y =>
{
byte* currentline = ptrfirstpixel + (y * bitmapdata.Stride);
for (int x = 0; x < widthinbytes; x = x + bytesperpixel)
{
int ob = currentline[x];
int og = currentline[x + 1];
int or = currentline[x + 2];
currentline[x] = 0;
currentline[x + 1] = (byte)og;
currentline[x + 2] = (byte)or;
}
});
bmap.UnlockBits(bitmapdata);
}
onimagefinished(bmap);
}
public void onimagefinished(Bitmap bmap)
{
pictureBox2.Image = bmap;
}

//system threads canvas draw image using graphics 
Bitmap InputImage = new Bitmap(pictureBox1.Image);
Thread t1 = new Thread(new ParameterizedThreadStart(import.skindetection));
t1.Start(InputImage);
Bitmap InputImage = new Bitmap(pictureBox1.Image) ;
Bitmap bmap = new Bitmap(pictureBox1.Image);
Size TileSize = new Size(bmap.Width / 4, bmap.Height / 2);
bmp = new Bitmap[4, 2];
for (int i = 0; i < 4; i++)
{
for (int j = 0; j < 2; j++)
{
Rectangle MovingTileFrames = new Rectangle(i * TileSize.Width, j * TileSize.Height, TileSize.Width, TileSize.Height);
bmp[i, j] = new Bitmap(TileSize.Width, TileSize.Height);
using (Graphics canvas = Graphics.FromImage(bmp[i, j]))
{
canvas.DrawImage(bmap, new Rectangle(0, 0, TileSize.Width, TileSize.Height), MovingTileFrames, GraphicsUnit.Pixel);
}                   
}
}
Parallel.For(0, 4, x =>
{
for (int x = 0; x < InputImage.Width; x++)
{
for (int y = 0; y < 2; y++)
{
int width = bmp[x, y].Width;
int height = bmp[x, y].Height;
for (int i = 0; i < width; i++)
{
for (int j = 0; j < height; j++)
{
Color colorPixel = bmp[x, y].GetPixel(i, j);
double r = colorPixel.R;
double g = colorPixel.G;
double b = colorPixel.B;
if (r >= 45 & r <= 255 & g > 34 & g <= 229 & b >= 15 & b <= 200 & (r - g) >= 11 & (r - b) >= 15 & (g - b) >= 4 & r > g & r > b & g > b)
bmp[x, y].SetPixel(i, j, Color.White);
else
bmp[x, y].SetPixel(i, j, Color.Black);
InputImage = bmp[x, y];
}
}
pictureBox2.Image = InputImage;
}
Color colorPixel = InputImage.GetPixel(x, y);
double r = colorPixel.R;
double g = colorPixel.G;
double b = colorPixel.B;


//combination of hsv, rgb and ycbcr
double yellow = ((0.299 * r) + (0.287 * g) + (0.11 * b));
double cr = r - yellow;
double cb = b - yellow;
double v1 = Math.Max(r,g);
double v = Math.Max(v1, b);
double u1 = Math.Min(r, g);
double u = Math.Min(u1, b);
double s = (v - u) / v;
if (v == r)
h = (g - b) / (6 * s);
if (v == g)
h = ((1 / 3) + ((b - r) / (6 * s)));
if (v == b)
h = ((2 / 3) + ((r - g) / s));
if ((r > 95 & g > 40 & b > 20 & r > g & r > b & (r - g) > 15 & cr > 135 & cb > 85 & y > 80 & cr <= ((1.5862 * cb) + 20) & cr >= ((0.3448 * cb) + 76.2069) & cr >= ((-4.5652 * cb) + 234.5652) & cr <= ((-1.15 * cb) + 301.75) & cr <= ((-2.2857 * cb) + 432.85)) || ((h >= 0.0 && h <= 50) & (s >= 0.23 && s <= 0.68) & r > 95 & g > 40 & b > 20 & r > g & r > b & (r - g) > 15))
if ((r >= 45 & r <= 255) & (g > 34 & g <= 229) & (b >= 15 & b <= 200) & (r - g >= 11 & r - b >= 15 & g - b >= 4) & (r > g & r > b & g > b))
InputImage.SetPixel(x, y, Color.White);
else
InputImage.SetPixel(x, y, Color.Black);
pictureBox2.Image = (Bitmap)InputImage;
}
});
for(int i=0;i<bmap.Width;i++)
for(int j=0;j<bmap.Height;j++)
pictureBox2.Image = bmp[i,j];
private void button2_Click(object sender, EventArgs e)
{
Bitmap InputImage = new Bitmap(pictureBox1.Image);
dest = import.skindetection(InputImage);
pictureBox2.Image = dest;
} 
}*/




//merge- different methods setpixel and lockbits tested
//lockbits
/*Bitmap source = originalimage;
Bitmap destination = InputImage;
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
ResizeBilinear resize = new ResizeBilinear(200, 200);
InputImage = resize.Apply(InputImage);
pictureBox2.Image = InputImage;

//threaded task using pointer offsets
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

//setpixel
Bitmap sampleImage = new Bitmap(pictureBox1.Image);
Bitmap sampleImage1 = new Bitmap(InputImage);
for (int i = cordx, m = 0; (i < sampleImage1.Width | m < sampleImage1.Width); i++, m++)
{
for (int j = cordy, n = 0; (j < sampleImage1.Height | n < sampleImage1.Height); j++, n++)
{
Color colorPixel = sampleImage.GetPixel(i, j);
Color colorPixel1 = sampleImage1.GetPixel(m, n);
int r = colorPixel.R; 
int g = colorPixel.G;
int b = colorPixel.B;
int r1 = colorPixel1.R;
int g1 = colorPixel1.G;
int b1 = colorPixel1.B;
if (r1 > 250 & g1 > 250 & b1 > 250)
sampleImage1.SetPixel(m, n, colorPixel);
else
sampleImage1.SetPixel(m, n, Color.Black);
pictureBox2.Image = (Bitmap)sampleImage1;
}
}*/
