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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Net.Sockets;
using System.Net;
using System.Threading;

using MetroFramework.Forms;
using Microsoft.Win32;
namespace WindowsFormsApplication3
{
   
    
    public partial class Form1 : MetroForm
    {
        [DllImport("User32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("User32.dll")]
        public static extern void ReleaseDC(IntPtr hwnd, IntPtr dc);

        [DllImport("User32.dll", CharSet=CharSet.Auto)]
        protected static extern int SetClipboardViewer(int hWndNewViewer);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

       //private System.Windows.Forms.RichTextBox richTextBox1;

        IntPtr nextClipboardViewer;
        Thread t;
        static ManualResetEvent autoEvent = null;
        RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        private delegate void InvokeDelegate(string text);
        private delegate void PressButton();
        private delegate void changestatus(bool status);
        private delegate void ChangeIcon(string text);
       private ClipboardSender clipb = null;
        public Thread clipbthread = null;
        MyServer server;
        public  bool ricevutodalsocket ;
        public ShowActiveServer s;
        private delegate void InvokeDelegatesetImageToClip(Bitmap b);

 private delegate void InvokeDelegate2(string text);


 private delegate void InvokeDelegateAddClipBoard(List<String> listLocalPath);

 public Socket getcurrentsockinvio() {

     return clipb.clipsocket;
 
 }
 

        protected override void Dispose(bool disposing)
        {
            ChangeClipboardChain(this.Handle, nextClipboardViewer);
            if (disposing)
            {
                
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        
         
 
        }

        public void Pressbuttoninvio()
        {
            if (InvokeRequired)
            {
                Invoke(new PressButton(Pressbuttoninvio));
                return;
            }
            // dosomething with text
            sendfile();

        }


        // serve a modificare la scritta sullo stato del server
        public void DoSomething(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new InvokeDelegate(DoSomething), text);
                return;
            }
            // dosomething with text
            label3.Text = text;

        }
        public void DoSomething2(string text)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new InvokeDelegate(DoSomething2), text);
                    return;
                }
                // dosomething with text
                label3.Text = text;
                server = null;
            }
            catch (Exception ex) {

                return;
            
            }
           

        }

        public void DoStatus(bool status)
        {
            if (InvokeRequired)
            {
                Invoke(new changestatus(DoStatus), status);
                return;
            }
            // dosomething with text
            ricevutodalsocket = status;

        }


        public void ResizeIcon(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new ChangeIcon(ResizeIcon), text);
                return;
            }
            // dosomething with text

            notifyIcon1.BalloonTipText = text;
            if (this.WindowState != FormWindowState.Minimized) this.WindowState = FormWindowState.Minimized;
            else { notifyIcon1.ShowBalloonTip(30000); }
        }
        public Form1()
        {
            InitializeComponent();
            rkApp.SetValue("WindowsFormsApplication3", Application.ExecutablePath.ToString());
            TopMost = true;
            nextClipboardViewer = (IntPtr)SetClipboardViewer((int)this.Handle);
           // Thread t = new Thread(new ThreadStart(Form1.runClip));
            //t.Start();
            
            autoEvent = new ManualResetEvent(false);// setto l'evento sul quale il thread che ascolta la clipboard si setta
           // mynotifyicon = new NotifyIcon(this.components);
            
            // questi saranno da cancellare
            textBox1.Text = "1500";
                textBox2.Text="s";
                this.MaximizeBox = false;
                this.MinimumSize = new Size(380, 350);
                this.MaximumSize = new Size(380, 350);
               // this.FormBorderStyle =(FormBorderStyle) MetroFormBorderStyle.FixedSingle;
                ListView s = new ListView();
               
        }

        public static void StartForm1() { 
        
            Application.Run(new Form1());
   
        }

      
        private void button1_Click(object sender, EventArgs e)
        {
            if (server == null)
            {

                if (textBox1.Text == "")
                {

                    MessageBox.Show("devi inserire una porta.",
        "Attenzione");
                    return;
                }
                int port = Convert.ToInt32(textBox1.Text);
                if (port < 1024 || port > 63000)
                {

                    MessageBox.Show("Porta non valida.",
         "Attenzione");
                    return;

                }
                if (textBox2.Text == "")
                {


                    MessageBox.Show("devi inserire una password.",
        "Attenzione");
                    return;

                }
               
                string password = textBox2.Text;
                clipb = new ClipboardSender();
                // istanzio l'oggetto server , creato sulla porta e la password scelte dall'utente.   
                server = new MyServer();
                server.setpassw(password);
                server.setport(Convert.ToInt32(textBox1.Text));
                server.form = this;
                server.clp = clipb;
             
                t = new Thread(new ThreadStart(server.StartListening)); // il thread t è quello che fa il run del server stesso
                t.IsBackground = true;// lo mettiamo in background altrimenti rimarrebbe bloccato sull'accept o cmq su un event. 
                // è inutile ucciderlo! perchè tanto dato che resta bloccato su un evento se non gira in background da problemi. 
                t.Name = "thread connessione";
                t.Start();

            }
            else { MessageBox.Show("server gia in uso", "Attenzione"); return; }
            
        }

        

      

        private void button2_Click(object sender, EventArgs e)
        {

            if (server != null)
            {
                Console.WriteLine("thread aborted.");
                server.terminateGracefully = true;
                server.terminatecurrentsocket = true;
                server.terminatecurrentsocketMouse=true;
                server.terminatecurrentsocketKeybd = true;
                server.terminatecurrentsocketPrinc = true;
                server.MouseUDPreceiveDone.Set();
                server.receiveDone.Set();
                server.KeybdreceiveDone.Set();
                server.PrincreceiveDone.Set();
                server.CloseTheSocket();
              
                server = null;

                DoSomething("offline");
            }
            else {
                MessageBox.Show("server gia offline");
            }
           
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            Console.WriteLine(e.KeyChar.ToString());
        }
       
        void DisplayClipboardData()
        {
            try
            {
                IDataObject iData = new DataObject();
                iData = Clipboard.GetDataObject();

                if (iData.GetDataPresent(DataFormats.Rtf)) { }
                //     Console.WriteLine("ciaooo" + (string)iData.GetData(DataFormats.Rtf));
                else if (iData.GetDataPresent(DataFormats.Text))
                    Console.WriteLine((string)Clipboard.GetDataObject().GetData(DataFormats.Text));
                else
                    Console.WriteLine("[Clipboard data is not RTF or ASCII Text]" + iData.GetType().ToString());

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }


        public void retriveClipBoard(String textClipBoard)
        {
            if (InvokeRequired)
            {
                Invoke(new InvokeDelegate2(retriveClipBoard), textClipBoard);
                return;
            }
            // dosomething with text
            Clipboard.SetData(DataFormats.Text, textClipBoard);
            if (Clipboard.GetDataObject() != null)
                Console.WriteLine("Clipboard" + Clipboard.GetText(TextDataFormat.Text));
        }

        public void addToClipBoard(List<String> text)
        {
            if (InvokeRequired)
            {
                Invoke(new InvokeDelegateAddClipBoard(addToClipBoard), text);
                return;
            }
            Clipboard.Clear();
            System.Collections.Specialized.StringCollection FileCollection = new System.Collections.Specialized.StringCollection();

            foreach (string FileToCopy in text)
            {
                FileCollection.Add(FileToCopy);
            }

            byte[] moveEffect = new byte[] { 2, 0, 0, 0 };
            MemoryStream dropEffect = new MemoryStream();
            dropEffect.Write(moveEffect, 0, moveEffect.Length);

            DataObject data = new DataObject();
            data.SetFileDropList(FileCollection);
            data.SetData("Preferred DropEffect", dropEffect);

            Clipboard.Clear();
            Clipboard.SetDataObject(data, true);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            // questa funziona tratta il resize delle fisntre ! 
            // riduce la finestra ad icona e visualizza un messaggio. 
            // viene  creato un delegato che successivamente va a modificare l'avviso dell'icona. 

            if (FormWindowState.Minimized == this.WindowState)
            {

                notifyIcon1.Text = "Server";
                notifyIcon1.Icon = new Icon(SystemIcons.Application, 40, 40);
                
                notifyIcon1.BalloonTipTitle = "ServerPDS";
                notifyIcon1.BalloonTipText = "il server continuerà a funzionare, per aprire effettuare doppio click";
                notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
                // Add menu to tray icon and show it.
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(30000);
                this.Hide();
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                notifyIcon1.Visible = false;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        [STAThreadAttribute]
        public void sendfile() {

            try
            {
                if (server != null)
                {


                    if (Clipboard.ContainsText(TextDataFormat.Text))
                    {
                        if (clipb != null)
                        {
                            if (server != null && server.GetConnected())
                            {
                                clipb.textselected = (string)Clipboard.GetDataObject().GetData(DataFormats.Text);
                                clipb.valore = 0;
                                if (clipbthread != null && clipbthread.IsAlive)
                                {
                                    MessageBox.Show("Attenzione, trasferimento clipboard gia in atto, interrompere prima il trasferimento corrente");

                                }
                                else
                                {
                                    clipbthread = new Thread(new ThreadStart(clipb.sendData));
                                    clipbthread.Name = "clip thread";
                                    clipbthread.IsBackground = true;
                                    clipbthread.Start();
                                    return;

                                }
                                // Console.WriteLine("sto svegliando la senddata");
                                // autoEvent.Set();
                            }
                            else
                            {

                                MessageBox.Show("non è possibile inviare la clipboard");
                                return;
                            }
                        }
                    }
                    if (Clipboard.ContainsFileDropList())
                    {

                        if (clipb != null)
                        {
                            if (server != null && server.GetConnected())
                            {
                                clipb.filename.Clear();
                                clipb.filetotallen = 0;

                                for (int i = 0; i < Clipboard.GetFileDropList().Count; i++)
                                {
                                    clipb.valore = 1;
                                    clipb.setparam(Clipboard.GetFileDropList()[i].ToString());
                                    //}
                                }

                                clipb.SetServer(server);

                                clipb.form1 = this;


                                clipbthread = new Thread(new ThreadStart(clipb.sendData));
                                clipbthread.Name = "clip thread";
                                clipbthread.IsBackground = true;
                                clipbthread.Start();
                                return;
                                //clipbthread.Join(); 
                            }
                            else
                            {
                                MessageBox.Show("non è possibile inviare la clipboard");
                                return;
                            }
                            //}
                        }

                    }
                    if (Clipboard.ContainsImage())
                    {

                        if (clipb != null)
                        {
                            if (server.GetConnected())
                            {
                                Image m = Clipboard.GetImage();
                                clipb.SetImmage(m);
                                clipb.valore = 2;
                                clipb.SetServer(server);
                                clipb.form1 = this;
                                clipbthread = new Thread(new ThreadStart(clipb.sendData));
                                clipbthread.Name = "clip thread";
                                clipbthread.IsBackground = true;
                                clipbthread.Start();
                                return;

                            }
                        }
                    }



                }
                else
                {
                    MessageBox.Show("non hai ancora effettuato la connessione con questo server");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                MessageBox.Show("Impossibile mandare attualmente la clipboard." );
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            sendfile();
        }

       

       



        public void setImageToClip(Bitmap b)
        {
            if (InvokeRequired)
            {
                Invoke(new InvokeDelegatesetImageToClip(setImageToClip), b);
                return;
            }
            Clipboard.Clear();
            Clipboard.SetImage(b);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (server != null)
            {
                Console.WriteLine("thread aborted.");
                server.terminateGracefully = true;
                server.terminatecurrentsocket = true;
                server.terminatecurrentsocketMouse = true;
                server.terminatecurrentsocketKeybd = true;
                server.terminatecurrentsocketPrinc = true;
                server.MouseUDPreceiveDone.Set();
                server.receiveDone.Set();
                server.KeybdreceiveDone.Set();
                server.PrincreceiveDone.Set();
                server.CloseTheSocket();
            
                if (server.clp.listener != null) server.clp.listener.Close();
                server = null;

                
            }

        }
    }


    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

}






