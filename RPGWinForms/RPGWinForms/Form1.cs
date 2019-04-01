using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RPGv2;

namespace RPGWinForms
{
    public partial class Form1 : Form
    {
        private static Form1 form = null;

        public Form1()
        {
            InitializeComponent();
            form = this;
        }

        public void LoadMap(Map m)
        {
            Bitmap bmp = new Bitmap(m.Img);
            int w = bmp.Width;
            int h = bmp.Height;
            int winSizeH = form.Height;
            int winSizeW = form.Width;
            double ratio = 0;
            ratio = winSizeH / h;
            Bitmap resized = new Bitmap(bmp, new Size(Convert.ToInt32(w * ratio), Convert.ToInt32(h * ratio)));
            pictureBox1.Image = resized;
            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
        }
    }
}
