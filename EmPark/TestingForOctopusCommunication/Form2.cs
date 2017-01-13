using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestingForOctopusCommunication
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            string path = Application.StartupPath + "/" + "CriticalError.txt";
              System.IO.StreamReader sr = new System.IO.StreamReader(path);
    richTextBox1.Text = sr.ReadToEnd();
            sr.Close();
        }
    }
}
