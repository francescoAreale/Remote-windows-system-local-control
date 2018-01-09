using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Runtime.InteropServices;  
using System.Security.Principal;  
using System.Diagnostics;  



namespace Progetto_Malnati
{
    public partial class Form2 : Form
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        //Keyboard API constants
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYUP = 0x0101;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYUP = 0x0105;
        private const int WM_SYSKEYDOWN = 0x0104;

        public int coordinataX;
        public int coordinataY;
        public MyClient client;
        public System.Net.Sockets.UdpClient sockMouse;
        public IPEndPoint localEndPoint;
        public IPAddress ipAddress;
        public int port;
        public Socket sockTastiera,sockClipBoard;
        public IPEndPoint remoteEP, remoteEPClipBoard;
        MyClipBoard clip;
        public byte[] msg = Encoding.ASCII.GetBytes("+OK");
        ClipboardSender clipb;
        public Thread tClip=null;
        static ManualResetEvent autoEvent = null;
        private delegate void InvokeDelegate(string text);
       
        private Thread clipbthread = null;
        public Thread invioClip;
        Form1 form;

        public Form2(MyClient client, Form1 form)//non è asincrono!!!!!!!!!!!!!!!!
        {
            InitializeComponent();
            this.form = form;

            this.Text="Mouse Area";
            this.client = client;

             //creo ed inizializzo il socket del mouse
            sockMouse = new System.Net.Sockets.UdpClient();
            


           
        }

        public void Caluculate(int i)
        {
            double pow = Math.Pow(i, i);
        }
        public void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var backgroundWorker = sender as BackgroundWorker;
            for (int j = 0; j < 100000; j++)
            {
                Caluculate(j);
                backgroundWorker.ReportProgress((j * 100) / 100000);
            }
        }

     

        public void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // TODO: do something with final calculation
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            
            /*Handle the mouse wheel here*/
            base.OnMouseWheel(e);
            if (e.Delta > 0)
            {
                //Console.WriteLine("su" + e.Delta.ToString());
                String msg = "SU"+e.Delta.ToString();
                byte[] data = Encoding.UTF8.GetBytes(msg);
                sockMouse.Send(data, data.Length, client.getCurrentEndpoint());
            }
            else
            {
                //Console.WriteLine("giù" + e.Delta.ToString());
                String msg = "GIU" + e.Delta.ToString();
                byte[] data = Encoding.UTF8.GetBytes(msg);
                sockMouse.Send(data, data.Length, client.getCurrentEndpoint());
            }
        }

      
        private void sendMouseData(object sender, MouseEventArgs e)
        {
            
            coordinataX = Cursor.Position.X;
            coordinataY = Cursor.Position.Y;

            String msg = coordinataX.ToString() + "," + coordinataY.ToString()+"\0";
          
             byte[] data = Encoding.UTF8.GetBytes(msg);
             sockMouse.Send(data, data.Length, client.getCurrentEndpoint());
            

        }

    

        private void Form2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            String msg = "d.click";
            byte[] data = Encoding.UTF8.GetBytes(msg);
            sockMouse.Send(data, data.Length, client.getCurrentEndpoint());
        }

        private void Form2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                
                String msg = "trascina";
                byte[] data = Encoding.UTF8.GetBytes(msg);
                sockMouse.Send(data, data.Length, client.getCurrentEndpoint());
            }
            else {
               
                String msg = "r.click";
                byte[] data = Encoding.UTF8.GetBytes(msg);
                sockMouse.Send(data, data.Length, client.getCurrentEndpoint());
            }
        }

        private void Form2_MouseUp(object sender, MouseEventArgs e)
        {
                String msg = "click";
               // Console.WriteLine("BYTECODE : " + e.Button);
                byte[] data = Encoding.UTF8.GetBytes(msg);
                sockMouse.Send(data, data.Length, client.getCurrentEndpoint());
            
        }
      

        private void Form2_Load(object sender, EventArgs e)
        {           
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            Program.kh = new KeyboardHook(null);

            Program.kh.KeyIntercepted += new KeyboardHook.KeyboardHookEventHandler(kh_KeyIntercepted);
            
        }

        void kh_KeyIntercepted(KeyboardHook.KeyboardHookEventArgs e)
        {
             try
                {
            /** Invia l' evento tastiera soltanto se form2 è in primo piano ***/
            if (this.IsActive(this.Handle) == false)
            {
                Program.kh.AllowKey = true;
                return;
            }
           // Console.WriteLine(e.KeyName);
            //ds.Draw(e.KeyName);
            if (e.WParam == (IntPtr)WM_KEYUP || e.WParam == (IntPtr)WM_SYSKEYUP)
            {
                byte[] msg = new byte[2];
                msg[0] = (byte)e.KeyCode;
                int bytesSent;
                msg[1] = (byte)'U';
               
                bytesSent = client.getCurrentKeyboardSocket().Send(msg);
                             
            }

            if(e.WParam == (IntPtr)WM_KEYDOWN || e.WParam == (IntPtr)WM_SYSKEYDOWN)
            {
                byte[] msg = new byte[2];
                msg[0] = (byte)e.KeyCode;
                msg[1] = (byte)'D';
            
               
                    // Send the data through the socket.
                    client.getCurrentKeyboardSocket().Send(msg);
                    // Console.WriteLine("Testo: " + e.KeyData);
                }
             }
                    catch(SocketException ex){
                        MessageBox.Show("Il server ha terminato la connessione sock ex");
                        client.closeAllSocket();
                        form.changeStatusListView(" ",true);
                        this.Close();
                        return;
                    }
                catch (Exception ex)
                {
                   MessageBox.Show("Il server ha terminato la connessione general ex");
                   form.changeStatusListView(" ", true);
                    client.closeAllSocket();
                    this.Close();
                    return;
                }

        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            
            this.Close();
        }





        public bool IsActive(IntPtr handle)
        {
            IntPtr activeHandle = GetForegroundWindow();
            return (activeHandle == handle);
        }
      


    }
}
