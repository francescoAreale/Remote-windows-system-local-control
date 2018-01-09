using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Generic;
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
namespace WindowsFormsApplication3
{
    class KeybdTCP
    {
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        
        const int VK_UP = 0x26; //up key
        const int VK_DOWN = 0x28;  //down key
        const int VK_LEFT = 0x25;
        const int VK_RIGHT = 0x27;
        const uint KEYEVENTF_KEYUP = 0x0002;
        const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        ManualResetEvent autoEvent = null;
        [Flags]
        public enum keybdEventFlagsAPI
        {
            ///<summary>
            ///Left mouse button
            ///</summary>
            KEY_LBUTTON = 0x01,
            ///<summary>
            ///Right mouse button
            ///</summary>
            KEY_RBUTTON = 0x02,
            ///<summary>
            ///Control-break processing
            ///</summary>
            KEY_CANCEL = 0x03,
            ///<summary>
            ///Middle mouse button (three-button mouse)
            ///</summary>
            KEY_MBUTTON = 0x04,
            ///<summary>
            ///Windows 2000/XP: X1 mouse button
            ///</summary>
            KEY_XBUTTON1 = 0x05,
            ///<summary>
            ///Windows 2000/XP: X2 mouse button
            ///</summary>
            KEY_XBUTTON2 = 0x06,
            ///<summary>
            ///BACKSPACE key
            ///</summary>
            KEY_BACK = 0x08,
            ///<summary>
            ///TAB key
            ///</summary>
            KEY_TAB = 0x09,
            ///<summary>
            ///CLEAR key
            ///</summary>
            KEY_CLEAR = 0x0C,
            ///<summary>
            ///ENTER key
            ///</summary>
            KEY_RETURN = 0x0D,
            ///<summary>
            ///SHIFT key
            ///</summary>
            KEY_ShiftKey = 0x10,
            ///<summary>
            ///CTRL key
            ///</summary>
            CONTROL = 0x11,
            ///<summary>
            ///ALT key
            ///</summary>
            KEY_MENU = 0x12,
            ///<summary>
            ///PAUSE key
            ///</summary>
            KEY_PAUSE = 0x13,
            ///<summary>
            ///CAPS LOCK key
            ///</summary>
            KEY_CAPITAL = 0x14,
            ///<summary>
            ///Input Method Editor (IME) Kana mode
            ///</summary>
            KEY_KANA = 0x15,
            ///<summary>
            ///IME Hangul mode
            ///</summary>
            KEY_HANGUL = 0x15,
            ///<summary>
            ///IME Junja mode
            ///</summary>
            KEY_JUNJA = 0x17,
            ///<summary>
            ///IME final mode
            ///</summary>
            KEY_FINAL = 0x18,
            ///<summary>
            ///IME Hanja mode
            ///</summary>
            KEY_HANJA = 0x19,
            ///<summary>
            ///IME Kanji mode
            ///</summary>
            KEY_KANJI = 0x19,
            ///<summary>
            ///ESC key
            ///</summary>
            KEY_ESCAPE = 0x1B,
            ///<summary>
            ///IME convert
            ///</summary>
            KEY_CONVERT = 0x1C,
            ///<summary>
            ///IME nonconvert
            ///</summary>
            KEY_NONCONVERT = 0x1D,
            ///<summary>
            ///IME accept
            ///</summary>
            KEY_ACCEPT = 0x1E,
            ///<summary>
            ///IME mode change request
            ///</summary>
            KEY_MODECHANGE = 0x1F,
            ///<summary>
            ///SPACEBAR
            ///</summary>
            KEY_SPACE = 0x20,
            ///<summary>
            ///PAGE UP key
            ///</summary>
            KEY_PRIOR = 0x21,
            ///<summary>
            ///PAGE DOWN key
            ///</summary>
            KEY_NEXT = 0x22,
            ///<summary>
            ///END key
            ///</summary>
            KEY_END = 0x23,
            ///<summary>
            ///HOME key
            ///</summary>
            KEY_HOME = 0x24,
            ///<summary>
            ///LEFT ARROW key
            ///</summary>
            KEY_LEFT = 0x25,
            ///<summary>
            ///UP ARROW key
            ///</summary>
            KEY_UP = 0x26,
            ///<summary>
            ///RIGHT ARROW key
            ///</summary>
            KEY_RIGHT = 0x27,
            ///<summary>
            ///DOWN ARROW key
            ///</summary>
            KEY_DOWN = 0x28,
            ///<summary>
            ///SELECT key
            ///</summary>
            KEY_SELECT = 0x29,
            ///<summary>
            ///PRINT key
            ///</summary>
            KEY_PRINT = 0x2A,
            ///<summary>
            ///EXECUTE key
            ///</summary>
            KEY_EXECUTE = 0x2B,
            ///<summary>
            ///PRINT SCREEN key
            ///</summary>
            KEY_SNAPSHOT = 0x2C,
            ///<summary>
            ///INS key
            ///</summary>
            KEY_INSERT = 0x2D,
            ///<summary>
            ///DEL key
            ///</summary>
            KEY_DELETE = 0x2E,
            ///<summary>
            ///HELP key
            ///</summary>
            KEY_HELP = 0x2F,
            ///<summary>
            ///0 key
            ///</summary>
            D0 = 0x30,
            ///<summary>
            ///1 key
            ///</summary>
            D1 = 0x31,
            ///<summary>
            ///2 key
            ///</summary>
            D2 = 0x32,
            ///<summary>
            ///3 key
            ///</summary>
            D3 = 0x33,
            ///<summary>
            ///4 key
            ///</summary>
            D4 = 0x34,
            ///<summary>
            ///5 key
            ///</summary>
            D5 = 0x35,
            ///<summary>
            ///6 key
            ///</summary>
            D6 = 0x36,
            ///<summary>
            ///7 key
            ///</summary>
            D7 = 0x37,
            ///<summary>
            ///8 key
            ///</summary>
            D8 = 0x38,
            ///<summary>
            ///9 key
            ///</summary>
            D9 = 0x39,
            ///<summary>
            ///A key
            ///</summary>
            KEY_A = 0x41,
            ///<summary>
            ///B key
            ///</summary>
            KEY_B = 0x42,
            ///<summary>
            ///C key
            ///</summary>
            KEY_C = 0x43,
            ///<summary>
            ///D key
            ///</summary>
            KEY_D = 0x44,
            ///<summary>
            ///E key
            ///</summary>
            KEY_E = 0x45,
            ///<summary>
            ///F key
            ///</summary>
            KEY_F = 0x46,
            ///<summary>
            ///G key
            ///</summary>
            KEY_G = 0x47,
            ///<summary>
            ///H key
            ///</summary>
            KEY_H = 0x48,
            ///<summary>
            ///I key
            ///</summary>
            KEY_I = 0x49,
            ///<summary>
            ///J key
            ///</summary>
            KEY_J = 0x4A,
            ///<summary>
            ///K key
            ///</summary>
            KEY_K = 0x4B,
            ///<summary>
            ///L key
            ///</summary>
            KEY_L = 0x4C,
            ///<summary>
            ///M key
            ///</summary>
            KEY_M = 0x4D,
            ///<summary>
            ///N key
            ///</summary>
            KEY_N = 0x4E,
            ///<summary>
            ///O key
            ///</summary>
            KEY_O = 0x4F,
            ///<summary>
            ///P key
            ///</summary>
            KEY_P = 0x50,
            ///<summary>
            ///Q key
            ///</summary>
            KEY_Q = 0x51,
            ///<summary>
            ///R key
            ///</summary>
            KEY_R = 0x52,
            ///<summary>
            ///S key
            ///</summary>
            KEY_S = 0x53,
            ///<summary>
            ///T key
            ///</summary>
            KEY_T = 0x54,
            ///<summary>
            ///U key
            ///</summary>
            KEY_U = 0x55,
            ///<summary>
            ///V key
            ///</summary>
            KEY_V = 0x56,
            ///<summary>
            ///W key
            ///</summary>
            KEY_W = 0x57,
            ///<summary>
            ///X key
            ///</summary>
            KEY_X = 0x58,
            ///<summary>
            ///Y key
            ///</summary>
            KEY_Y = 0x59,
            ///<summary>
            ///Z key
            ///</summary>
            KEY_Z = 0x5A,
            ///<summary>
            ///Left Windows key (Microsoft Natural keyboard) 
            ///</summary>
            KEY_LWIN = 0x5B,
            ///<summary>
            ///Right Windows key (Natural keyboard)
            ///</summary>
            KEY_RWIN = 0x5C,
            ///<summary>
            ///Applications key (Natural keyboard)
            ///</summary>
            KEY_APPS = 0x5D,
            ///<summary>
            ///Computer Sleep key
            ///</summary>
            KEY_SLEEP = 0x5F,
            ///<summary>
            ///Numeric keypad 0 key
            ///</summary>
            NUMPAD0 = 0x60,
            ///<summary>
            ///Numeric keypad 1 key
            ///</summary>
            NUMPAD1 = 0x61,
            ///<summary>
            ///Numeric keypad 2 key
            ///</summary>
            NUMPAD2 = 0x62,
            ///<summary>
            ///Numeric keypad 3 key
            ///</summary>
            NUMPAD3 = 0x63,
            ///<summary>
            ///Numeric keypad 4 key
            ///</summary>
            NUMPAD4 = 0x64,
            ///<summary>
            ///Numeric keypad 5 key
            ///</summary>
            NUMPAD5 = 0x65,
            ///<summary>
            ///Numeric keypad 6 key
            ///</summary>
            NUMPAD6 = 0x66,
            ///<summary>
            ///Numeric keypad 7 key
            ///</summary>
            NUMPAD7 = 0x67,
            ///<summary>
            ///Numeric keypad 8 key
            ///</summary>
            NUMPAD8 = 0x68,
            ///<summary>
            ///Numeric keypad 9 key
            ///</summary>
            NUMPAD9 = 0x69,
            ///<summary>
            ///Multiply key
            ///</summary>
            KEY_MULTIPLY = 0x6A,
            ///<summary>
            ///Add key
            ///</summary>
            KEY_ADD = 0x6B,
            ///<summary>
            ///Separator key
            ///</summary>
            KEY_SEPARATOR = 0x6C,
            ///<summary>
            ///Subtract key
            ///</summary>
            KEY_SUBTRACT = 0x6D,
            ///<summary>
            ///Decimal key
            ///</summary>
            KEY_DECIMAL = 0x6E,
            ///<summary>
            ///Divide key
            ///</summary>
            KEY_DIVIDE = 0x6F,
            ///<summary>
            ///F1 key
            ///</summary>
            F1 = 0x70,
            ///<summary>
            ///F2 key
            ///</summary>
            F2 = 0x71,
            ///<summary>
            ///F3 key
            ///</summary>
            F3 = 0x72,
            ///<summary>
            ///F4 key
            ///</summary>
            F4 = 0x73,
            ///<summary>
            ///F5 key
            ///</summary>
            F5 = 0x74,
            ///<summary>
            ///F6 key
            ///</summary>
            F6 = 0x75,
            ///<summary>
            ///F7 key
            ///</summary>
            F7 = 0x76,
            ///<summary>
            ///F8 key
            ///</summary>
            F8 = 0x77,
            ///<summary>
            ///F9 key
            ///</summary>
            F9 = 0x78,
            ///<summary>
            ///F10 key
            ///</summary>
            F10 = 0x79,
            ///<summary>
            ///F11 key
            ///</summary>
            F11 = 0x7A,
            ///<summary>
            ///F12 key
            ///</summary>
            F12 = 0x7B,
            ///<summary>
            ///F13 key
            ///</summary>
            F13 = 0x7C,
            ///<summary>
            ///F14 key
            ///</summary>
            F14 = 0x7D,
            ///<summary>
            ///F15 key
            ///</summary>
            F15 = 0x7E,
            ///<summary>
            ///F16 key
            ///</summary>
            F16 = 0x7F,
            ///<summary>
            ///F17 key  
            ///</summary>
            F17 = 0x80,
            ///<summary>
            ///F18 key  
            ///</summary>
            F18 = 0x81,
            ///<summary>
            ///F19 key  
            ///</summary>
            F19 = 0x82,
            ///<summary>
            ///F20 key  
            ///</summary>
            F20 = 0x83,
            ///<summary>
            ///F21 key  
            ///</summary>
            F21 = 0x84,
            ///<summary>
            ///F22 key, (PPC only) Key used to lock device.
            ///</summary>
            F22 = 0x85,
            ///<summary>
            ///F23 key  
            ///</summary>
            F23 = 0x86,
            ///<summary>
            ///F24 key  
            ///</summary>
            F24 = 0x87,
            ///<summary>
            ///NUM LOCK key
            ///</summary>
            NUMLOCK = 0x90,
            ///<summary>
            ///SCROLL LOCK key
            ///</summary>
            SCROLL = 0x91,
            ///<summary>
            ///Left SHIFT key
            ///</summary>
            LSHIFT = 0xA0,
            ///<summary>
            ///Right SHIFT key
            ///</summary>
            RSHIFT = 0xA1,
            ///<summary>
            ///Left CONTROL key
            ///</summary>
            LCONTROL = 0xA2,
            ///<summary>
            ///Right CONTROL key
            ///</summary>
            RCONTROL = 0xA3,
            ///<summary>
            ///Left MENU key
            ///</summary>
            LMENU = 0xA4,
            ///<summary>
            ///Right MENU key
            ///</summary>
            RMENU = 0xA5,
            ///<summary>
            ///Windows 2000/XP: Browser Back key
            ///</summary>
            BROWSER_BACK = 0xA6,
            ///<summary>
            ///Windows 2000/XP: Browser Forward key
            ///</summary>
            BROWSER_FORWARD = 0xA7,
            ///<summary>
            ///Windows 2000/XP: Browser Refresh key
            ///</summary>
            BROWSER_REFRESH = 0xA8,
            ///<summary>
            ///Windows 2000/XP: Browser Stop key
            ///</summary>
            BROWSER_STOP = 0xA9,
            ///<summary>
            ///Windows 2000/XP: Browser Search key 
            ///</summary>
            BROWSER_SEARCH = 0xAA,
            ///<summary>
            ///Windows 2000/XP: Browser Favorites key
            ///</summary>
            BROWSER_FAVORITES = 0xAB,
            ///<summary>
            ///Windows 2000/XP: Browser Start and Home key
            ///</summary>
            BROWSER_HOME = 0xAC,
            ///<summary>
            ///Windows 2000/XP: Volume Mute key
            ///</summary>
            VOLUME_MUTE = 0xAD,
            ///<summary>
            ///Windows 2000/XP: Volume Down key
            ///</summary>
            VOLUME_DOWN = 0xAE,
            ///<summary>
            ///Windows 2000/XP: Volume Up key
            ///</summary>
            VOLUME_UP = 0xAF,
            ///<summary>
            ///Windows 2000/XP: Next Track key
            ///</summary>
            MEDIA_NEXT_TRACK = 0xB0,
            ///<summary>
            ///Windows 2000/XP: Previous Track key
            ///</summary>
            MEDIA_PREV_TRACK = 0xB1,
            ///<summary>
            ///Windows 2000/XP: Stop Media key
            ///</summary>
            MEDIA_STOP = 0xB2,
            ///<summary>
            ///Windows 2000/XP: Play/Pause Media key
            ///</summary>
            MEDIA_PLAY_PAUSE = 0xB3,
            ///<summary>
            ///Windows 2000/XP: Start Mail key
            ///</summary>
            LAUNCH_MAIL = 0xB4,
            ///<summary>
            ///Windows 2000/XP: Select Media key
            ///</summary>
            LAUNCH_MEDIA_SELECT = 0xB5,
            ///<summary>
            ///Windows 2000/XP: Start Application 1 key
            ///</summary>
            LAUNCH_APP1 = 0xB6,
            ///<summary>
            ///Windows 2000/XP: Start Application 2 key
            ///</summary>
            LAUNCH_APP2 = 0xB7,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard.
            ///</summary>
            OEM_1 = 0xBA,
            ///<summary>
            ///Windows 2000/XP: For any country/region, the '+' key
            ///</summary>
            OEM_PLUS = 0xBB,
            ///<summary>
            ///Windows 2000/XP: For any country/region, the ',' key
            ///</summary>
            OEM_COMMA = 0xBC,
            ///<summary>
            ///Windows 2000/XP: For any country/region, the '-' key
            ///</summary>
            OEM_MINUS = 0xBD,
            ///<summary>
            ///Windows 2000/XP: For any country/region, the '.' key
            ///</summary>
            OEM_PERIOD = 0xBE,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard.
            ///</summary>
            OEM_2 = 0xBF,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard. 
            ///</summary>
            OEM_3 = 0xC0,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard. 
            ///</summary>
            OEM_4 = 0xDB,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard. 
            ///</summary>
            OEM_5 = 0xDC,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard. 
            ///</summary>
            OEM_6 = 0xDD,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard. 
            ///</summary>
            OEM_7 = 0xDE,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard.
            ///</summary>
            OEM_8 = 0xDF,
            ///<summary>
            ///Windows 2000/XP: Either the angle bracket key or the backslash key on the RT 102-key keyboard
            ///</summary>
            OEM_102 = 0xE2,
            ///<summary>
            ///Windows 95/98/Me, Windows NT 4.0, Windows 2000/XP: IME PROCESS key
            ///</summary>
            PROCESSKEY = 0xE5,
            ///<summary>
            ///Windows 2000/XP: Used to pass Unicode characters as if they were keystrokes.
            ///The VK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods. For more information,
            ///see Remark in KEYBDINPUT, SendInput, WM_KEYDOWN, and WM_KEYUP
            ///</summary>
            PACKET = 0xE7,
            ///<summary>
            ///Attn key
            ///</summary>
            ATTN = 0xF6,
            ///<summary>
            ///CrSel key
            ///</summary>
            CRSEL = 0xF7,
            ///<summary>
            ///ExSel key
            ///</summary>
            EXSEL = 0xF8,
            ///<summary>
            ///Erase EOF key
            ///</summary>
            EREOF = 0xF9,
            ///<summary>
            ///Play key
            ///</summary>
            PLAY = 0xFA,
            ///<summary>
            
            ///</summary>
            ZOOM = 0xFB,
            ///<summary>
            ///Reserved 
            ///</summary>
            NONAME = 0xFC,
            ///<summary>
            ///PA1 key
            ///</summary>
            PA1 = 0xFD,
            ///<summary>
            ///Clear key
            ///</summary>
            OEM_CLEAR = 0xFE
        }

        void press(string str)
        {
            Console.WriteLine(str);
            //  keybdEventFlagsAPI key = (keybdEventFlagsAPI)Enum.Parse(typeof(keybdEventFlagsAPI),str);
            //Press the key

            //  keybd_event(key, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);


        }
        public Socket keybsock;
        byte[] bytes = new Byte[1024];
        Socket listener;
        public MyServer server = null;
        public void SetServer(MyServer server)
        {

            this.server = server;
        }

        public void setevent(ManualResetEvent autoEvent)
        {

            this.autoEvent = autoEvent;

        }

        public void setnewserver()
        {

            IPEndPoint localEndPoint = new IPEndPoint(server.ipAddress, server.port - 1);
            listener = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            listener.Listen(10);
            keybsock = listener.Accept();
            Console.WriteLine("connessione eseguita da kebd");
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
        public void keybdrun()
        {
            IPEndPoint localEndPoint = new IPEndPoint(server.ipAddress, server.port - 1);
            listener = new Socket(AddressFamily.InterNetwork,
           SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            listener.Listen(10);


            try
            {
                server.terminateGracefully = false;
                while (!server.terminateGracefully) // in questo while tento di chiudere bene il thread tramite la variabile. Appena esco dal while chiudo tutto i socket.
                {
                    keybsock = listener.Accept(); // in attesa di connessioni  
                    server.terminatecurrentsocketKeybd = false;
                   
                   while (!server.terminatecurrentsocketKeybd)
                   {
                       StateObject state = new StateObject();
                       state.workSocket = keybsock;
                        // la begin receive fa un thread per ogni connessione. Uso un evento per bloccarla dato che è asincrona. Ogni volta che voglio terminare il thread
                        // mi basta settare la variabile e segnalare l'evento. 
                        server.KeybdreceiveDone.Reset();
                        keybsock.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                        server.KeybdreceiveDone.WaitOne();
                    }

                   if (keybsock != null) { keybsock.Close(); keybsock = null; }
                }
                closeSocket();
            }
            catch (SocketException ex)
            {

                if (keybsock != null) { keybsock.Close(); keybsock = null; }
                Console.WriteLine("sono il socket che riceve in eccezione per socket ");

                return;
            }
            catch (ObjectDisposedException ex)
            {

                if (keybsock != null) { keybsock.Close(); keybsock = null; }
                Console.WriteLine("sono il socket che riceve in eccezione disposes");
                return;

            }
            catch (Exception ex)
            {
                if (keybsock != null) { keybsock.Close(); keybsock = null; }
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
                    int bytesRead = keybsock.EndReceive(ar);



                    if (bytesRead == 0)
                    {
                        if (keybsock != null) { keybsock.Close(); keybsock = null; }
                        Console.WriteLine("il client ha chiuso la connessione");
                        server.terminatecurrentsocketKeybd = true;
                        server.KeybdreceiveDone.Set();
                        return;
                    }
                    if (bytesRead > 0)
                    {

                        string data = Encoding.UTF8.GetString(state.buffer);

                        //keybd_event(state.buffer[0], state.buffer[1],(uint)state.buffer[2], 0);
                        switch (data.Substring(1, 1))
                        {
                            case "D":

                                keybd_event(state.buffer[0], state.buffer[2], 0, 0);
                              //  if (state.buffer[0] == 164) keybd_event(state.buffer[0], 0, KEYEVENTF_KEYUP, 0);
                                break;
                            case "U":

                                keybd_event(state.buffer[0], state.buffer[2], KEYEVENTF_KEYUP, 0);

                                break;

                        }
                    }
                }

                catch (SocketException ex)
                {

                    
                    Console.WriteLine("sono il socket che riceve in eccezione ");
                    server.terminatecurrentsocketKeybd = true;
                    server.KeybdreceiveDone.Set();
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("sono il socket che riceve in eccezione: " + ex.ToString());
                    server.terminatecurrentsocketKeybd = true;
                    server.KeybdreceiveDone.Set();
                    return;
                }
            server.KeybdreceiveDone.Set();
                    
       
            }

        public void closeSocket() {
            
               if (listener!=null)
               { listener.Close(); listener=null;}
            
           
        }

    }
}

