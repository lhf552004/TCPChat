namespace TCPClient
{
    partial class ChatClient
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChatClient));
            this.btnSendMsg = new System.Windows.Forms.Button();
            this.txtSendMsg = new System.Windows.Forms.RichTextBox();
            this.txtMsg = new System.Windows.Forms.RichTextBox();
            this.YourSelfpictureBox = new System.Windows.Forms.PictureBox();
            this.AnotherpictureBox = new System.Windows.Forms.PictureBox();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.YourSelfpictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AnotherpictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSendMsg
            // 
            this.btnSendMsg.Location = new System.Drawing.Point(476, 240);
            this.btnSendMsg.Name = "btnSendMsg";
            this.btnSendMsg.Size = new System.Drawing.Size(75, 62);
            this.btnSendMsg.TabIndex = 9;
            this.btnSendMsg.Text = "Send";
            this.btnSendMsg.UseVisualStyleBackColor = true;
            this.btnSendMsg.Click += new System.EventHandler(this.btnSendMsg_Click);
            // 
            // txtSendMsg
            // 
            this.txtSendMsg.BackColor = System.Drawing.Color.LightCyan;
            this.txtSendMsg.Location = new System.Drawing.Point(12, 240);
            this.txtSendMsg.Name = "txtSendMsg";
            this.txtSendMsg.Size = new System.Drawing.Size(458, 62);
            this.txtSendMsg.TabIndex = 7;
            this.txtSendMsg.Text = "";
            this.txtSendMsg.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtSendMsg_KeyUp);
            // 
            // txtMsg
            // 
            this.txtMsg.BackColor = System.Drawing.Color.Azure;
            this.txtMsg.Location = new System.Drawing.Point(12, 31);
            this.txtMsg.Name = "txtMsg";
            this.txtMsg.Size = new System.Drawing.Size(458, 203);
            this.txtMsg.TabIndex = 8;
            this.txtMsg.Text = "";
            // 
            // YourSelfpictureBox
            // 
            this.YourSelfpictureBox.Image = global::TCPClient.Properties.Resources.Male;
            this.YourSelfpictureBox.Location = new System.Drawing.Point(476, 21);
            this.YourSelfpictureBox.Name = "YourSelfpictureBox";
            this.YourSelfpictureBox.Size = new System.Drawing.Size(108, 106);
            this.YourSelfpictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.YourSelfpictureBox.TabIndex = 10;
            this.YourSelfpictureBox.TabStop = false;
            // 
            // AnotherpictureBox
            // 
            this.AnotherpictureBox.Image = global::TCPClient.Properties.Resources.Female;
            this.AnotherpictureBox.Location = new System.Drawing.Point(476, 129);
            this.AnotherpictureBox.Name = "AnotherpictureBox";
            this.AnotherpictureBox.Size = new System.Drawing.Size(108, 110);
            this.AnotherpictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.AnotherpictureBox.TabIndex = 10;
            this.AnotherpictureBox.TabStop = false;
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            // 
            // ChatClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(596, 310);
            this.Controls.Add(this.AnotherpictureBox);
            this.Controls.Add(this.YourSelfpictureBox);
            this.Controls.Add(this.btnSendMsg);
            this.Controls.Add(this.txtSendMsg);
            this.Controls.Add(this.txtMsg);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ChatClient";
            this.Text = "ChatClient";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ChatClient_FormClosing);
            this.Load += new System.EventHandler(this.ChatClient_Load);
            ((System.ComponentModel.ISupportInitialize)(this.YourSelfpictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AnotherpictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSendMsg;
        private System.Windows.Forms.RichTextBox txtSendMsg;
        private System.Windows.Forms.RichTextBox txtMsg;
        private System.Windows.Forms.PictureBox YourSelfpictureBox;
        private System.Windows.Forms.PictureBox AnotherpictureBox;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
    }
}