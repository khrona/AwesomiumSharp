namespace WinFormsSample
{
    partial class WebForm
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
            this.webViewBitmap = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.webViewBitmap)).BeginInit();
            this.SuspendLayout();
            // 
            // webViewBitmap
            // 
            this.webViewBitmap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webViewBitmap.Location = new System.Drawing.Point(0, 0);
            this.webViewBitmap.Name = "webViewBitmap";
            this.webViewBitmap.Size = new System.Drawing.Size(532, 533);
            this.webViewBitmap.TabIndex = 0;
            this.webViewBitmap.TabStop = false;
            // 
            // WebForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(532, 533);
            this.Controls.Add(this.webViewBitmap);
            this.Name = "WebForm";
            this.Text = "WinFormsSample";
            ((System.ComponentModel.ISupportInitialize)(this.webViewBitmap)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox webViewBitmap;
    }
}

