using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Epicoin.Library;
using Epicoin.Library.Blockchain;
using Epicoin.Library.Container;
using Epicoin.Library.Net;
using Epicoin.Library.Net.Server;
using Epicoin.Library.Net.Client;
using System.Net;

namespace EpicoinGraphics
{
    public partial class Serveur : Form
    {
        public Serveur()
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
            Epicoin.Library.Epicoin.ImportChain();
            if (Epicoin.Library.Epicoin.Coin == null)
            {
                Epicoin.Library.Epicoin.Init();
            }
            Epicoin.Library.Epicoin.Server = new Server(Epicoin.Library.Epicoin.Port, Epicoin.Library.Epicoin.Coin);

            InitializeComponent();

        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Epicoin.Library.Epicoin.Continue = false;
            DataClient.Continue = false;
            DataServer.Continue = false;
            StopServ();
            Epicoin.Library.Epicoin.ExportChain();
            Epicoin.Library.Epicoin.ExportWallet();
            Application.Exit();
        }

        protected void StartServClick(object sender, EventArgs e)
        {
            StartServ();
        }

        protected void StopServClick(object sender, EventArgs e)
        {
            StopServ();
        }

        protected void StartServ()
        {
            Epicoin.Library.Epicoin.Server = new Server(Epicoin.Library.Epicoin.Port, Epicoin.Library.Epicoin.Coin);
            Epicoin.Library.Epicoin.Continue = true;
            DataClient.Continue = true;
            DataServer.Continue = true;
            this.block = new Thread(Epicoin.Library.Epicoin.CreateBlock) { Priority = ThreadPriority.Highest };
            this.server = new Thread(Epicoin.Library.Epicoin.Server.Start) { Priority = ThreadPriority.Highest };
            this.saveChain = new Thread(Epicoin.Library.Epicoin.SaveBlockchain) { Priority = ThreadPriority.BelowNormal };
            
            
            this.block.Start();
            this.server.Start();
            Thread.Sleep(1000);
            this.saveChain.Start();
            Epicoin.Library.Epicoin.Client = new Client(IPAddress.Loopback.ToString(), Epicoin.Library.Epicoin.Port);
        }

        protected void StopServ()
        {
            Epicoin.Library.Epicoin.Continue = false;
            DataClient.Continue = false;
            DataServer.Continue = false;
            Epicoin.Library.Epicoin.ExportChain();
            Epicoin.Library.Epicoin.ExportWallet();
            this.block = null;
            this.server = null;
            this.saveChain = null;
            Epicoin.Library.Epicoin.Server = null;
            Epicoin.Library.Epicoin.Client = null;
        }

        protected Thread block = null;
        protected Thread server = null;
        protected Thread saveChain = null;

        private void Serveur_Load(object sender, EventArgs e)
        {
            this.TextBoxName.Text = Epicoin.Library.Epicoin.Wallet.Name;
            this.TextBoxAddress.Text = Epicoin.Library.Epicoin.Wallet.Address[0];
            timerAmount.Start();
            timerServeur.Start();
        }

        private void timerAmount_Tick(object sender, EventArgs e)
        {
            if (Epicoin.Library.Epicoin.Continue)
            {
                try
                {
                    this.EpicoinAmount.Text = Epicoin.Library.Epicoin.Wallet.TotalAmount().ToString();
                }
                catch (Exception)
                {
                    this.EpicoinAmount.Text = "Server offline";
                }
                
            }
            else
            {
                this.EpicoinAmount.Text = "Server offline";
            }
            this.EpicoinAmount.Refresh();
        }

        private void timerServeur_Tick(object sender, EventArgs e)
        {
            bool datastatus = DataClient.Continue;
            bool minestatus = DataClient.Continue;
            bool transactionstatus = DataClient.Continue;

            this.ServeurDataStatus.Text = datastatus.ToString();
            this.ServeurMinerStatus.Text = minestatus.ToString();
            this.ServeurTransStatus.Text = transactionstatus.ToString();
            this.ServeurDataStatus.Refresh();
            this.ServeurMinerStatus.Refresh();
            this.ServeurTransStatus.Refresh();

            this.ChainIsValid.Text = Epicoin.Library.Epicoin.Coin.IsvalidChain().ToString();
            this.ChainIsValid.Refresh();
            this.ChainPending.Text = (Epicoin.Library.Epicoin.Coin.Pending.Count + (Epicoin.Library.Epicoin.Coin.BlockToMines.Count * Block.nb_trans)).ToString();
            this.ChainPending.Refresh();
            
            Block last = Epicoin.Library.Epicoin.Coin.GetLatestBlock();
            this.ChainLenght.Text = Epicoin.Library.Epicoin.Coin.Chainlist.Count.ToString();
            this.ChainLenght.Refresh();
            this.ChainLastIndex.Text = last.Index.ToString();
            this.ChainLastIndex.Refresh();
            this.ChainLastHash.Text = last.Hashblock.ToString();
            this.ChainLastHash.Refresh();
            this.ChainDifficulty.Text = Epicoin.Library.Epicoin.Coin.Difficulty.ToString();
            this.ChainDifficulty.Refresh();

            this.NextBlockBar.Value = Epicoin.Library.Epicoin.Coin.Pending.Count * 100 / 3;
            this.NextBlockBar.Invalidate();
            this.NextBlockBar.Update();
        }

        protected void SendTransactionClick(object sender, EventArgs e)
        {
            this.TransactionLog.Text = "";
            bool error = false;
            string ToAddress = this.ToAddressTrans.Text;
            string Samount = this.AmountTrans.Text;
            if (ToAddress == "" || Samount == "")
            {
                error = true;
            }
            int amount = 0;
            if (!error)
            {

                try
                {
                    amount = int.Parse(Samount);
                    if (amount <= 0)
                    {
                        throw new Exception();
                    }
                }
                catch
                {
                    error = true;
                }
            }

            string display = "";
            if (!error)
            {
                List<DataTransaction> ltrans = Epicoin.Library.Epicoin.Wallet.GenTransactions(amount, ToAddress);
                
                foreach (var trans in ltrans)
                {
                    display += Epicoin.Library.Epicoin.Client.SendTransaction(trans) + "\n";
                }
            }

            this.ToAddressTrans.Text = "";
            this.ToAddressTrans.Refresh();
            this.AmountTrans.Text = "";
            this.AmountTrans.Refresh();
            this.TransactionLog.Text = display;
            this.TransactionLog.Refresh();

        }
    }
}
