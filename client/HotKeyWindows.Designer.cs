namespace Progetto_Malnati
{
    partial class HotKeyWindows
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

      

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ctrl = new System.Windows.Forms.CheckBox();
            this.alt = new System.Windows.Forms.CheckBox();
            this.shift = new System.Windows.Forms.CheckBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ctrl
            // 
            this.ctrl.AutoSize = true;
            this.ctrl.Location = new System.Drawing.Point(14, 51);
            this.ctrl.Name = "ctrl";
            this.ctrl.Size = new System.Drawing.Size(54, 17);
            this.ctrl.TabIndex = 0;
            this.ctrl.Text = "CTRL";
            this.ctrl.UseVisualStyleBackColor = true;
            // 
            // alt
            // 
            this.alt.AutoSize = true;
            this.alt.Location = new System.Drawing.Point(74, 51);
            this.alt.Name = "alt";
            this.alt.Size = new System.Drawing.Size(46, 17);
            this.alt.TabIndex = 1;
            this.alt.Text = "ALT";
            this.alt.UseVisualStyleBackColor = true;
            // 
            // shift
            // 
            this.shift.AutoSize = true;
            this.shift.Location = new System.Drawing.Point(126, 51);
            this.shift.Name = "shift";
            this.shift.Size = new System.Drawing.Size(57, 17);
            this.shift.TabIndex = 2;
            this.shift.Text = "SHIFT";
            this.shift.UseVisualStyleBackColor = true;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(189, 47);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 21);
            this.comboBox1.TabIndex = 3;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(66, 91);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(164, 91);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 5;
            this.button2.Text = "CANCEL";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // HotKeyWindows
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(317, 126);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.shift);
            this.Controls.Add(this.alt);
            this.Controls.Add(this.ctrl);
            this.Name = "HotKeyWindows";
            this.Text = "HotKeyWindows";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox ctrl;
        private System.Windows.Forms.CheckBox alt;
        private System.Windows.Forms.CheckBox shift;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}