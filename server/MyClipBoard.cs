using System;
using System.IO;
using System.IO.Compression;
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
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
namespace WindowsFormsApplication3
{
   
    class MyClipBoard
    {
        public IPAddress ipAddress;
        public int port;
        public Socket sockClipBoard;
        public IPEndPoint remoteEPClipBoard;
        public MyServer server;
        public byte[] bytes = new byte[1024];
        public byte[] bufDim = new byte[1024];
        public byte[] clientData = new byte[1024 * 1024];
        public byte[] msg = Encoding.ASCII.GetBytes("+OK");
        public string receivedPath = @"C:\local\";
        public byte[] msg2 = Encoding.ASCII.GetBytes("+OK");
        public List<String> listFileName = new List<string>();
        public String data;
        long verify = 0;
        long dimTot;
        double d;
        int byteRead;
        long byteRead2;
        long dimToClear = 0;
        public Form1 formClip;
        public Form2 form2;
        public bool ricevutodasocket;

        ManualResetEvent autoEvent = null;
        Socket listener;


        public void setRicevutodalserver(bool ricevutodasocket)
        {
            this.ricevutodasocket = ricevutodasocket;

        }


        public void setServer(MyServer server)
        {
            this.server = server;

        }

        public void setevent(ManualResetEvent autoEvent)
        {

            this.autoEvent = autoEvent;

        }

        
        [STAThreadAttribute]
        public void setForm(Form1 form)
        {
            this.formClip = form;
        }

        [STAThreadAttribute]
        public void receiveDataClipBoard()
        {
            // questo thread è quello che riceve
            IPEndPoint localEndPoint = new IPEndPoint(server.ipAddress, server.port + 2);
            listener = new Socket(AddressFamily.InterNetwork,
             SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            listener.Listen(10);
            
            try
            {
                server.terminateGracefully = false;
                while (!server.terminateGracefully) // in questo while tento di chiudere bene il thread tramite la variabile. Appena esco dal while chiudo tutto i socket.
                {
                    sockClipBoard = listener.Accept(); // in attesa di connessioni  
                    server.terminatecurrentsocket = false;
                    StateObject state = new StateObject();
                    state.workSocket = sockClipBoard;
                   
                    while (!server.terminatecurrentsocket)
                    {
                        // la begin receive fa un thread per ogni connessione. Uso un evento per bloccarla dato che è asincrona. Ogni volta che voglio terminare il thread
                        // mi basta settare la variabile e segnalare l'evento. 
                        server.receiveDone.Reset();
                       if(sockClipBoard!=null) sockClipBoard.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                        server.receiveDone.WaitOne();
                    }

                    if (sockClipBoard != null) sockClipBoard.Close();

                }
                closeSocket();
            }
            catch (SocketException ex)
            {

                if (sockClipBoard != null) { sockClipBoard.Close(); sockClipBoard = null; }
                Console.WriteLine("sono il socket che riceve in eccezione per socket ");
                return;
            }
            catch (ObjectDisposedException ex)
            {

                if (sockClipBoard != null) { sockClipBoard.Close(); sockClipBoard = null; }
                Console.WriteLine("sono il socket che riceve in eccezione disposes");
                return;

            }
            catch (Exception ex)
            {
                if (sockClipBoard != null) { sockClipBoard.Close(); sockClipBoard = null; }
                Console.WriteLine("sono il socket che riceve in eccezione Generale");
                return;

            }

        }


        public void ReceiveCallback(IAsyncResult ar)
        {


            try
            {
                StateObject state = (StateObject)ar.AsyncState;

                // Read data from the remote device.
                int bytesRead = state.workSocket.EndReceive(ar);



                if (bytesRead == 0)
                {
                    if (sockClipBoard != null) sockClipBoard.Close();
                    Console.WriteLine("il client ha chiuso la connessione");

                    server.terminatecurrentsocket = true;
                    server.receiveDone.Set();
                    return;
                }
                if (bytesRead > 0)
                {
                    if (server.GetConnected())
                    {
                        data = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
                        formClip.DoStatus(false);
                        switch (data)
                        {

                            case "F":

                                if (recvFile(receivedPath) < 0)
                                {
                                    receivedPath = @"C:\local\";
                                    if (Directory.Exists(receivedPath)) Directory.Delete(receivedPath, true);
                                    listFileName.Clear();
                                    Directory.CreateDirectory(receivedPath);

                                }
                                break;
                            case "I":
                                Bitmap bmp = ReceiveVarData(sockClipBoard);
                                formClip.setImageToClip(bmp);
                                break;
                            case "T":
                                //formClip.DoStatus(false);
                                byteRead = sockClipBoard.Receive(clientData);
                                string text = Encoding.Default.GetString(clientData, 0, byteRead);
                                Console.WriteLine("!!!!!!!!!!!!!!!!!!" + text);
                                formClip.retriveClipBoard(text);
                                //formClip.DoStatus(true);
                                break;
                            case "DIR":
                                if (recvDirectory(receivedPath) < 0)
                                {
                                    receivedPath = @"C:\local\";
                                    listFileName.Clear();
                                }
                                break;
                            case "ENDDIR":
                                DirectoryInfo parentDir = Directory.GetParent(Path.GetDirectoryName(receivedPath));
                                string parent = parentDir.FullName;
                                receivedPath = parent + @"\";
                                sockClipBoard.Send(msg2);
                                break;

                            case "END":
                                Console.WriteLine("Ho ricevuto END");
                                formClip.addToClipBoard(listFileName);
                                receivedPath = @"C:\local\";
                                listFileName.Clear();
                                // formClip.DoStatus(true);
                                break;
                        }
                        formClip.DoStatus(true);
                    }


                }
            }

            catch (SocketException ex)
            {
                if (sockClipBoard != null) sockClipBoard.Close();
                Console.WriteLine("sono il socket che riceve in eccezione ");
                server.terminatecurrentsocket = true;
                server.receiveDone.Set();
                return;
            }
          catch (Exception ex)
            {
                Console.WriteLine("sono il socket che riceve in eccezione: " + ex.ToString());
                server.terminatecurrentsocket = true;
                server.receiveDone.Set();
                return;
            }
            server.receiveDone.Set();
        }


        public static Bitmap ConvertByteArrayToBitmap(byte[] imageSource)
        {
            var imageConverter = new ImageConverter();
            var image = (Image)imageConverter.ConvertFrom(imageSource);
            return new Bitmap(image);
        }

        /**
         * Ricevo dal socket un byte array relativo all' image
         * **/
        private static Bitmap ReceiveVarData(Socket s)
        {
            int total = 0;
            int recv;
            byte[] datasize = new byte[4];

            recv = s.Receive(datasize, 0, 4, 0);
            int size = BitConverter.ToInt32(datasize, 0);
            int dataleft = size;
            byte[] data = new byte[size];


            while (total < size)
            {
                recv = s.Receive(data, total, dataleft, 0);
                if (recv == 0)
                {
                    break;
                }
                total += recv;
                dataleft -= recv;
            }
            return ConvertByteArrayToBitmap(data);
        }
        public void closeSocket()
        {

            if (sockClipBoard != null) sockClipBoard.Close();
            if (listener != null) listener.Close();
            listener = null;
            sockClipBoard = null;

        }

        public int recvFile(string path)
        {
            try
            {
                string fileName;


                //invio +ok al server   
                //   sockClipBoard.Send(msg);
                // Console.WriteLine("Ho ricevuto F allora mando +ok mandato");


                //ricevo il nome del file
                sockClipBoard.Receive(clientData);

                // Console.WriteLine("ClientData: " + Encoding.ASCII.GetString(clientData));
                int fileNameLen = BitConverter.ToInt32(clientData, 0);

                fileName = Encoding.ASCII.GetString(clientData, 4, fileNameLen);
                //ricevo la dimensione
                long dim = BitConverter.ToInt64(clientData, fileNameLen + 4);
                // byte[] msg2 = Encoding.ASCII.GetBytes("+OK" + fileName);
                //Console.WriteLine("Ho ricevuto correttamente i dati del file");
                // Console.WriteLine("ClientData: " + Encoding.ASCII.GetString(clientData));
                //invio +ok al server 
                sockClipBoard.Send(msg);
                // Console.WriteLine("+ok mandato");
                // Console.WriteLine("++++++++++++++ " + fileName);
                BinaryWriter bWrite = new BinaryWriter(File.Open(path + fileName, FileMode.Append));


                int ricevuti = 0;
                bool errorefile = false;
                long dimBar = dim;

                Form2 form2 = new Form2();
                
                //
                Thread backgroundThread = new Thread(
        new ThreadStart(() =>
        {
            form2.SetBar(100);
            // Iterate from 0 - 99
            // On each iteration, pause the thread for .05 seconds, then update the dialog's progress bar
            try
            {


                while (dim > 0 && !errorefile)
                {
                    if (sockClipBoard.Poll(3000000, SelectMode.SelectRead) )
                    {
                        byteRead = sockClipBoard.Receive(clientData);

                        ricevuti = ricevuti + byteRead;
                        string response = Encoding.ASCII.GetString(clientData, 0, byteRead);

                        if (response == "-ERR\r\n")//il server mi manda -ERR quando  durante un trasferimento file 
                        //                      io cambio file da mettere nella clip
                        {
                            bWrite.Close();
                            // Close the dialog if it hasn't been already
                            if (form2.InvokeRequired)
                                form2.BeginInvoke(new Action(() => { form2.showdialogset(true); form2.Close(); form2.showdialogset(false); }));
                            errorefile = true;
                            return;
                        }
                       // Console.WriteLine(dimBar);
                       // Console.WriteLine((float)((100 * byteRead) /dimBar));
                        form2.UpdateProgress(byteRead, (int)dimBar, fileName);
                       
                        bWrite.Write(clientData, 0, byteRead);
                        dim = dim - byteRead;
                    }
                    else
                    {
                        bWrite.Close();
                        // Close the dialog if it hasn't been already
                        if (form2.InvokeRequired)
                            form2.BeginInvoke(new Action(() => { form2.showdialogset(true); form2.Close(); form2.showdialogset(false); }));
                        errorefile = true;
                        return;
                    }
                }
                bWrite.Close();

                if (form2.InvokeRequired)
                    form2.BeginInvoke(new Action(() => { form2.showdialogset(true); form2.Close(); form2.showdialogset(false); }));

            }
            catch (Exception ex)
            {
                bWrite.Close();
                // Close the dialog if it hasn't been already
                if (form2.InvokeRequired)
                    form2.BeginInvoke(new Action(() => { form2.showdialogset(true); form2.Close(); form2.showdialogset(false); }));
                errorefile = true;
                server.terminatecurrentsocket = true;
                return;

            }
            // Console.WriteLine("File Ricevuto!");
            

            // Reset the flag that indicates if a process is currently running

        }
    ));

                // Start the background process thread
                backgroundThread.Start();

                if (form2.ShowDialog() == DialogResult.OK)
                {
                    formClip.getcurrentsockinvio().Send(Encoding.ASCII.GetBytes("-ERR\r\n"));
                    errorefile = true;
                }
                backgroundThread.Join();

                if (errorefile == true)
                {
                    errorefile = false;
                    return -1;

                }

                if (path == @"C:\local\")
                {
                    //Console.WriteLine("Sono nell if");
                    listFileName.Add(path + fileName);
                }
                sockClipBoard.Send(msg);
                return 1;
            }
            catch (Exception ex)
            {

                Console.WriteLine("errore nella ricezione");
                return -1;

            }
        }

        public int recvDirectory(string path)
        {


            sockClipBoard.Send(msg);
            sockClipBoard.Receive(clientData);
            int DirectoryNameLen = BitConverter.ToInt32(clientData, 0);
            string DirectoryName = Encoding.ASCII.GetString(clientData, 4, DirectoryNameLen);
            string NewPath = path + DirectoryName;

            Directory.CreateDirectory(NewPath);
            //invio +ok al server 

            // Console.WriteLine("+ok mandato");
            if (path == @"C:\local\")
                listFileName.Add(NewPath);

            receivedPath = NewPath + @"\";

            sockClipBoard.Send(msg);
            return 1;


        }
    }
}