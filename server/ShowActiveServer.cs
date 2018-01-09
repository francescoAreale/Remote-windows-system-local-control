using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Windows.Documents;
using System.Windows.Input;

namespace WindowsFormsApplication3
{
    public partial class ShowActiveServer : Form
    {
        [DllImport("User32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("User32.dll")]
        public static extern void ReleaseDC(IntPtr hwnd, IntPtr dc);
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        private delegate void InvokeDelegate2();

        private delegate void InvokeDelegate();
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);
        Form1 form;
        IntPtr nextClipboardViewer;

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            // defined in winuser.h
            //const int WM_DRAWCLIPBOARD = 0x308;
            const int WM_CHANGECBCHAIN = 0x030D;
            const int WM_CUT = 0x0300;



            switch (m.Msg)
            {
                case 0x0312:
                    
                    int id = m.WParam.ToInt32();
                    if (id == 100)
                    {

                        form.Pressbuttoninvio();
                        keybd_event(164, 0, 0x0002, 0);
                    }

                    break;
                case WM_CUT:
                    break;
                case WM_CHANGECBCHAIN:
                    if (m.WParam == nextClipboardViewer)
                        nextClipboardViewer = m.LParam;
                    else
                        SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    break;
            }
            base.WndProc(ref m);

        }
        public void Nascondi()
        {
            if (InvokeRequired)
            {
                Invoke(new InvokeDelegate2(Nascondi));
                return;
            }
            // dosomething with text
            this.Visible = false;
            UnregisterHotKey(this.Handle, 100);
        }

        public void chiudi()
        {
            if (InvokeRequired)
            {
                Invoke(new InvokeDelegate(chiudi));
                return;
            }
            // dosomething with text
            UnregisterHotKey(this.Handle, 100);
           this.Close();
            
        }
        public void Mostra()
        {
            if (InvokeRequired)
            {
                Invoke(new InvokeDelegate2(Mostra));
                return;
            }
            // dosomething with text
            this.Visible = true;
            RegisterHotKey(this.Handle, 100, 0x0001, 0x4E);
            keybd_event(162, 0, 0x0002, 0);
            keybd_event(160, 0, 0x0002, 0);

           // keybd_event(, , 0, 0);
            
        }
        public ShowActiveServer(Form1 form)
        {
            InitializeComponent();
            TopMost = true;
            TopLevel = true;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.LightGreen;
            TransparencyKey = Color.LightGreen;
            Width = SystemInformation.VirtualScreen.Width;   
            Height= SystemInformation.VirtualScreen.Height;
            
            this.form = form;
           // Height = 100;
        }

        private void ShowActiveServer_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.TopLevel = true;
        }

        private void ShowActiveServer_Paint(object sender, PaintEventArgs e)
        {
            
            System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(255, 255, 0, 0));
            pen.Width = 15;
        e.Graphics.DrawLine(pen, 0, 0, 0, Screen.PrimaryScreen.Bounds.Height);
        e.Graphics.DrawLine(pen, 0, 0, Screen.PrimaryScreen.Bounds.Width, 0);
        e.Graphics.DrawLine(pen, Screen.PrimaryScreen.Bounds.Width, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        e.Graphics.DrawLine(pen, 0, Screen.PrimaryScreen.Bounds.Height, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            //Image newImage = Image.FromFile("foto.jpg");

            // Create coordinates for upper-left corner of image.
            //float x = Screen.PrimaryScreen.Bounds.Width - newImage.Width - 25;
            float y = 30;

            // Draw image to screen.
            //g.DrawImage(newImage, x, y);
            e.Graphics.Dispose();
           
            //ReleaseDC(IntPtr.Zero, desktopPtr);

        }
    }
}
