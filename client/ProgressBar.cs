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

namespace Progetto_Malnati
{
    public partial class ProgressBar : Form
    {
        public ProgressBar()
        {
            InitializeComponent();
        }

        public delegate void updateBar(int value, int tot, string nomefile);
        public updateBar delegateUpdateBar;

        private int perc;
        private bool showdialog = false;

        public void showdialogset(bool s) {

            this.showdialog = s;
        }
        public void UpdateProgress(int progress, int tot, string nomefile)
        {


            if (progressBar1.InvokeRequired)
            {
                progressBar1.BeginInvoke(new Action(() =>
                {
                    perc += progress; progressBar1.Value += progress;
                    if (tot < (1024 * 1024))
                    {
                        label1.Text = " UPLoad File " + perc / (1024) + "/ " + tot / (1024) + " KB" + " Nome File: " + nomefile;
                     }
                    else
                        label1.Text = " UPLoad File " + perc / (1024 * 1024) + "/ " + tot / (1024 * 1024) + " MB" + " Nome File: " + nomefile;
                }));
            }
            else
            {
                perc += progress; progressBar1.Value += progress; label1.Text = " UPLoad File " + perc / (1024 * 1024) + "/ " + tot / (1024 * 1024) + " MB" + " Nome File: " + nomefile; ;
            }
            if (tot < (1024 * 1024))
                Thread.Sleep(300);
        }


        public void SetBar(int progress)
        {


            if (progressBar1.InvokeRequired)
                progressBar1.BeginInvoke(new Action(() => progressBar1.Maximum = progress));
            else
                progressBar1.Maximum = progress;


        }


        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            //  base.OnFormClosing(e);

            if (e.CloseReason == CloseReason.ApplicationExitCall) return;
            if (e.CloseReason == CloseReason.FormOwnerClosing) return;

            if (e.CloseReason == CloseReason.UserClosing && !showdialog)
            // Confirm user wants to close
            {
                switch (MessageBox.Show(this, "Are you sure you want to close?", "Closing", MessageBoxButtons.YesNo))
                {
                    case DialogResult.No:
                        e.Cancel = true;
                        break;
                    default:
                        this.DialogResult = DialogResult.OK;
                        break;
                }

            }
        }
    }
}
