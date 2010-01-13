using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DUIP
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            Visual.View view = new Visual.View();
            view.Location = new Core.GeneralSector().Center;
            view.ZoomLevel = 0.7;

        }
    }
}
