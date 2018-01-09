using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Configuration;

namespace Progetto_Malnati
{
    static class Program
    {
        /**Variabili hook*/
        public static KeyboardHook kh;
        //public static NameValueCollection appSettings = ConfigurationManager.appSettings;
        /// <summary>
       /// Punto di ingresso principale dell'applicazione.
       /// </summary>
       [STAThread]
      static void Main()
       {  

               Application.EnableVisualStyles();
               Application.SetCompatibleTextRenderingDefault(false);
               Application.Run(new Form1());
            
       }
    }
}
