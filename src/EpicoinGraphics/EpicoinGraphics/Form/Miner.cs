using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Epicoin.Library;
using Epicoin.Library.Tools;
using Epicoin.Library.Net.Client;
using Epicoin.Library.Net.Server;

namespace EpicoinGraphics
{
    public partial class Miner : Form
    {
        public Miner()
        {
            Epicoin.Library.Epicoin.Continue = false;
            Epicoin.Library.Epicoin.ImportWallet();
            if (Epicoin.Library.Epicoin.Wallet == null)
            {
                string name = "";
                while (name == "")
                {
                    name = Microsoft.VisualBasic.Interaction.InputBox("Your pseudo :  ", "Your pseudo", "Bob");
                }
                Epicoin.Library.Epicoin.CreateWallet(name);
                Epicoin.Library.Epicoin.ExportWallet();
            }

            Epicoin.Library.Epicoin.Client = new Client(Epicoin.Library.Epicoin.Host, Epicoin.Library.Epicoin.Port);
            
            InitializeComponent();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Epicoin.Library.Epicoin.Continue = false;
            if (this.worker != null)
            {
                this.worker.Abort();
            }
            this.worker = null;
            this.work = false;
            Epicoin.Library.Epicoin.ExportWallet();
            Environment.Exit(0);
        }

        protected void StartClick(object sender, EventArgs e)
        {
            if (!this.work)
            {
                Epicoin.Library.Epicoin.Continue = true;
                Epicoin.Library.Epicoin.Log = new Logger();
                DataClient.Continue = true;
                DataServer.Continue = true;
                this.work = true;
                this.worker = new Thread(Epicoin.Library.Epicoin.Mine) { Priority = ThreadPriority.Highest };
                this.worker.Start(Epicoin.Library.Epicoin.Wallet.Address[0]);
                logWorker = new Thread(UpdateLog);
                logWorker.Start();
                Invoke(new MethodInvoker(delegate { StartLog(); }));
            }
        }

        protected void StopClick(object sender, EventArgs e)
        {
            if (this.work)
            {
                try
                {
                    logWorker.Abort();
                    logWorker = null;
                }
                catch(Exception)
                { }
                Epicoin.Library.Epicoin.Continue = false;
                DataClient.Continue = false;
                DataServer.Continue = false;
                Epicoin.Library.Epicoin.Log = null;
                this.worker.Abort();
                this.worker = null;
                this.work = false;
                Invoke(new MethodInvoker(delegate { StopLog(); }));
            }
        }

        protected void UpdateLog()
        {
            while (this.work)
            {
                Invoke(new MethodInvoker(delegate { RefreshLog(); }));
                Thread.Sleep(200);
            }
        }

        protected void RefreshLog()
        {
            try
            {
                string msg = Epicoin.Library.Epicoin.Log.Pop();
                if (msg != null && msg != "")
                {
                    this.LogMiner.AppendText(msg);
                }
            }
            catch (Exception e)
            { Console.WriteLine(e);  } 
            this.LogMiner.Refresh();
        }

        protected void StartLog()
        {
            this.LogMiner.AppendText("[CM] Start\n");
            this.LogMiner.Refresh();
            Thread.Sleep(100);
        }

        protected void StopLog()
        {
            this.LogMiner.AppendText("[CM] Stop\n");
            this.LogMiner.Refresh();
            Thread.Sleep(100);
        }

        protected Thread logWorker;
        protected bool work = false;
        protected Thread worker = null;

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (perfcpu == null)
            {
                return;
            }
            float fcpu = perfcpu.NextValue();
            if (fcpu > cpubar.Maximum)
            {
                fcpu = cpubar.Maximum;
            }
            cpubar.Value = (int)fcpu;
            cpubar.Invalidate();
            cpubar.Update();
        }

        private void Miner_Load(object sender, EventArgs e)
        {
            this.TextBoxName.Text = Epicoin.Library.Epicoin.Wallet.Name;
            this.TextBoxAddress.Text = Epicoin.Library.Epicoin.Wallet.Address[0];
            try
            {
                perfcpu = new PerformanceCounter("Processor Information", "% Processor Time", "_Total");
            }
            catch
            {
                return;
            }
            timer1.Start();
        }

        PerformanceCounter perfcpu = null;
    }
}
