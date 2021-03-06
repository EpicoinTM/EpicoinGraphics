﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Epicoin.Library;
using Epicoin.Library.Container;
using Epicoin.Library.Net.Client;
using Epicoin.Library.Blockchain;

namespace EpicoinGraphics
{
    public partial class User : Form
    {
        public User()
        {
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
            Epicoin.Library.Epicoin.ExportWallet();
            Epicoin.Library.Epicoin.Continue = false;
            Environment.Exit(0);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            RefreshChain();
        }

        protected void RefreshChainClick(object sender, EventArgs e)
        {
            RefreshChain();
        }

        protected void RefreshChain()
        {
            DataChainStats stats = Epicoin.Library.Epicoin.Client.GetChainStats();
            if (stats != null)
            {
                this.ChainLenght.Text = stats.Lenght.ToString();
                this.ChainLenght.Refresh();
                this.ChainLastIndex.Text = stats.LastIndex.ToString();
                this.ChainLastIndex.Refresh();
                this.ChainLastHash.Text = stats.LastBlockHash;
                this.ChainLastHash.Refresh();
                this.ChainDifficulty.Text = stats.Difficulty.ToString();
                this.ChainDifficulty.Refresh();

                this.EpicoinAmount.Text = Epicoin.Library.Epicoin.Wallet.TotalAmount().ToString();
                this.EpicoinAmount.Refresh();
            }
            else
            {
                Console.WriteLine("Error");
            }
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

        private void User_Load(object sender, EventArgs e)
        {
            this.TextBoxName.Text = Epicoin.Library.Epicoin.Wallet.Name;
            this.TextBoxAddress.Text = Epicoin.Library.Epicoin.Wallet.Address[0];
            timer1.Start();
        }
    }
}
