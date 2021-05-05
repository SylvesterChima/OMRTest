using IronOcr;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OMRTest
{
    public partial class frmIronOCR : Form
    {
        public frmIronOCR()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var Ocr = new IronTesseract();
            using (var Input = new OcrInput(@"C:\AttendanceRoot\input\sample1.tif"))
            {
                var Result = Ocr.Read(Input);
                richTextBox1.AppendText(Result.Text);
            }
        }
    }
}
