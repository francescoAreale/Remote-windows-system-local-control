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
namespace Progetto_Malnati
{
    public class ClipboardSender
    {
        private Socket clipsocket;
        private bool erroreTrasferimento = false;
        MyClient client;
        public IPAddress ipAddress;
        public int port;
        public Socket sockClipBoard;
        public IPEndPoint remoteEPClipBoard;
        private string nomefile;
        private long dim;
        public string textselected;
        public int valore; // controlla se è testo o file.
        private ManualResetEvent autoEvent;
        private string path;
        private Boolean flagNumber;
        private Thread backgroundThread;
        public List<string> filename = new List<string>();
        public long filetotallen = 0;
        // public Form1 form1;
        public void SetClient(MyClient client)
        {

            this.client = client;
        }

        public void setparam(string path)
        {

            filename.Add(path);

        }
        public void setevent(ManualResetEvent autoEvent)
        {

            this.autoEvent = autoEvent;

        }




        public void sendData()
        {


            byte[] msg = new byte[1024];
            byte[] buffer = new byte[1024 * 1024];
            byte[] conferma = new byte[1024];


            try
            {
            


                if (this.valore == 1)
                {
                    // Console.WriteLine("mi sono svegliato sto inviando ");


                    for (int i = 0; i < filename.Count; i++)
                    {
                        string confermastringa;

                        if (!Directory.Exists(filename[i]))
                        {

                            if (this.SendFile(filename[i]) == -1)
                            {
                                Console.WriteLine("!!!!!!!!!!!!!!!!!!! Errore nella send file");
                                filename.RemoveRange(0, filename.Count);
                                return;
                            }
                            client.getClipboardSenderSocket().Receive(conferma);

                        }
                        else
                        {

                            if (this.SendDir(filename[i]) < 0)
                            {
                                Console.WriteLine("La sendDir ha tornato -1");
                                filename.RemoveRange(0, filename.Count);
                                return;
                            }

                        }
                    }



                    Console.WriteLine("Il client ha ricevuto il file");
                    msg = Encoding.ASCII.GetBytes("END");

                    client.getClipboardSenderSocket().Send(msg);
                    Console.WriteLine("Invio END al client");
                    //autoEvent.Reset();

                    filename.RemoveRange(0, filename.Count);
                }
            }
            catch (ThreadAbortException e)
            {
                Console.WriteLine("Thread - caught ThreadAbortException - resetting.");
                Console.WriteLine("Exception message: {0}", e.Message);
                backgroundThread.Abort();
                backgroundThread.Join();
            };



            if (this.valore == 0)
            {
                
                    client.getClipboardSenderSocket().Send(Encoding.ASCII.GetBytes("T"));
                    Console.WriteLine("Il testo selezionato è:  " + textselected);

                    Encoding encoding = Encoding.GetEncoding("iso-8859-1");

                    msg = encoding.GetBytes(textselected);

                    client.getClipboardSenderSocket().Send(msg);
             

            }
            // }


        }

        public int SendDir(string nameDir)
        {


            byte[] buffer = new byte[1024 * 1024];
            byte[] conferma = new byte[1024];
            // Console.WriteLine("sto mandando la directory " + new FileInfo(nameDir + @"\").Directory.Name);
            byte[] msg = Encoding.ASCII.GetBytes("DIR");

            try
            {
                client.getClipboardSenderSocket().Send(msg);
                client.getClipboardSenderSocket().Receive(conferma);

                byte[] DirNameByte = Encoding.ASCII.GetBytes(new FileInfo(nameDir + @"\").Directory.Name);
                byte[] DirNameLen = BitConverter.GetBytes(DirNameByte.Length);
                byte[] clientData = new byte[4 + DirNameByte.Length];
                DirNameLen.CopyTo(clientData, 0);
                DirNameByte.CopyTo(clientData, 4);
                client.getClipboardSenderSocket().Send(clientData);
                client.getClipboardSenderSocket().Receive(conferma);
                string confermastringa = Encoding.ASCII.GetString(conferma);

                if (confermastringa.IndexOf("+OK") > -1)
                {
                    Console.WriteLine("!!!!!!!!!!!SUB Entries");


                    string[] fileEntries = Directory.GetFiles(nameDir);
                    string[] subdirEntries = Directory.GetDirectories(nameDir);



                    foreach (string filename in fileEntries)
                    {
                        Console.WriteLine("!!!!!!!!!!!Mando il file");
                        if(SendFile(filename)<0){
                        return -1;
                        }
                        client.getClipboardSenderSocket().Receive(conferma);
                    }

                    foreach (string dirname in subdirEntries)
                    {
                        Console.WriteLine("!!!!!!!!!!!Mando la cartella");
                        if (SendDir(dirname) < 0)
                            return -1;


                    }

                    msg = Encoding.ASCII.GetBytes("ENDDIR");
                    client.getClipboardSenderSocket().Send(msg);
                    Console.WriteLine("Ho mandato END DIR");
                    client.getClipboardSenderSocket().Receive(conferma);
                    //}

                }
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erroe nella sendDir");
                return -1;
            }
        }
        public int SendFile(string namefile)
        {

            byte[] msg = new byte[1024];
            byte[] buffer = new byte[1024 * 1024];
            byte[] conferma = new byte[1024];


            try
            {
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
                // Thread.Sleep(3000);
                msg = Encoding.ASCII.GetBytes("F");
                client.getClipboardSenderSocket().Send(msg);


                byte[] fileNameByte = Encoding.ASCII.GetBytes(this.nomefile);
                byte[] fileNameLen = BitConverter.GetBytes(fileNameByte.Length);
                byte[] filedim = BitConverter.GetBytes(this.dim);
                //   Console.WriteLine("sto creando le informazioni da mandare al client");
                byte[] clientData = new byte[4 + fileNameByte.Length + filedim.Length];
                fileNameLen.CopyTo(clientData, 0);
                fileNameByte.CopyTo(clientData, 4);
                filedim.CopyTo(clientData, 4 + fileNameByte.Length);

                Console.WriteLine(" Invio ClientData");

                client.getClipboardSenderSocket().Send(clientData);
                // Console.WriteLine("Ho mandato clientData");
                client.getClipboardSenderSocket().Receive(conferma);
                string confermastringa = Encoding.ASCII.GetString(conferma);

                if (confermastringa.IndexOf("+OK") > -1)
                {

                    //   Console.WriteLine(" inizio lo stream del file");

                    FileStream fileStream = new FileStream(namefile, FileMode.Open, FileAccess.Read);
                   

                        int length = (int)fileStream.Length;  // get file length
                        // create buffer
                        int count;
                        double d;// actual number of bytes read

                        if (filetotallen > (1024 * 1024))
                        {
                            d = (((double)filetotallen) / (double)(1024 * 1024));
                        }
                        else
                        {
                            d = 1;
                        }

                        ProgressBar bar = new ProgressBar();

                        long sum = dim;
                        backgroundThread = new Thread(
                new ThreadStart(() =>
                {
                    try{
                    // Iterate from 0 - 99
                    // On each iteration, pause the thread for .05 seconds, then update the dialog's progress bar
                    while ((count = fileStream.Read(buffer, 0, 1024 * 1024)) > 0)
                    {

                        bar.SetBar((int)sum);
                        client.getClipboardSenderSocket().Send(buffer, count, SocketFlags.None);
                        dim = dim - count;
                        //  Console.WriteLine(dim);
                        long count2 = count;
                      //  bar.UpdateProgress(count, (int)sum, nomefile);

                        /*** aggiorno la progress bar***/
                    }

                    fileStream.Close();

                    // Close the dialog if it hasn't been already
                    if (bar.InvokeRequired)
                        bar.BeginInvoke(new Action(() => { bar.showdialogset(true); bar.Close(); bar.showdialogset(false); }));

                    }
                    catch (ThreadAbortException ex)
                    {
                        
                        // fileStream.Close();
                        if (bar.InvokeRequired)
                            bar.BeginInvoke(new Action(() => { bar.showdialogset(true); bar.Close(); bar.showdialogset(false); }));

                        Console.WriteLine("sto ammazzando il thread().");
                        client.getClipboardSenderSocket().Send(Encoding.ASCII.GetBytes("-ERR\r\n"));
                        fileStream.Close();
                        return;
                    }
                    catch (SocketException ex)
                    {
                        erroreTrasferimento = true;
                        // fileStream.Close();
                        if (bar.InvokeRequired)
                            bar.BeginInvoke(new Action(() => { bar.showdialogset(true); bar.Close(); bar.showdialogset(false); }));

                        Console.WriteLine("sto ammazzando il thread().");
                        if (client.getClipboardSenderSocket()!=null)
                        client.getClipboardSenderSocket().Send(Encoding.ASCII.GetBytes("-ERR\r\n"));
                        fileStream.Close();
                        return;
                    }
                    // Reset the flag that indicates if a process is currently running

                }
            ));

                        // Start the background process thread
                        backgroundThread.Start();

                       if (bar.ShowDialog()== DialogResult.OK)
                        {
                            
                            backgroundThread.Abort();
                          
                            return -1;
                        }
                    // }
                }
                return 1;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Erroe nella sendFile");
                return -1;
            }
        }
    }
}

