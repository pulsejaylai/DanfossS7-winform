namespace Danfoss
{
    partial class SampleSN
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
            this.samplesninput = new System.Windows.Forms.TextBox();
            this.OK = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // samplesninput
            // 
            this.samplesninput.Location = new System.Drawing.Point(156, 28);
            this.samplesninput.Name = "samplesninput";
            this.samplesninput.Size = new System.Drawing.Size(494, 26);
            this.samplesninput.TabIndex = 1;
            this.samplesninput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.samplesninput_KeyDown);
            // 
            // OK
            // 
            this.OK.Location = new System.Drawing.Point(51, 81);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(99, 39);
            this.OK.TabIndex = 2;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(671, 81);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(122, 39);
            this.Cancel.TabIndex = 3;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // SampleSN
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(805, 132);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.samplesninput);
            this.Name = "SampleSN";
            this.Text = "SampleSN";
            this.Load += new System.EventHandler(this.SampleSN_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox samplesninput;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.Button Cancel;
    }
}