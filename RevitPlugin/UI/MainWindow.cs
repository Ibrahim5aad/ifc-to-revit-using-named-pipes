using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IFCtoRevit.UI
{
    public partial class MainWindow : Form
    {
        public bool isCancelled = false;
        private const int CP_NOCLOSE_BUTTON = 0x200;


        public MainWindow()
        {
            InitializeComponent();
        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
        private void button1_Click(object sender, EventArgs e)
        {
            okBtn.Enabled = true;
        }
        private void okBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            isCancelled = true;
            this.Close();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            okBtn.Enabled = false;

        }
    }
}
