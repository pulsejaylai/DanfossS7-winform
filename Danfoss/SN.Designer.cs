namespace Danfoss
{
    partial class SN
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
            this.sninput = new System.Windows.Forms.TextBox();
            this.OK = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // sninput
            // 
            this.sninput.Location = new System.Drawing.Point(145, 28);
            this.sninput.Name = "sninput";
            this.sninput.Size = new System.Drawing.Size(494, 26);
            this.sninput.TabIndex = 0;
            this.sninput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.sninput_KeyDown);
            // 
            // OK
            // 
            this.OK.Location = new System.Drawing.Point(71, 96);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(99, 39);
            this.OK.TabIndex = 1;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(613, 96);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(142, 39);
            this.Cancel.TabIndex = 2;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // SN
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 147);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.sninput);
            this.Name = "SN";
            this.Text = "SN";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox sninput;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.Button Cancel;
    }
}