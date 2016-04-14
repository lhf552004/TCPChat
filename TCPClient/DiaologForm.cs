using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TCPClient
{
    public partial class DiaologForm : Form
    {
        public DiaologForm(string title)
        {
            InitializeComponent();
            this.Text = title;
        }

        public static void ShowDialogueForm(ref string nickName)
        {
            
        }
    }
}
