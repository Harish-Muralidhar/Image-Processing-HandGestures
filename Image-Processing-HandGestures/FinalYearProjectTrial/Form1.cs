using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FinalYearProjectTrial
{
    public partial class MainPage : Form
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new Webcam().Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            new Import().Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            new Discover().Show();
            this.Hide();
        }
    }
}
