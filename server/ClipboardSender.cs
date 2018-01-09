using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Drawing;
namespace WindowsFormsApplication3
{
    class ClipboardSender
    {
        public Socket clipsocket;
        public MyServer server = null;
        private string nomefile;
        private long dim;
        public string textselected;
        public int valore; // controlla se è testo o file.
        private ManualResetEvent autoEvent;
        private string path;
        public List<string> filename = new List<string>();
        public long filetotallen = 0;
        private Thread backgroundThread;
        private bool erroreTrasferimento = false;
        public Form1 form1;
        public bool err = false;
        bool flagNumber = true;
        public Socket listener;
        System.Drawing.Image m;

        public void SetServer(MyServer server)
        {

            this.server = server;
        }

        public void SetImmage(Image m )
        {

            this.m = m;
        }

        
        public void setparam(string path)
        {

            filename.Add(path);

        }
        public void setevent(ManualResetEvent autoEvent)
        {

            this.autoEvent = autoEvent;

        }
        public void setnewserver()
        {
            try
            {
                IPEndPoint localEndPoint = new IPEndPoint(server.ipAddress, server.port + 1);
                listener = new Socket(AddressFamily.InterNetwork,
                 SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEndPoint);
                listener.Listen(1);
                while (!server.terminateGracefully)
                {
                    clipsocket = listener.Accept();
                }
                if (listener != null) listener.Close();
            }
            catch (Exception ex) {

                if (clipsocket != null) clipsocket.Close();
                if (listener != null) listener.Close();
            }
        }

        public void closeSocket()
        {

            flagNumber = false;
            if (clipsocket!=null) clipsocket.Close();
            if (listener != null) listener.Close();
            listener = null;
            clipsocket = null;
            //autoEvent.Set();

        }


        public void sendData()
        {

            byte[] msg = new byte[1024];
            byte[] buffer = new byte[1024 * 1024];
            byte[] conferma = new byte[1024];

            try
            {
                form1.ResizeIcon("invio in corso");
                if (this.valore == 1)
                {

                    for (int i = 0; i < filename.Count; i++)
                    {

                        if (!Directory.Exists(filename[i]))
                        {

                            int result = this.SendFile(filename[i]);
                            if (result == -2)
                            {
                                // clipsocket.Send(Encoding.ASCII.GetBytes("-ERR\r\n"));

                                Console.WriteLine("errore nella send data.");
                                filename.RemoveRange(0, filename.Count);
                                return;
                            }
                            else if (result == -1)
                            {
                                Console.WriteLine("errore nella send data.");
                                filename.RemoveRange(0, filename.Count);
                                return;
                            }
                            clipsocket.Receive(conferma);
                        }
                        else
                        {
                            if (this.SendDir(filename[i]) < 0)
                            {
                                Console.WriteLine("errore nella send dir.");
                                filename.RemoveRange(0, filename.Count);
                                return;
                            }
                        }
                    }
                    Console.WriteLine("Il client ha ricevuto il file");
                    msg = Encoding.ASCII.GetBytes("END");
                    clipsocket.Send(msg);
                    Console.WriteLine("Invio END al client");

                    filename.RemoveRange(0, filename.Count);
                }


                if (this.valore == 0)
                {

                    clipsocket.Send(Encoding.ASCII.GetBytes("T"));
                    Console.WriteLine("Il testo selezionato è:  " + textselected);
                    Encoding enc = Encoding.GetEncoding("iso-8859-1");
                    msg = enc.GetBytes(textselected);
                    clipsocket.Send(msg);

                }

                if (this.valore == 2)
                {

                    clipsocket.Send(Encoding.ASCII.GetBytes("I"));
                    SendVarData(clipsocket, m);


                }
            }
            catch (SocketException ex) {

                Console.WriteLine(ex.ToString());
               if (backgroundThread != null)
                {
                    backgroundThread.Abort();
                    backgroundThread.Join();
                }
            
            }
        }

       

        private static int SendVarData(Socket s, Image m )
        {
            ImageConverter converter = new ImageConverter();
            byte[] data = (byte[])converter.ConvertTo(m, typeof(byte[]));
             
            int total = 0;
            int size = data.Length;
            int dataleft = size;
            int sent;

            byte[] datasize = new byte[4];
            datasize = BitConverter.GetBytes(size);
            sent = s.Send(datasize);

            while (total < size)
            {
                sent = s.Send(data, total, dataleft, SocketFlags.None);
                total += sent;
                dataleft -= sent;
            }
            return total;
        }
        public int SendDir(string nameDir)
        {

            try
            {
                byte[] buffer = new byte[1024 * 1024];
                byte[] conferma = new byte[1024];
                byte[] msg = Encoding.ASCII.GetBytes("DIR"); 
                clipsocket.Send(msg);
                clipsocket.Receive(conferma);
                byte[] DirNameByte = Encoding.ASCII.GetBytes(new FileInfo(nameDir + @"\").Directory.Name);
                byte[] DirNameLen = BitConverter.GetBytes(DirNameByte.Length);
                byte[] clientData = new byte[4 + DirNameByte.Length];
                DirNameLen.CopyTo(clientData, 0);
                DirNameByte.CopyTo(clientData, 4);
                clipsocket.Send(clientData);
                clipsocket.Receive(conferma);
                string confermastringa = Encoding.ASCII.GetString(conferma);
                if (confermastringa.IndexOf("+OK") > -1)
                {
                    string[] fileEntries = Directory.GetFiles(nameDir);
                    string[] subdirEntries = Directory.GetDirectories(nameDir);

                    foreach (string filename in fileEntries)
                    {
                        Console.WriteLine("mando il file");
                       if (SendFile(filename)<0){

                           return -1;
                       
                       }
                        clipsocket.Receive(conferma);
                    }

                    foreach (string dirname in subdirEntries)
                    {
                        Console.WriteLine("mando la cartella");
                        if (SendDir(dirname) < 0)
                        {
                            Console.WriteLine("errore nella sendDir");
                            return -1;
                        }
                    }


                    msg = Encoding.ASCII.GetBytes("ENDDIR");
                    clipsocket.Send(msg);
                    Console.WriteLine("ho mandato EndDir");
                    clipsocket.Receive(conferma);

                }
                return 1;
            }
            catch (Exception ex)
            {

                Console.WriteLine("si è verificato un errore.");
                return -1;
            }
            
        }
        public int SendFile(string namefile)
        {
            try{
            byte[] msg = new byte[1024];
            byte[] buffer = new byte[1024 * 1024];
            byte[] conferma = new byte[1024];

            

                FileInfo f = new FileInfo(namefile);
                // Console.WriteLine("ho trovato un file di dimensione" + f.Length);
                nomefile = f.Name;
                dim = f.Length;
                if (dim > (100 * 1024 * 1024))
                {

                    DialogResult result = MessageBox.Show("You try to copy or cut a big file bigger than 100 MB , are you sure? the operation may take a long time", "Warning",
                     MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
                    if (result == DialogResult.Cancel)
                    {

                        return -2;

                    }
                }
                msg = Encoding.ASCII.GetBytes("F");
                clipsocket.Send(msg);

                byte[] fileNameByte = Encoding.ASCII.GetBytes(this.nomefile);
                byte[] fileNameLen = BitConverter.GetBytes(fileNameByte.Length);
                byte[] filedim = BitConverter.GetBytes(this.dim);
                //   Console.WriteLine("sto creando le informazioni da mandare al client");
                byte[] clientData = new byte[4 + fileNameByte.Length + filedim.Length];
                fileNameLen.CopyTo(clientData, 0);
                fileNameByte.CopyTo(clientData, 4);
                filedim.CopyTo(clientData, 4 + fileNameByte.Length);

                    clipsocket.Send(clientData);


                    clipsocket.Receive(conferma);
                    
                string confermastringa = Encoding.ASCII.GetString(conferma);
                if (confermastringa.IndexOf("+OK") > -1)
                {

                    //      Console.WriteLine(" inizio lo stream del file");

                    FileStream fileStream = new FileStream(namefile, FileMode.Open, FileAccess.Read);

                    int length = (int)fileStream.Length;  // get file length
                    // create buffer
                    int count;
                    double d;// actual number of bytes read
                    long sum = dim;                        // total number of bytes read
                    // Thread.Sleep(100);
                    // io questo coso non è che lo abbia tanto capito.
                    if (dim > (1024 * 1024))
                    {
                        d = (((double)dim) / (double)(1024 * 1024));
                    }
                    else
                    {
                        d = 1;
                    }

                    /************************************ ISTANZIO LA FORM PER LA SEND DEL FILE *****************/
                    // questa inzialmente era fatta allocando un altro thread che si occupava di aggiornare la 
                    // barra. Da rivedere è l'utilizzo della percentuale. Da mettere Lato client. Ma al ricevitore?

                    Form2 form2 = new Form2();
                    //
                   err = false;
                   erroreTrasferimento = false;
                    backgroundThread = new Thread(
               new ThreadStart(() =>
               {
                   try
                   {
                       while (((count = fileStream.Read(buffer, 0, 1024 * 1024)) > 0) && !err)
                       {
                          // form2.SetBar((int)sum);
                           clipsocket.Send(buffer, count, SocketFlags.None);
                           dim = dim - count;
                           //  Console.WriteLine(dim);
                           long count2 = count;
                          form2.UpdateProgress(count, (int)sum, nomefile);

                           /*** aggiorno la progress bar***/
                       }

                       fileStream.Close();

                       // Close the dialog if it hasn't been already
                       if (form2.InvokeRequired)
                           form2.BeginInvoke(new Action(() => { form2.showdialogset(true); form2.Close(); form2.showdialogset(false); }));
                       Console.WriteLine("sto ammazzando il thread().");
                       
                       //clipsocket.Send(Encoding.ASCII.GetBytes("-ERR\r\n"));
                       // Reset the flag that indicates if a process is currently running
                   }
                   catch (SocketException ex)
                   {

                       erroreTrasferimento = true;
                       // fileStream.Close();
                       if (form2.InvokeRequired)
                           form2.BeginInvoke(new Action(() => { form2.showdialogset(true); form2.Close(); form2.showdialogset(false); }));
                       if(clipsocket!=null)clipsocket.Close();
                       Console.WriteLine("sto ammazzando il thread().");
                       fileStream.Close();
                       return;

                   }
                    catch (Exception ex)
                    {
                        erroreTrasferimento = true;
                        // fileStream.Close();
                        if (form2.InvokeRequired)
                            form2.BeginInvoke(new Action(() => { form2.Close(); }));

                        Console.WriteLine("errore");
                        Console.WriteLine(ex.ToString() + ex.Message);
                    }
               }
           ));

                    // Start the background process thread
                    backgroundThread.Start();

                    // form2.ShowDialog();
                    // qui sto curando il caso in cui chiudo la finestra del trasferimento.
                    // backgroundThread.Join();
                    if (form2.ShowDialog() == DialogResult.OK)
                    {

                        err = true;
                        return -1;
                      
                    }
                    backgroundThread.Join();
                    if (erroreTrasferimento)
                    {
                        err = true;
                        return -1;

                    }
                }
             
                return 1;
            }catch(IOException ex){

                MessageBox.Show("Devi selezionare prima un file");
                return -1;
            }  
        }
    }
    }


