using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IFCtoRevit.IFCLoader
{
    public partial class LoaderWin : Form
    {
        public static bool isCancelled = false;

        OpenFileDialog ofd;

        public static string FilePath;
        public LoaderWin()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            ofd = new OpenFileDialog();
            ofd.Filter = "IFC files (*.ifc)|*.ifc|All files (*.*)|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                FilePath = ofd.FileName;
                textBox1.Text = ofd.FileName;
            }

        }

        private void LoaderWin_Load(object sender, EventArgs e)
        {
            checkBox1.Checked = false;
            checkBox2.Checked = false;
            checkBox3.Checked = false;
            checkBox4.Checked = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Program.incsImport = checkBox2.Checked ? true : false;

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Program.colsImport = checkBox1.Checked ? true : false;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            Program.beamsImport = checkBox3.Checked ? true : false;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            Program.floorsImport = checkBox4.Checked ? true : false;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
