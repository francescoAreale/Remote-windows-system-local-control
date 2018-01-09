using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.Specialized;
using System.Text.RegularExpressions;


namespace Progetto_Malnati
{
    public partial class Form1 : Form
    {
        [DllImport("User32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("User32.dll")]
        public static extern void ReleaseDC(IntPtr hwnd, IntPtr dc);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        protected static extern int SetClipboardViewer(int hWndNewViewer);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        Thread threadClient = null;
        Thread threadMouse = null;
        Thread threadKeyboard = null;
        Thread threadClipboard = null;
        Thread area = null;

        private string hotKeyValues;
        private delegate void InvokeDelegate(string text,bool b);
        private delegate void InvokeDelegate2(string text);
        private delegate void InvokeDelegateStatus(bool status);
        private delegate void InvokeDelegateAddClipBoard(List<String> listLocalPath);

        private Int32 idHotkey=0; // id incrementale passato alla funzione registerHotKey
        public MyClient client = null;
        public int indexListView;
        public ClipboardSender clipb ;
        public IntPtr nextClipboardViewer;
        Form2 formMouse;
        private bool ricevutoDalSocket;
        static ManualResetEvent autoEvent = null;
        public Thread invioClip;
        public Thread tClip = null;
        



        public void changeStatusListView(string text, bool b)
        {
            if (InvokeRequired)
            {
                Invoke(new InvokeDelegate(changeStatusListView), text,b);
                return;
            }
            for (int i = 0; i < listView1.Items.Count;i++ )
            {

            if(listView1.Items[i].SubItems[3].Text.Equals("Connesso") && !b)
                listView1.Items[i].SubItems[3].Text = "Pronto per il controllo";
            }
            listView1.FocusedItem.SubItems[3].Text = text;
        }


        public void DoStatus(bool status)
        {
            if (InvokeRequired)
            {
                Invoke(new InvokeDelegateStatus(DoStatus), status);
                return;
            }
            ricevutoDalSocket = status;
        }

        //List<Thread> listThread = new List<Thread>();

        public int clientConnected;

      

        public Form1()
        {
            InitializeComponent();
            this.Text = "Conenssione";
            listView1.View = View.Details;
            listView1.LabelEdit = false;
            listView1.AllowColumnReorder = true;
            listView1.FullRowSelect = true;
            // Display grid lines.
            listView1.GridLines = true;
            // Sort the items in the list in ascending order.
           // listView1.Sorting = SortOrder.Ascending;

                // Create columns for the items and subitems.
                // Width of -2 indicates auto-size.
            
                listView1.Columns.Add("Indirizzo IP", 100, HorizontalAlignment.Left);
                listView1.Columns.Add("Porta", 40, HorizontalAlignment.Left);
                listView1.Columns.Add("HotKyes", 120, HorizontalAlignment.Left);
                listView1.Columns.Add("Stato", 150, HorizontalAlignment.Left);
                listView1.Columns.Add("TimeStamp", 100, HorizontalAlignment.Left);

           

            textBox1.Text = "169.254.239.119";
            textBox2.Text = "1500";
            textBox3.Text = "s";

            nextClipboardViewer = (IntPtr)SetClipboardViewer((int)this.Handle);
            clipb = new ClipboardSender();
           
   
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


    


        private void Form1_Load(object sender, EventArgs e)
        {
            StreamReader confF=null;
            try
            {
                if (client == null)
                {
                    client = new MyClient();// è la prima volta che un client si connette

                }

                if (!Directory.Exists(@"C:\local\"))
                {
                    Directory.CreateDirectory(@"C:\local\");
                }

                if (File.Exists(Application.StartupPath + "\\data.txt"))
                {
                    string line = string.Empty;
                    confF = new StreamReader(Application.StartupPath + "\\data.txt");
                    ListViewItem item2 = null;
                    while ((line = confF.ReadLine()) != null)
                    {
                        /* salto se è un commento */
                        if (isComment(line)) continue;

                        /* controllo se la parola nel file corrisponde
                        *  a qualcuna della mia lista */
                        int val = 0;
                        foreach (AccettableStrings item in Enum.GetValues(typeof(AccettableStrings)))
                            if (line.StartsWith(item.ToString()))
                            {
                                switch (item.ToString())
                                {
                                    case "ADDRESS":
                                        item2 = new ListViewItem(getContent(line), clientConnected);
                                        break;
                                    case "HOTKEYS":
                                        Keys k = (Keys)Enum.Parse(typeof(Keys), getContent(line));
                                        item2.SubItems.Add(getContent(line));
                                        // Keys k = (Keys)val;

                                        bool success = RegisterHotKey(this.Handle, idHotkey, HotKeyWindows.Win32ModifiersFromKeys(k), HotKeyWindows.CharCodeFromKeys(k));
                                        if (success)
                                        {
                                            idHotkey++;
                                            hotKeyValues = val.ToString();

                                        }
                                        else
                                            MessageBox.Show("Could not register Hotkey - there is probably a conflict.  ", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        break;
                                    case "ENDSERVER":
                                        item2.Checked = true;
                                        item2.SubItems.Add(" ");
                                        item2.SubItems.Add(GetTimestamp(DateTime.Now));
                                        //Add the items to the ListView.
                                        listView1.Items.AddRange(new ListViewItem[] { item2 });
                                        clientConnected++;
                                        indexListView++;
                                        client.alignListSocketWithConfigurationFIle();
                                        break;
                                    default: item2.SubItems.Add(getContent(line));
                                        break;
                                }
                            }
                    }
                    confF.Close();
                }
            }catch(Exception ex){
                if(confF != null)
                    confF.Close();
                MessageBox.Show("Errore nell' apertura del file");
                return;
            }
        }
        


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
           
        }

        public void button1_Click(object sender, EventArgs e)
        {

            try
            {
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    if (listView1.Items[i].SubItems[0].Text == textBox1.Text && listView1.Items[i].SubItems[1].Text == textBox2.Text &&  !listView1.FocusedItem.SubItems[3].Text.Equals(" ") )
                    {
                        MessageBox.Show("Esiste già un server con quell' indirizzo e porta");
                        return;
                    }

                    if (listView1.Items[i].SubItems[0].Text == textBox1.Text && listView1.Items[i].SubItems[1].Text == textBox2.Text && listView1.FocusedItem.SubItems[3].Text.Equals(" "))
                    {
                        client.serverAddress = IPAddress.Parse(textBox1.Text);
                        client.port = Convert.ToInt32(textBox2.Text);
                        client.password = textBox3.Text;

                        client.numClients = clientConnected;
                        client.form = this;
                        threadClient = new Thread(client.StartClient);

                        threadClient.Start();
                        listView1.Items[i].SubItems[3].Text = "Attendo risposta dal server";
                        return;
                    }

                }

                //client.ipHostInfo = Dns.GetHostEntry(textBox1.Text);
                client.serverAddress = IPAddress.Parse(textBox1.Text);
                client.port = Convert.ToInt32(textBox2.Text);
                client.password = textBox3.Text;

                client.numClients = clientConnected;
                client.form = this;
                threadClient = new Thread(client.StartClient);

                threadClient.Start();

                ListViewItem item = new ListViewItem(textBox1.Text, client.numClients);
                // Place a check mark next to the item.
                item.Checked = true;

                item.SubItems.Add(client.port.ToString());
                item.SubItems.Add(" ");
                item.SubItems.Add("Attendo risposta dal server");
                item.SubItems.Add(GetTimestamp(DateTime.Now));
                //Add the items to the ListView.
                listView1.Items.AddRange(new ListViewItem[] { item });
                listView1.Items[listView1.Items.Count - 1].Focused = true;


               


            }
            catch (FormatException ex)
            {
                MessageBox.Show("Formato indirizzo non valido");
                return;
            }
            catch(Exception ex) {

                MessageBox.Show("Errore nella connessione" + ex.ToString());
                return;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

            textBox1.Text = listView1.FocusedItem.Text;
            textBox2.Text = listView1.FocusedItem.SubItems[1].Text;
        }
        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("HH:mm dd/MM/yyyy");
        }

       

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = string.Empty;
            textBox2.Text = string.Empty;
            textBox3.Text = string.Empty;
        }

        private void button3_Click(object sender, EventArgs e)
        {
          //  try
          //  {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("Devi prima selezionare un server");
                return;
            }
            if (listView1.FocusedItem.SubItems[3].Text == "Pronto per il controllo" && listView1.FocusedItem.SubItems[3].Text != "Connesso")
                {
 

                    if (listView1.Items.Count > 1)
                    {
                        Console.WriteLine("DIMENSIONE LISTA + " + listView1.Items.Count);
                        if (client == null)
                        {
                            MessageBox.Show("Devi prima effettuare una connessione ad un server");
                            return;
                        }

                        client.addormentaServer();
                    }

                    indexListView = listView1.FocusedItem.Index;
                    Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!" + indexListView);
                    client.currentSocket = client.listSocket[indexListView];
                    Console.WriteLine(client.currentSocket.RemoteEndPoint);


                    client.svegliaServer();

                    startControl();/***piuttosto che avviare un nuovo thread, usiamo quello esistente***/

                   // Thread ControlThread = new Thread(new ThreadStart(startControl));
                   // ControlThread.Start();
                }
                else
                {
                    MessageBox.Show("Devi prima effettuare la connessione al server");
                    return;
                }
                 
           /*}catch(Exception ex){
                Console.WriteLine(ex.Message);

                listView1.Items.RemoveAt(listView1.FocusedItem.Index);
                //client.closeAllSocket();
                MessageBox.Show("Errore nel tentativo di controllo");
                return;
            }*/
             
        }


          [STAThreadAttribute]
        public void runMouseArea(){
              try{
                formMouse = new Form2(client,this);  
                    Application.Run(formMouse);
              }
              catch (Exception ex)
              {
                  formMouse = null;
                
                  return;
              }
        }


         [STAThreadAttribute]
        public void startControl() {

        //  try{

            byte[] bytes = new byte[1024];
           
            String response;
            //invio messaggio al server, che sto inziando a controllarlo
            
            byte[] msg = Encoding.ASCII.GetBytes("CONTROLLO\r\n<EOF>");
            
            // Send the data through the socket.
            int bytesSent = client.currentSocket.Send(msg);
            int scaleX = Screen.PrimaryScreen.WorkingArea.Width;
            int scaleY = Screen.PrimaryScreen.WorkingArea.Height;
            //aspetto il +OK del server
            // Receive the response from the remote device.
           int bytesRec = client.currentSocket.Receive(bytes);

           response = Encoding.ASCII.GetString(bytes, 0, bytesRec);

           if (response == "+OK\r\n")
           {
               string coo = scaleX.ToString() + "," + scaleY.ToString() + "\0";
               byte[] coordinate = Encoding.ASCII.GetBytes(coo);
               int cord = client.currentSocket.Send(coordinate);

               changeStatusListView("Connesso", false);
             
             
               if (area == null)
               {
                   area = new Thread(new ThreadStart(runMouseArea));
                   area.Start();
               }
               
               MyClipBoard clip = new MyClipBoard();
               clip.setClient(client);

               clip.setForm(this);

              /* if (tClip != null && tClip.IsAlive)
               {
                   tClip.Abort();
                   //tClip = null;
               }*/

             //  if (tClip == null)
               //{
                   tClip = new Thread(new ThreadStart(clip.receiveDataClipBoard));
                   tClip.IsBackground = true;
                   tClip.Start();
               //}
           }
           else {
               MessageBox.Show("Controllo server NON riuscito");
           }
           
             /* }catch(Exception ex){
                  area = null;
                //listView1.Items.RemoveAt(listView1.FocusedItem.Index);
                //client.closeAllSocket();
                MessageBox.Show("Errore nel tentativo di controllo");
                return;
            }*/
           
        }





         protected override void WndProc(ref System.Windows.Forms.Message m)
         {
             // defined in winuser.h
             const int WM_DRAWCLIPBOARD = 0x308;
             const int WM_CHANGECBCHAIN = 0x030D;
             const int WM_CUT = 0x0300;


            
             switch (m.Msg)
             {
                 case 0x0312:
                     this.Activate();         
                      int id = m.WParam.ToInt32();
                    
                      switchWithHotKey(id);
                     break;
                 case WM_CUT:
                     break;
                 case WM_DRAWCLIPBOARD:
                     if (ricevutoDalSocket)
                     {
                         if (Clipboard.ContainsText(TextDataFormat.UnicodeText))
                         {
                             Console.WriteLine(Clipboard.GetText(TextDataFormat.UnicodeText));
                             if (clipb != null)
                             {
                                 clipb.SetClient(client);
                                 clipb.textselected = (string)Clipboard.GetDataObject().GetData(DataFormats.UnicodeText);
                                 clipb.valore = 0;


                                 if (invioClip != null && invioClip.IsAlive)
                                 {
                                     MessageBox.Show("Attenzione, trasferimento clipboard in corso, interrompere prima il trasferimento corrente");

                                 }
                                 else
                                 {
                                     invioClip = new Thread(new ThreadStart(clipb.sendData));
                                     invioClip.Start();
                                 }
                             }
                         }
                         if (Clipboard.ContainsFileDropList())
                         {
                             if (clipb != null)
                             {
                                 clipb.SetClient(client);
                                 clipb.filename.Clear();
                                 clipb.filetotallen = 0;

                                 for (int i = 0; i < Clipboard.GetFileDropList().Count; i++)
                                 {
                                     Console.WriteLine("sono qui dentro " + Clipboard.GetFileDropList()[i].ToString());
                                     clipb.valore = 1;
                                     clipb.setparam(Clipboard.GetFileDropList()[i].ToString());
                                     
                                 }
                                 if (invioClip != null && invioClip.IsAlive)
                                 {
                                     MessageBox.Show("Attenzione, trasferimento clipboard in corso, interrompere prima il trasferimento corrente");

                                 }
                                 else
                                 {
                                     invioClip = new Thread(new ThreadStart(clipb.sendData));
                                     invioClip.Start();
                                 }
                                 
                                     
                                 
                             }
                         }
                         SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                     }
                     break;

                 case WM_CHANGECBCHAIN:
                     if (m.WParam == nextClipboardViewer)
                         nextClipboardViewer = m.LParam;
                     else
                         SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                     break;

                 default:
                     base.WndProc(ref m);
                     break;
             }

         }

         private void button4_Click(object sender, EventArgs e)
         {
             try{

                 if(listView1.FocusedItem.SubItems[3].Text.Equals("Connesso")){
                     MessageBox.Show("Stai già controllando questo server");
                     return;
                 }


             if (listView1.SelectedItems.Count ==0)
             {
                 MessageBox.Show("Per impostare una hotkey devi prima selezionare un server");
                 return;
             }
             Keys k = (hotKeyValues!="" && hotKeyValues!=null) ? (Keys)Int32.Parse(hotKeyValues) : Keys.None;
             HotKeyWindows hotkeyform = new HotKeyWindows(k);
             if (hotkeyform.ShowDialog() == DialogResult.OK)
             {
                 UnregisterHotKey(this.Handle, listView1.FocusedItem.Index);
                 bool success = RegisterHotKey(this.Handle, listView1.FocusedItem.Index, hotkeyform.Win32Modifiers, hotkeyform.CharCode);
                 if (success)
                 {

                     //(int)hotkeyform.Keys).ToString()
                     idHotkey++;
                     listView1.FocusedItem.SubItems[2].Text = hotkeyform.Keys.ToString();
                     Console.WriteLine(hotkeyform.combinazione);

                 }
                 else {
                     MessageBox.Show("Combinazione già registrata");
                 }
             }
             }
             catch (Exception ex)
             {
                 //listView1.Items.RemoveAt(listView1.FocusedItem.Index);
                 //client.closeAllSocket();
                 MessageBox.Show("Errore nela definizione di un' hotkey");
                 return;
             }
         }

        // questo serve per capire se la linea presa è un commento nel file.
         private static bool isComment(string line)
         {
             if (line.StartsWith("#")) return true;
             return false;
         }
        // questo serve a riconsocere le parole dentro le virgolette sia doppie che singole e a levarle dalla parola.
         private static string getContent(string line)
         {
             try
             {
                 string pattern = "(\"[^\"]*\")|('[^\r]*)(\r\n)?";
                 Regex re = new Regex(pattern);
                 Match res = re.Match(line);

                 return res.Value.Replace("'", "");
             }
             catch { return null; }
         }
        // questi sono i parametri inziali che trovo nel file
         private enum AccettableStrings
         {
             ADDRESS,
             PORT,
             PASSWORD,
             HOTKEYS,
             ENDSERVER
         };

         private void Form1_FormClosing(object sender, FormClosingEventArgs e)
         {
             if(listView1.Items.Count>0)
             if (MessageBox.Show("Vuoi salvare i settaggi correnti?",
         "Attenzione",
         MessageBoxButtons.YesNo) == DialogResult.Yes)
             {
                 StreamWriter stream = new StreamWriter(Application.StartupPath + "\\data.txt",false);
                 for (int i = 0; i < listView1.Items.Count; i++)
                 {

                     stream.WriteLine("ADDRESS " + "'" + listView1.Items[i].SubItems[0].Text + "'");
                     stream.WriteLine("PORT " + "'" + listView1.Items[i].SubItems[1].Text + "'");
                     stream.WriteLine("HOTKEYS " + "'" + listView1.Items[i].SubItems[2].Text + "'");
                     stream.WriteLine("ENDSERVER");
                 }
                 stream.Close();
                 
             }
            
         }

         public void switchWithHotKey(int id) {

          try
            {
                if (listView1.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Devi prima selezionare un server");
                    return;
                }
                if (listView1.Items[id].SubItems[3].Text.Equals("Connesso"))
                {
                    MessageBox.Show("Stai già controllando questo server");
                    return;
                }
                if (listView1.FocusedItem.SubItems[3].Text == "Pronto per il controllo")
                {
             listView1.Items[id].Focused = true;
             
             if (listView1.Items.Count > 1)
             {
                 Console.WriteLine("DIMENSIONE LISTA  " + listView1.Items.Count);
                 client.addormentaServer();
             }
             indexListView = listView1.FocusedItem.Index;
             Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!" + indexListView);
             client.currentSocket = client.listSocket[indexListView];
             Console.WriteLine(client.currentSocket.RemoteEndPoint);


             client.svegliaServer();


             Thread ControlThread = new Thread(new ThreadStart(startControl));
             ControlThread.Start();
                }
                else
                {
                    MessageBox.Show("Non hai ancora effettuato la connessione al server");
                    return;
                }
            }
              catch (Exception ex)
              {
                  listView1.Items.RemoveAt(listView1.FocusedItem.Index);
                  //client.closeAllSocket();
                  MessageBox.Show("Errore nel tentativo di controllo");
                  return;
              }
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
    }
}
