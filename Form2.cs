using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VLC_HTTP_Launcher
{
    public partial class Form2 : Form
    {
        private readonly Form1 form1;
        public Form2(Form1 form)
        {
            form1 = form;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            form1.AddServer(textBox1.Text, textBox2.Text, textBox3.Text);
            this.Close();
        }
    }
}
