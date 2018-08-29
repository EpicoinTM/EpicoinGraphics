using Epicoin.Library;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EpicoinGraphics
{
    public partial class Menu : Form
    {
        public Menu()
        {
            InitializeComponent();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Environment.Exit(0);
        }

        private void Menu_Load(object sender, EventArgs e)
        {
            this.HostBox.Text = Epicoin.Library.Epicoin.Host;
            this.HostBox.Refresh();
        }

        private void ServeurClick(object sender, EventArgs e)
        {
            Epicoin.Library.Epicoin.Host = this.HostBox.Text;
            this.Visible = false;
            Serveur serv = new Serveur();
            serv.ShowDialog();
        }

        private void MinerClick(object sender, EventArgs e)
        {
            Epicoin.Library.Epicoin.Host = this.HostBox.Text;
            this.Visible = false;
            Miner miner = new Miner();
            miner.ShowDialog();

        }

        private void UserClick(object sender, EventArgs e)
        {
            Epicoin.Library.Epicoin.Host = this.HostBox.Text;
            this.Visible = false;
            User user = new User();
            user.ShowDialog();
        }
    }
}
