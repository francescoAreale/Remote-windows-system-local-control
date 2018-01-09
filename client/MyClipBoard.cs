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
namespace Progetto_Malnati
{
    class MyClipBoard
    {
        public IPAddress ipAddress;
        public int port;
        public Socket sockClipBoard;
        public IPEndPoint remoteEPClipBoard;
        public MyClient client;
        public byte[] bytes = new byte[1024];
        public byte[] bufDim = new byte[1024];
        public byte[] clientData = new byte[1024 * 1024];
        public byte[] msg = Encoding.ASCII.GetBytes("+OK");
       
        public string receivedPath = @"C:\local\";
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

        public void setClient(MyClient client)
        {
            this.client = client;
        }
        [STAThreadAttribute]
        public void setForm(Form1 form)
        {
            this.formClip = form;
        }




        [STAThreadAttribute]
        public void receiveDataClipBoard()
        {
            int bytesRec;
            while (true)
            {
                try
                {

                    
                    while (true)
                    {

                        formClip.DoStatus(true);


                        //Console.WriteLine("Attendo ciao dal server");

                        //ricevo i dati dalla clipboard

                        bytesRec = client.getClipboardReciveSocket().Receive(bytes);
                        data = Encoding.ASCII.GetString(bytes, 0, bytesRec);

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

                            case "T":
                                Console.WriteLine("Case T");
                                byteRead = client.getClipboardReciveSocket().Receive(clientData);
                                string text = Encoding.Default.GetString(clientData, 0, byteRead);
                                formClip.retriveClipBoard(text);
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
                                //  Console.WriteLine("Sono nel path: " + receivedPath);
                                client.getClipboardReciveSocket().Send(msg);
                                //Console.WriteLine("Ho ricevuto ENDDIR allora mando +ok");
                                break;

                            case "END":
                                Console.WriteLine("Ho ricevuto END");
                                formClip.addToClipBoard(listFileName);
                                receivedPath = @"C:\local\";
                                listFileName.Clear();
                                break;

                            case "-ERR\r\n":
                                Console.WriteLine("CASE ERR");
                                if (formClip.invioClip != null && formClip.invioClip.IsAlive)
                                    formClip.invioClip.Abort();
                                break;
                        }

                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("Il server ha terminato la connessione, MyClipBoard 1");
                    break;
                   // client.aggiustaLista();
                    //client.closeAllSocket();
                    //client.getClipboardReciveSocket().Close();
                   // return;
                }
                catch (ObjectDisposedException ex)
                {
                    Console.WriteLine("Il server ha terminato la connessione, MyClipBoard disposed");
                   
                    client.aggiustaLista();
                    //client.closeAllSocket();
                   // return;
                }
                /* catch (ThreadAbortException ex)
                 {
                     MessageBox.Show("Il server ha terminato la connessione, MyClipBoard 2");
                    //client.closeAllSocket();
                     Thread.ResetAbort();
                     return;
                 }*/

            }
        }



        public int recvFile(string path)
        {
            string fileName;

            try
            {
                
                
                //ricevo il nome del file
                client.getClipboardReciveSocket().Receive(clientData);

                int fileNameLen = BitConverter.ToInt32(clientData, 0);

                fileName = Encoding.ASCII.GetString(clientData, 4, fileNameLen);
               
                long dim = BitConverter.ToInt64(clientData, fileNameLen + 4);
              //  byte[] msg2 = Encoding.ASCII.GetBytes("+OK" + fileName);
              

              
                client.getClipboardReciveSocket().Send(msg);
            
                


                BinaryWriter bWrite = new BinaryWriter(File.Open(path + fileName, FileMode.Append));
                //   Console.WriteLine("Nome File: " + fileName);
                // Console.WriteLine("Dimensione File: " + dim);
                bool errorFile = false;

                int ricevuti = 0;
                long dimBar = dim;

                ProgressBar bar = new ProgressBar();
                //
                Thread backgroundThread = new Thread( new ThreadStart(() =>{
                    
            bar.SetBar((int)dim);
            // Iterate from 0 - 99
            // On each iteration, pause the thread for .05 seconds, then update the dialog's progress bar
            try
            {
               

                while (dim > 0)
                {
                    if (client.getClipboardReciveSocket().Poll(3000000, SelectMode.SelectRead))
                    {
                        byteRead = client.getClipboardReciveSocket().Receive(clientData);

                        ricevuti = ricevuti + byteRead;
                        string response = Encoding.ASCII.GetString(clientData, 0, byteRead);

                        if (response.Equals("-ERR\r\n"))//il server mi manda -ERR quando  durante un trasferimento file 
                        {
                            bWrite.Close();
                            // Close the dialog if it hasn't been already
                            if (bar.InvokeRequired)
                                bar.BeginInvoke(new Action(() => { bar.showdialogset(true); bar.Close(); bar.showdialogset(false); }));
                            MessageBox.Show("-ERR, connessione interrotta");
                            errorFile = true;
                            return;
                        }
                       // bar.UpdateProgress(byteRead, (int)dimBar, fileName);
                        bWrite.Write(clientData, 0, byteRead);
                        dim = dim - byteRead;
                    }
                    else {
                        bWrite.Close();
                        // Close the dialog if it hasn't been already
                        if (bar.InvokeRequired)
                            bar.BeginInvoke(new Action(() => { bar.showdialogset(true); bar.Close(); bar.showdialogset(false); }));
                        MessageBox.Show("Errore nel trasferimento, connessione interrotta");
                        errorFile = true;

                        return;
                    }
                }

            }
            catch (Exception ex)
            {
                bWrite.Close();
                // Close the dialog if it hasn't been already
                if (bar.InvokeRequired)
                    bar.BeginInvoke(new Action(() => { bar.showdialogset(true); bar.Close(); bar.showdialogset(false); }));
                MessageBox.Show("Errore nel trasferimento, connessione interrotta");
                errorFile = true;

                return ;
            }

                      
                        bWrite.Close();

                        client.getClipboardReciveSocket().Send(msg);
                     
                        if (bar.InvokeRequired)
                            bar.BeginInvoke(new Action(() => { bar.showdialogset(true); bar.Close(); bar.showdialogset(false); }));


                    }
                ));
                
                // Start the background process thread
                backgroundThread.Start();


                //****Il caso in cui il client chiede la finestra del trasferimento****//

                if (bar.ShowDialog() == DialogResult.OK) {
                    Console.WriteLine("Ho interrotto il trasferimento");
                    bWrite.Close();
                    client.currentSocket.Send(Encoding.ASCII.GetBytes("-ERR\r\n<EOF>"));
                    backgroundThread.Abort();
                    return -1;
                }

                backgroundThread.Join();

                if (errorFile)
                {
                    errorFile = false;
                    return -1;
                }
                if (path == @"C:\local\")
                {
                    listFileName.Add(path + fileName);
                }
                return 1;
            }
            catch (SocketException ex)
            {
                MessageBox.Show("Il server ha terminato la connessione");
                client.closeAllSocket();
                return -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Il server ha terminato la connessione");
                client.closeAllSocket();
                return -1;
            }

        }



        public int recvDirectory(string path)
        {

            try
            {
                //invio +ok al server 
                client.getClipboardReciveSocket().Send(msg);
                client.getClipboardReciveSocket().Receive(clientData);
                int DirectoryNameLen = BitConverter.ToInt32(clientData, 0);
                string DirectoryName = Encoding.ASCII.GetString(clientData, 4, DirectoryNameLen);
                string NewPath = path + DirectoryName;
              /*  Console.WriteLine("lunghezzaNameDir " + DirectoryNameLen);
                Console.WriteLine("Dir Name " + DirectoryName);
                Console.WriteLine("Nuovo Percorso " + NewPath);*/
                Directory.CreateDirectory(NewPath);
              
                if (path == @"C:\local\")
                    listFileName.Add(NewPath);

                receivedPath = NewPath + @"\";
                client.getClipboardReciveSocket().Send(msg);
                return 1;
            }
            catch (SocketException ex)
            {
                MessageBox.Show("Il server ha terminato la connessione");
                client.closeAllSocket();
                return -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Il server ha terminato la connessione");
                client.closeAllSocket();
                return -1;
            }
        }
    }
}
