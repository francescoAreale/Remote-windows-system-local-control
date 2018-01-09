using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Configuration;
using System.Security.Cryptography;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace WindowsFormsApplication3
{
    class MyServer
    {
        

        private IPEndPoint remoteIpEndPoint = null; 
        private byte[] msg;
        public  ManualResetEvent receiveDone = new ManualResetEvent(false);
        public ManualResetEvent KeybdreceiveDone = new ManualResetEvent(false);
        public ManualResetEvent MouseUDPreceiveDone = new ManualResetEvent(false);
        public ManualResetEvent PrincreceiveDone = new ManualResetEvent(false);
        public static string data = null;
        public  int port;
        private Thread mousethread = null;
        private Thread keybdthread = null;
        private Thread invioClipboard = null;
        private string password;
        public Form1 form;
        public Socket handler;
        public Socket listener;
        public IPAddress ipAddress;
        public string coordinateCLient;
        public ClipboardSender clp;
        public Thread clipbthread = null;
        public Thread avvioServInvio = null;
        public MyClipBoard myclp;
        public Socket SocketClipboard =null;
        public ShowActiveServer active;
        public Boolean terminateGracefully = false;
        public Boolean terminatecurrentsocket = false;
        public Boolean terminatecurrentsocketMouse = false;
        public Boolean terminatecurrentsocketKeybd = false;
        public Boolean terminatecurrentsocketPrinc = false;
        public MouseUDP mouse = null;
        public KeybdTCP keybd = null;
       
        public  void RunSecondForm() {

            active= new ShowActiveServer(form);
            Application.Run(active);
        }
      
        bool connected = false;
        Thread disegna;
        private string Encrypt(string clearText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public bool GetConnected() {

            return connected;
        }

        public string getpassw()
        {

            return password;

        }
        public void setpassw(string password)
        {

            this.password = password;

        }
        public int getport() {

            return port;
        
        }
        public void setport(int port) {

            this.port = port;
        
        }
        public MyServer()
        {
           /* autoMouse = new ManualResetEvent(false);
            autoRecv = new ManualResetEvent(false);
            autoSender = new ManualResetEvent(false);
            autoTast = new ManualResetEvent(false);*/
        }
      
        public static string resolve() {

            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    return localIP;
                }
            }
            return localIP;      
        }

        private  bool flag = true;

        public void CloseTheSocket() {

          
           // if (handler != null) { handler.Close(); handler = null; }
           if (listener != null) { listener.Close(); listener = null; }
        
        }

        public Form2 form2;
        public void InvokeMethod()
        {
            form2 = new Form2();
            //This function will be on main thread if called by Control.Invoke/Control.BeginInvoke
            Application.Run(form2);
        }

        const int bytesperlong = 4; // 32 / 8
        const int bitsperbyte = 8;
        public static bool SetKeepAlive(Socket sock, ulong time, ulong interval)
        {
            try
            {
                // resulting structure
                byte[] SIO_KEEPALIVE_VALS = new byte[3 * bytesperlong];

                // array to hold input values
                ulong[] input = new ulong[3];

                // put input arguments in input array
                if (time == 0 || interval == 0) // enable disable keep-alive
                    input[0] = (0UL); // off
                else
                    input[0] = (1UL); // on

                input[1] = (time); // time millis
                input[2] = (interval); // interval millis

                // pack input into byte struct
                for (int i = 0; i < input.Length; i++)
                {
                    SIO_KEEPALIVE_VALS[i * bytesperlong + 3] = (byte)(input[i] >> ((bytesperlong - 1) * bitsperbyte) & 0xff);
                    SIO_KEEPALIVE_VALS[i * bytesperlong + 2] = (byte)(input[i] >> ((bytesperlong - 2) * bitsperbyte) & 0xff);
                    SIO_KEEPALIVE_VALS[i * bytesperlong + 1] = (byte)(input[i] >> ((bytesperlong - 3) * bitsperbyte) & 0xff);
                    SIO_KEEPALIVE_VALS[i * bytesperlong + 0] = (byte)(input[i] >> ((bytesperlong - 4) * bitsperbyte) & 0xff);
                }
                // create bytestruct for result (bytes pending on server socket)
                byte[] result = BitConverter.GetBytes(0);
                // write SIO_VALS to Socket IOControl
                sock.IOControl(IOControlCode.KeepAliveValues, SIO_KEEPALIVE_VALS, result);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }



        public void ReceiveCallback(IAsyncResult ar)
        {
            

            try
            {
                StateObject state = (StateObject)ar.AsyncState;

                // Read data from the remote device.
                int bytesRead = state.workSocket.EndReceive(ar);
                
                

                data = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
                if (bytesRead == 0)
                {
                 
                    if (active != null) active.Nascondi();
                    if (handler != null) handler.Close();

                    Console.WriteLine("il client ha chiuso la connessione");

                    terminatecurrentsocketPrinc = true;
                    PrincreceiveDone.Set();
                  
                    return;
                }
                if (data.IndexOf("PASS") > -1)
                {
                    remoteIpEndPoint = handler.RemoteEndPoint as IPEndPoint;
                    Console.WriteLine(data.Substring(4, data.Length - 9));
                    string pass = this.Encrypt(password);
                    Console.WriteLine("la password del server è {0}", pass);
                    if (pass == data.Substring(4, data.Length - 9))
                    {
                        // MessageBox.Show("Nuovo host collegato.","Attenzione");
                        msg = Encoding.ASCII.GetBytes("+OK\r\n");
                        form.DoSomething("I am connected to " + remoteIpEndPoint.Address + " on port number " + remoteIpEndPoint.Port);
                        handler.Send(msg);
                        
                    }
                    else
                    {
                        msg = Encoding.ASCII.GetBytes("-ERR\r\n");
                        form.DoSomething("tentativo di autenticazione dall'hos " + remoteIpEndPoint.Address + " fallito");
                        handler.Send(msg);
                    }
                }

                if (data.IndexOf("-ERR\r\n") > -1)
                {
                    clp.err = true;
                }

                if (data.IndexOf("CONTR") > -1)
                {
                    msg = Encoding.ASCII.GetBytes("+OK\r\n");
                    form.DoSomething("attualmente in uso da host" + remoteIpEndPoint.Address);
                    handler.Send(msg);

                    int bytesRec = handler.Receive(state.buffer);
                    coordinateCLient = Encoding.ASCII.GetString(state.buffer, 0, bytesRec);
                    Console.WriteLine("le coordinate sono " + coordinateCLient);
                    connected = true;
                    if (disegna == null)
                    {
                        disegna = new Thread(new ThreadStart(RunSecondForm));
                        disegna.Start();
                    }
                    else
                    {
                        if (active != null) active.Mostra();

                    }
                 
                    //mouse.mouserun();
                }

                // Console.WriteLine("********************" + data);

                if (data.IndexOf("SVEGLIA") > -1)
                {
                    if (disegna == null)
                    {
                        disegna = new Thread(new ThreadStart(RunSecondForm));
                        disegna.Start();
                    }
                    else
                    {
                        if (active != null) active.Mostra();

                    }
                    form.DoSomething("MI sono svegliato sono il server corrente");
                    connected = true;
                    form.ResizeIcon("attualmente in uso da Host " + remoteIpEndPoint.Address);
                    
                }

                if (data.IndexOf("DORMI") > -1)
                {
                    form.DoSomething("MI sono addormentato non  sono il server corrente");
                    if (active != null) active.Nascondi();
                    connected = false;
                    // form.ResizeIcon("Server non in uso . ");
                    
                }

                PrincreceiveDone.Set();  
            }
            catch (Exception ex)
            {
                if (handler != null) { handler.Close(); handler = null; }

                if (keybd.keybsock != null) { keybd.keybsock.Close(); keybd.keybsock = null; }
                if (clp.clipsocket != null) { clp.clipsocket.Close(); clp.clipsocket = null; }
                if (myclp.sockClipBoard != null) { myclp.sockClipBoard.Close(); myclp.sockClipBoard = null; }
                if (active != null) { active.chiudi(); active = null; disegna = null; }
                terminatecurrentsocketPrinc = true;
                PrincreceiveDone.Set();
                

            }
               
        
        
        }
        [STAThreadAttribute]
        public void StartListening()
        {
             mouse =null;
             keybd=null;
            byte[] bytes = new Byte[1024];     
            Console.WriteLine("sono bindato all'indirizzo " + resolve()+"porta " + port);
            form.DoSomething("sono in ascolto all'indirizzo " + resolve());        
            ipAddress = IPAddress.Parse(resolve());
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
            listener = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp );

            try
            {
                mousethread = null;
                keybdthread = null;
                invioClipboard = null;
                avvioServInvio = null;

                listener.Bind(localEndPoint);
                listener.Listen(1);
                terminateGracefully = false;
                while (!terminateGracefully)
                {



                    Console.WriteLine("Waiting for a connection...");
                    Console.WriteLine("la password è " + password);
                    // Program is suspended while waiting for an incoming connection.
                    handler = listener.Accept();
                    SetKeepAlive(handler, 90, 100);
                    //  handler.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, true);
                    /***************************** CREAZIONE DEI THREAD. ****************************************/
                    terminatecurrentsocketPrinc = false;
                    StateObject state = new StateObject();
                    state.workSocket = handler;
                    if (mousethread == null)
                    {
                        mouse = new MouseUDP();
                        mouse.getServer(this);
                        // mouse.setevent)(autoMouse);
                        mousethread = new Thread(new ThreadStart(mouse.mouserun));
                        mousethread.IsBackground = true; // ricorda che il thread in background viene terminato immediatamente.
                        // al contrario di un thread in primo piano che deve attendere il completamento dell'operazione.
                        // mettendo il thread in background quando si chiude la finestra viene termianto tutto automaticamente. 

                        mousethread.Name = "mouse thread";
                        mousethread.Start();
                    }
                    if (keybdthread == null)
                    {

                        keybd = new KeybdTCP();
                        keybd.SetServer(this);
                        keybdthread = new Thread(new ThreadStart(keybd.keybdrun));
                        keybdthread.Name = "keybd thread";
                        keybdthread.IsBackground = true;
                        keybdthread.Start();
                        flag = false;

                    }
                    if (avvioServInvio == null)
                    {
                        clp.SetServer(this);
                        clp.form1 = form;
                        avvioServInvio = new Thread(new ThreadStart(clp.setnewserver));
                        avvioServInvio.Name = "Server per invio.";
                        avvioServInvio.IsBackground = true;
                        avvioServInvio.Start();

                    }
                    if (invioClipboard == null)
                    {
                        myclp = new MyClipBoard();
                        myclp.setServer(this);
                        myclp.setForm(form);
                        invioClipboard = new Thread(new ThreadStart(myclp.receiveDataClipBoard));
                        invioClipboard.Name = "server ricezione myclipboard";
                        invioClipboard.IsBackground = true;
                        invioClipboard.Start();
                    }
                    /**********************************************************/

                    connected = false;
                    /*  autoMouse.Reset();
                      autoRecv.Reset();
                      autoTast.Reset();*/
                    bool flagerrore = false;
                    while (!terminatecurrentsocketPrinc)
                    {

                        Console.WriteLine("sono connesso ");
                        // An incoming connection needs to be processed.
                        PrincreceiveDone.Reset();
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                        PrincreceiveDone.WaitOne();
                    }
                    if (handler != null) handler.Close();

                    form.DoSomething("Online in attesa di Host");

                    // chiudo i socket per far tornare bene le callback


                }
                if (listener != null) { listener.Close(); listener = null; }

            }
           
            catch (Exception e)
            {

                Console.WriteLine(e.ToString());
                if (handler != null) { handler.Close(); handler = null; }
                if (listener != null) { listener.Close(); listener = null; }
                Console.WriteLine("errore nel socket uccido tutti i thread e ricomincio.");
                if (mousethread != null) mouse.closeSocket();
                if (keybdthread != null) keybd.closeSocket();
                if (avvioServInvio != null) clp.closeSocket();
                if (invioClipboard != null) myclp.closeSocket();
                mousethread = null;
                keybdthread = null;
                invioClipboard = null;
                avvioServInvio = null;
                if(form!=null) form.DoSomething2("Offline");
            }
     
            }

        }

      

      
}
