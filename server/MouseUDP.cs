using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

using System.Threading.Tasks;

namespace WindowsFormsApplication3
{
    class MouseUDP
    {
          // Dll import. -> method mouse_move
        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        private EndPoint point;
        private Socket receiveSocket;
        private byte[] recBuffer;
        private int speed = 1;
        MyServer server = null;
        ManualResetEvent autoEvent;
        // Flags for mouse_event api
        [Flags]
        public enum MouseEventFlagsAPI
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010,
            MOUSEEVENTF_WHEEL=0x0800,
            MOUSEEVENTF_HWHEEL=0x01000
        }

        public void getServer (MyServer server){
        
            this.server=server;
        
        
        }

       
        public  void mouserun()
        {
            try
            {
                // initilization and listening
                Console.WriteLine("sono in run");
                server.terminatecurrentsocketMouse = false;
                receiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                point = new IPEndPoint(IPAddress.Any, server.getport());
                this.recBuffer = new byte[60];
                receiveSocket.Bind(point);
                server.terminatecurrentsocket = false;
                while (!server.terminatecurrentsocketMouse)
                {
                    server.MouseUDPreceiveDone.Reset();
                    
                    receiveSocket.BeginReceiveFrom(recBuffer, 0, recBuffer.Length,
                        SocketFlags.None, ref point,
                        new AsyncCallback(MessageReceiveCallback), (object)this);
                    
                     server.MouseUDPreceiveDone.WaitOne();
                }
                if (receiveSocket != null) { receiveSocket.Close(); receiveSocket = null; }
            }
            catch (Exception ex) {

                if (receiveSocket != null) { receiveSocket.Close(); receiveSocket = null; }

            }
        }

        public void setevent(ManualResetEvent autoEvent)
        {

            this.autoEvent = autoEvent;

        }
        private void LeftClick()
        {
            // Send click to system
           // mouse_event((int)MouseEventFlagsAPI.LEFTDOWN, 0, 0, 0, 0);
        
            mouse_event((int)MouseEventFlagsAPI.LEFTUP, 0, 0, 0, 0);
            
        }
        private void RightClick()
        {
            // Send click to system
           
            mouse_event((int)MouseEventFlagsAPI.RIGHTDOWN, 0, 0, 0, 0);
            mouse_event((int)MouseEventFlagsAPI.RIGHTUP, 0, 0, 0, 0);
        }
        private void trascina() {

          
           // mouse_event((int)MouseEventFlagsAPI.LEFTDOWN, 0, 0, 0, 0);
            mouse_event((int)MouseEventFlagsAPI.LEFTDOWN, 0, 0, 0, 0);
        
        
        }
        private void SU(int n) {

           
           mouse_event((int)MouseEventFlagsAPI.MOUSEEVENTF_WHEEL, 0, 0, n, 0);
        
        }
        private void GIU(int n)
        {
            
            mouse_event((int)MouseEventFlagsAPI.MOUSEEVENTF_WHEEL, 0, 0, n, 0);

        }

        private void MessageReceiveCallback(IAsyncResult result)
        {
            EndPoint remote = new IPEndPoint(0, 0);
            string pos = "";
                
           // autoEvent.WaitOne();
            try
            {
                if (server.GetConnected())
                {
                    // get received message.
                    pos = Encoding.UTF8.GetString(recBuffer);

                    // clicked?
                    if (pos.StartsWith("click"))
                    {
                        this.LeftClick();

                        // long click = double click
                    }
                    else if (pos.StartsWith("d.click"))
                    {
                        this.LeftClick();
                        this.LeftClick();
                    }
                    else if (pos.StartsWith("r.click"))
                    {

                        this.RightClick();

                    }
                    else if (pos.StartsWith("trascina"))
                    {

                        this.trascina();

                    }
                    else if (pos.StartsWith("SU"))
                    {

                        this.SU(Convert.ToInt32(pos.Substring(2, 2)) + 30);
                        //  Console.WriteLine(Convert.ToInt32(pos.Substring(2, 2)));

                    }
                    else if (pos.StartsWith("GIU"))
                    {

                        this.GIU(Convert.ToInt32(pos.Substring(3, 3)) - 30);
                        //Console.WriteLine(Convert.ToInt32(pos.Substring(3, 3)));

                    }
                    else
                    {

                        string x = pos.Substring(0, pos.IndexOf(","));
                        // calculate delta
                        int deltaX;
                        int deltaY;
                        int.TryParse(x, out deltaX);
                        int.TryParse(pos.Substring(pos.IndexOf(",") + 1,
                            pos.IndexOf("\0") + -pos.IndexOf(",")), out deltaY);
                        // Console.WriteLine(deltaY+"//////////////////");
                        int scaleY;
                        int.TryParse(server.coordinateCLient.Substring(server.coordinateCLient.IndexOf(",") + 1, (server.coordinateCLient.IndexOf("\0") - server.coordinateCLient.IndexOf(","))), out scaleY);
                        int scaleX;
                        int.TryParse(server.coordinateCLient.Substring(0, server.coordinateCLient.IndexOf(",")), out scaleX);
                        int coordX = (Screen.PrimaryScreen.Bounds.Width * deltaX) / scaleX;
                        int coordY = (Screen.PrimaryScreen.Bounds.Height * deltaY) / scaleY;
                        //  Console.WriteLine("nuove coordinate" + coordX + " " + coordY + " " + (Screen.PrimaryScreen.Bounds.Height * deltaY) + " " + scaleY);
                        // set new point
                        System.Drawing.Point pt = System.Windows.Forms.Cursor.Position;
                        System.Windows.Forms.Cursor.Position = new System.Drawing.Point(coordX, coordY);

                    }
                }
                // End and "begin" for next package
                this.receiveSocket.EndReceiveFrom(result, ref remote);
                
            }
            catch (SocketException e)
            {
                Console.Write("i close the socket");
                // chiudo il socket per la ricezione. 
                if (receiveSocket != null) { receiveSocket.Close(); receiveSocket = null; }

                server.terminatecurrentsocketMouse = true;
            }
            catch (Exception)
            {
                Console.Write(pos);
                // chiudo il socket per la ricezione. 
                if (receiveSocket != null) { receiveSocket.Close(); receiveSocket = null; }
                server.terminatecurrentsocketMouse = true;
            }
            server.MouseUDPreceiveDone.Set();
        }

        public void closeSocket() {

            if (receiveSocket != null)
            {
                receiveSocket.Close();
                receiveSocket = null;
            }
        }
      
    }
}

