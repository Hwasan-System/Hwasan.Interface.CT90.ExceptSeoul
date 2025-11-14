using System;
using System.Windows.Forms;

namespace CT90
{
    public partial class ErrMsgForm : Form
    {
        public ErrMsgForm()
        {
            InitializeComponent();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
