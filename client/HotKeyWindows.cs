using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Progetto_Malnati
{
    public partial class HotKeyWindows : Form
    {
        //bytecode di alt certl e shift
        private const byte ModAlt = 1, ModControl = 2, ModShift = 4;
        public String combinazione;
      
        public HotKeyWindows(Keys k)
        {
            InitializeComponent();

            /****Seleziona dalla comboBox la lettera o il numero scelto dall' utente****/
            for (int i = 65; i < 91; i++)
                comboBox1.Items.Add(" " + (char)i);

            for (int i = 48; i < 58; i++)
                comboBox1.Items.Add(" " + (char)i);

           if(k!=Keys.None)
                Keys = k;

            comboBox1.SelectedIndex = 0;

        }

        public override int GetHashCode()
        {
            return (Int32)Keys;
        }

     /***Set e Get dei valori della ComboBox***/
        public byte CharCode
        {
            get { return (byte)((string)comboBox1.SelectedItem)[1]; }
            set
            {
                foreach (object item in comboBox1.Items)
                {
                    if (item.ToString() == " " + (char)value)
                    {
                        comboBox1.SelectedItem = item;
                        return;
                    }
                }
            }
        }


        /***Ritorna i valori delle checkBox***/
        public byte Win32Modifiers
        {
            get
            {
                byte toReturn = 0;
                if (ctrl.Checked)
                    toReturn += ModControl;
                if (alt.Checked)
                    toReturn += ModAlt;
                if (shift.Checked)
                    toReturn += ModShift;
                return toReturn;
            }
        }


        //Set e Get valori della combobox con quelli della checkbox
        public Keys Keys
        {
            get
            {
                Keys k = (Keys)CharCode;
                if (shift.Checked)
                    k |= Keys.Shift;
                if (ctrl.Checked)
                    k |= Keys.Control;
                if (alt.Checked)
                    k |= Keys.Alt;
                return k;
            }
            set
            {
                Keys k = (Keys)value;
                if (((int)k & (int)Keys.Shift) == (int)Keys.Shift)
                    Shift = true;
                if (((int)k & (int)Keys.Control) == (int)Keys.Control)
                    Control = true;
                if (((int)k & (int)Keys.Alt) == (int)Keys.Alt)
                    Alt = true;

                CharCode = CharCodeFromKeys(k);
            }
        }

        /**Se sono a true setta il valore nella checkbox**/
        public bool Shift
        {
            get{return shift.Checked; }
            set { shift.Checked = value; }
        }

        /**Se sono a true setta il valore nella checkbox**/
        public bool Control
        {
            get {return ctrl.Checked; }
            set { ctrl.Checked = value; }
        }

        /**Se sono a true setta il valore nella checkbox**/
        public bool Alt
        {
            get {return alt.Checked; }
            set { alt.Checked = value; }
        }


        public static byte Win32ModifiersFromKeys(Keys k)
        {
            byte total = 0;

            if (((int)k & (int)Keys.Shift) == (int)Keys.Shift)
            {
                total += ModShift;
                //combinazione += "SHIFT ";
            }
            if (((int)k & (int)Keys.Control) == (int)Keys.Control)
            {
               // combinazione += "CTRL ";
                total += ModControl;
            }
            if (((int)k & (int)Keys.Alt) == (int)Keys.Alt)
            {
                //combinazione += "ALT ";
                total += ModAlt;
            }

            return total;
        }

        //**Torna il charCode della keys**//
        public static byte CharCodeFromKeys(Keys k)
        {
            byte charCode = 0;
            if ((k.ToString().Length == 1) || ((k.ToString().Length > 2) && (k.ToString()[1] == ',')))
                charCode = (byte)k.ToString()[0];
            else if ((k.ToString().Length > 3) && (k.ToString()[0] == 'D') && (k.ToString()[2] == ','))
                charCode = (byte)k.ToString()[1];
            return charCode;
        }

        private void button1_Click(object sender, EventArgs e)
        {
           
            combinazione += (string)comboBox1.SelectedItem;
            DialogResult = DialogResult.OK;
            Close();
        }

       

    }
}
