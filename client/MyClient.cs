using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace Progetto_Malnati
{
    public class MyClient
    {
        

        // ManualResetEvent instances signal completion.
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);
        public IPHostEntry ipHostInfo;
        public int port;
        public String password;
        public int numClients;
        public Form1 form;
        public Boolean connessioneEffettuata=false;
        public Socket currentSocket=null;
        public List<Socket> listSocket = new List<Socket>();
        public List<IPEndPoint> listMouseSocket = new List<IPEndPoint>();
        public List<Socket> listKeyboardSocket = new List<Socket>();
        public List<Socket> listClipboardSenderSocket = new List<Socket>();
        public List<Socket> listClipboardReceiveSocket = new List<Socket>();
        public byte[] dormi = Encoding.ASCII.GetBytes("DORMI<EOF>");
        public byte[] sveglia = Encoding.ASCII.GetBytes("SVEGLIA<EOF>");
        public IPAddress serverAddress;
        // The response from the remote device.
        public static String response = String.Empty;

        public void StartClient()
        {
            // Connect to a remote device.
            try
            {
                
                // Establish the remote endpoint for the socket.
                // The name of the 
                // remote device is "host.contoso.com".
              //  IPHostEntry ipHostInfo = Dns.GetHostEntry("192.168.1.13");
                
                IPEndPoint remoteEP = new IPEndPoint(serverAddress, port);
                byte[] bytes = new byte[1024];
                // Create a TCP/IP  socket.
                Socket sender = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);
                
                currentSocket = sender;
                // Connect the socket to the remote endpoint. Catch any errors.
             
                    sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}",sender.RemoteEndPoint.ToString());
                    
                    // Encode the data string into a byte array.
                    byte[] msg = Encoding.ASCII.GetBytes("PASS"+Encrypt(password)+"<EOF>");
                    Console.WriteLine("PASSWORD:{0} ", password);
                    // Send the data through the socket.
                    int bytesSent = sender.Send(msg);
                    
                    // Receive the response from the remote device.
                    int bytesRec = sender.Receive(bytes);

                    response = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                    if (response == "+OK\r\n")
                    {
                        connessioneEffettuata = true;
                        numClients = listSocket.Count();
                        form.changeStatusListView("Pronto per il controllo",true);// aggiorna lo stato della connessione
                        Console.WriteLine("PASSWORD CORRETTA");
                        if (listSocket.Count <= form.indexListView)
                            listSocket.Add(sender);//aggiungo il nuovo socket all' array di socket
                        else
                            listSocket.Insert(form.indexListView+1, sender);

                        Console.WriteLine("E' stato instanziato il primo socket");


                        createMouseSocket();
                        createKeyboardSocket();
                        createClipboardSenderSocket();
                        createClipboardReceiveSocket();

                        if (listSocket.Count > 1)
                            addormentaServer();


                            svegliaServer();

                            Thread ControlThread = new Thread(new ThreadStart(form.startControl));
                            ControlThread.Start();
                        

                    }
                    else {
                        form.changeStatusListView("Password errata", false);// aggiorna lo stato della connessione
                        Console.WriteLine("PASSWORD ERRATA");
                    }


             

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException ex)
                {
                    MessageBox.Show("Connessione terminata");
                    form.changeStatusListView(" ",true);
                    closeAllSocket();
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Connessione terminata");
                    closeAllSocket();
                    return;
                }

           
          
        }



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

        public void alignListSocketWithConfigurationFIle (){
                listClipboardReceiveSocket.Add(null);
                listClipboardSenderSocket.Add(null);
                listKeyboardSocket.Add(null);
                listSocket.Add(null);
                listMouseSocket.Add(null);
        }


        public void createMouseSocket() {
            port = Convert.ToInt32(((IPEndPoint)currentSocket.RemoteEndPoint).Port.ToString());
            IPEndPoint localEndPoint = new IPEndPoint(serverAddress, port);
            if (listMouseSocket.Count <= form.indexListView)
                listMouseSocket.Add(localEndPoint);
            else
                listMouseSocket.Insert(form.indexListView+1, localEndPoint);

        }

        public IPEndPoint getCurrentEndpoint() {
            return listMouseSocket.ElementAt(form.indexListView);
          }


        public void createKeyboardSocket() {
            Socket sockTastiera = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            port = Convert.ToInt32(((IPEndPoint)currentSocket.RemoteEndPoint).Port.ToString());
            IPEndPoint remoteEP = new IPEndPoint(serverAddress, port - 1);
            sockTastiera.Connect(remoteEP);

            if (listKeyboardSocket.Count <= form.indexListView)
                listKeyboardSocket.Add(sockTastiera);
            else
                listKeyboardSocket.Insert(form.indexListView+1, sockTastiera);
        }

        public Socket getCurrentKeyboardSocket() {
            if (listKeyboardSocket.Count > form.indexListView)
            return listKeyboardSocket.ElementAt(form.indexListView);
            return null;
          }



        public void createClipboardSenderSocket()
        {
            Socket clipsocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            port = Convert.ToInt32(((IPEndPoint)currentSocket.RemoteEndPoint).Port.ToString());
            IPEndPoint remoteEPClipBoard = new IPEndPoint(serverAddress, port + 2);
            clipsocket.Connect(remoteEPClipBoard);

            if (listClipboardSenderSocket.Count <= form.indexListView)
                listClipboardSenderSocket.Add(clipsocket);
            else
                listClipboardSenderSocket.Insert(form.indexListView+1, clipsocket);
        }

        public Socket getClipboardSenderSocket()
        {
            if (listClipboardSenderSocket.Count > form.indexListView)
            {
                return listClipboardSenderSocket.ElementAt(form.indexListView);
                
            }
            return null;
        }


        public void createClipboardReceiveSocket()
        {
            Socket csockClipBoard = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            port = Convert.ToInt32(((IPEndPoint)currentSocket.RemoteEndPoint).Port.ToString());
            IPEndPoint remoteClipBoard = new IPEndPoint(serverAddress, port + 1);
            csockClipBoard.Connect(remoteClipBoard);

          /*  if (listClipboardReceiveSocket.Count>0 &&  listClipboardReceiveSocket[form.indexListView] == null)
                listClipboardReceiveSocket.Insert(form.indexListView, csockClipBoard);
            else*/ if (listClipboardReceiveSocket.Count <= form.indexListView)
                listClipboardReceiveSocket.Add(csockClipBoard);
            else
                listClipboardReceiveSocket.Insert(form.indexListView+1, csockClipBoard);

        }
        public void aggiustaLista() {

            Socket csockClipBoard = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            port = Convert.ToInt32(((IPEndPoint)currentSocket.RemoteEndPoint).Port.ToString());
            IPEndPoint remoteClipBoard = new IPEndPoint(serverAddress, port + 1);
            csockClipBoard.Connect(remoteClipBoard);
                listClipboardReceiveSocket.Insert(form.indexListView, csockClipBoard);
        }

        public Socket getClipboardReciveSocket()
        {
            if (listClipboardReceiveSocket.Count > form.indexListView)
                return listClipboardReceiveSocket.ElementAt(form.indexListView);
            return null;
        }

        public void setNullSocket(Socket s) {
            s = null;
        }

        public void addormentaServer() {

            if (listSocket.Count > 1 && getClipboardReciveSocket() != null)
            {
                getClipboardReciveSocket().Close();
                setNullSocket(listClipboardReceiveSocket.ElementAt(form.indexListView));
            }
            for (int i = 0; i < listSocket.Count; i++)
            {
                if(listSocket[i]!=null)
                listSocket[i].Send(dormi);
            }
            Console.WriteLine("ho addormentato il precedente");
        }

        public void svegliaServer()
        {
             if (listSocket.Count > 1)
                    aggiustaLista();

            currentSocket.Send(sveglia);

            Console.WriteLine("ho svegliato quello selezionato");
        }

        public void closeAllSocket() {

            if (currentSocket != null)
            {
                currentSocket.Close();
                setNullSocket(currentSocket);
            }
            
            if (getClipboardReciveSocket() != null)
            {
                getClipboardReciveSocket().Close();
                setNullSocket(getClipboardReciveSocket());
               // listClipboardReceiveSocket.Remove(getClipboardReciveSocket());
            }
            if (getClipboardSenderSocket() != null)
            {
                
                getClipboardSenderSocket().Close();
                setNullSocket(getClipboardSenderSocket());
                //listClipboardSenderSocket.Remove(getClipboardSenderSocket());
            }
            if (getCurrentKeyboardSocket() != null)
            {
                
                getCurrentKeyboardSocket().Close();
                setNullSocket(getCurrentKeyboardSocket());
                //listKeyboardSocket.Remove(getCurrentKeyboardSocket());
            }

        }
    }
    
}
