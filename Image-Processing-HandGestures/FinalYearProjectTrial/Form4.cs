using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


namespace FinalYearProjectTrial
{
    public partial class Discover : Form
    {
        string text;
        int textlength = 0;
        public Discover()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            new MainPage().Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            text = textBox1.Text;
            textlength = text.Length;
            if (textlength == 0)
                MessageBox.Show("No alphabet entered. Please enter an alphabet");
            else if (textlength > 1)
                MessageBox.Show("Please enter only one alphabet");
            else
            {
                text = text.ToUpper();

                switch (text)
                {
                    case "A":
                        pictureBox1.Image = Image.FromFile(@"datasets\discover\A.jpg");
                        break;
                    case "B":
                        pictureBox1.Image = Image.FromFile(@"datasets\discover\B.jpg");
                        break;
                    case "C":
                        pictureBox1.Image = Image.FromFile(@"datasets\discover\C.jpg");
                        break;
                    case "D":
                        pictureBox1.Image = Image.FromFile(@"datasets\discover\D.jpg");
                        break;
                    case "E":
                        pictureBox1.Image = Image.FromFile(@"datasets\discover\E.jpg");
                        break;
                    case "F":
                        pictureBox1.Image = Image.FromFile(@"datasets\discover\F.jpg");
                        break;
                    case "G":
                        pictureBox1.Image = Image.FromFile(@"datasets\discover\G.jpg");
                        break;
                    case "H":
                        pictureBox1.Image = Image.FromFile(@"datasets\discover\H.jpg");
                        break;
                    case "I":
                        pictureBox1.Image = Image.FromFile(@"datasets\discover\I.jpg");
                        break;
                    case "J":
                        pictureBox1.Image = Image.FromFile(@"datasets\discover\J.jpg");
                        break;
                    case "K":
                        pictureBox1.Image = Image.FromFile(@"datasets\discover\K.jpg");
                        break;
                    case "L":
                        pictureBox1.Image = Image.FromFile(@"datasets\discover\L.jpg");
                        break;
                    case "M":
                        pictureBox1.Image = Image.FromFile(@"datasets\discover\M.jpg");
                        break;
                    case "N":
                        pictureBox1.Image = Image.FromFile(@"datasets\discover\N.jpg");
                        break;
                    case "O":
                        pictureBox1.Image = Image.FromFile(@"datasets\discover\O.jpg");
                        break;
                    case "P":
                        pictureBox1.Image = Image.FromFile(@"datasets\discover\P.jpg");
                        break;
                    case "Q":
                        pictureBox1.Image = Image.FromFile(@"datasets\discover\Q.jpg");
                        break;
                    case "R":
                        pictureBox1.Image = Image.FromFile(@"datasets\discover\R.jpg");
                        break;
                    case "S":
                        pictureBox1.Image = Image.FromFile(@"datasets\discover\S.jpg");
                        break;
                    case "T":
                        pictureBox1.Image = Image.FromFile(@"datasets\discover\T.jpg");
                        break;
                    case "U":
                        pictureBox1.Image = Image.FromFile(@"datasets\discover\U.jpg");
                        break;
                    case "V":
                        pictureBox1.Image = Image.FromFile(@"datasets\discover\V.jpg");
                        break;
                    case "W":
                        pictureBox1.Image = Image.FromFile(@"datasets\discover\W.jpg");
                        break;
                    case "X":
                        pictureBox1.Image = Image.FromFile(@"datasets\discover\X.jpg");
                        break;
                    case "Y":
                        pictureBox1.Image = Image.FromFile(@"datasets\discover\Y.jpg");
                        break;
                    case "Z":
                        pictureBox1.Image = Image.FromFile(@"datasets\discover\Z.jpg");
                        break;
                    default:
                        MessageBox.Show("Invalid alphabet. Please enter a valid alphabet");
                        break;
                }
            }
        }
    }

}


