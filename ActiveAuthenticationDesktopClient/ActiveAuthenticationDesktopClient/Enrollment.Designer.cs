namespace ActiveAuthenticationDesktopClient
{
    partial class Enrollment
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Enrollment));
            this.Carrier = new System.Windows.Forms.ComboBox();
            this.Phone = new System.Windows.Forms.TextBox();
            this.Email = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.TextCode = new System.Windows.Forms.TextBox();
            this.ReSend = new System.Windows.Forms.Button();
            this.showMessage = new System.Windows.Forms.LinkLabel();
            this.EmailCode = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.TextCheck = new System.Windows.Forms.PictureBox();
            this.MailCheck = new System.Windows.Forms.PictureBox();
            this.Textx = new System.Windows.Forms.PictureBox();
            this.Mailx = new System.Windows.Forms.PictureBox();
            this.label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.TextCheck)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MailCheck)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Textx)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Mailx)).BeginInit();
            this.SuspendLayout();
            // 
            // Carrier
            // 
            this.Carrier.FormattingEnabled = true;
            this.Carrier.Location = new System.Drawing.Point(212, 255);
            this.Carrier.Name = "Carrier";
            this.Carrier.Size = new System.Drawing.Size(184, 21);
            this.Carrier.TabIndex = 1;
            this.Carrier.SelectedIndexChanged += new System.EventHandler(this.InfoChanged);
            // 
            // Phone
            // 
            this.Phone.Location = new System.Drawing.Point(212, 224);
            this.Phone.Name = "Phone";
            this.Phone.Size = new System.Drawing.Size(184, 20);
            this.Phone.TabIndex = 0;
            this.Phone.TextChanged += new System.EventHandler(this.InfoChanged);
            // 
            // Email
            // 
            this.Email.Location = new System.Drawing.Point(212, 307);
            this.Email.Name = "Email";
            this.Email.Size = new System.Drawing.Size(184, 20);
            this.Email.TabIndex = 2;
            this.Email.TextChanged += new System.EventHandler(this.InfoChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(10, 253);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(176, 20);
            this.label1.TabIndex = 10;
            this.label1.Text = "Mobile Service Provider:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(67, 222);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(119, 20);
            this.label2.TabIndex = 9;
            this.label2.Text = "Phone Number:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(71, 307);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 20);
            this.label3.TabIndex = 8;
            this.label3.Text = "Email Address:";
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(-3, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(773, 48);
            this.label4.TabIndex = 7;
            this.label4.Text = "In order to use Active Authentication you will need an active internet connection" +
    " and a mobile phone capable of receiving both text messages and email.\r\n";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // TextCode
            // 
            this.TextCode.Location = new System.Drawing.Point(658, 224);
            this.TextCode.Name = "TextCode";
            this.TextCode.Size = new System.Drawing.Size(51, 20);
            this.TextCode.TabIndex = 3;
            this.TextCode.TextChanged += new System.EventHandler(this.InfoChanged);
            // 
            // ReSend
            // 
            this.ReSend.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ReSend.Location = new System.Drawing.Point(310, 361);
            this.ReSend.Name = "ReSend";
            this.ReSend.Size = new System.Drawing.Size(161, 28);
            this.ReSend.TabIndex = 5;
            this.ReSend.Text = "Send Code";
            this.ReSend.UseVisualStyleBackColor = true;
            this.ReSend.Click += new System.EventHandler(this.ReSend_Click);
            // 
            // showMessage
            // 
            this.showMessage.AutoSize = true;
            this.showMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.showMessage.Location = new System.Drawing.Point(242, 57);
            this.showMessage.Name = "showMessage";
            this.showMessage.Size = new System.Drawing.Size(341, 24);
            this.showMessage.TabIndex = 11;
            this.showMessage.TabStop = true;
            this.showMessage.Text = "Why do I need to enter this information?\r\n";
            this.showMessage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.showMessage_LinkClicked);
            // 
            // EmailCode
            // 
            this.EmailCode.Location = new System.Drawing.Point(658, 305);
            this.EmailCode.Name = "EmailCode";
            this.EmailCode.Size = new System.Drawing.Size(51, 20);
            this.EmailCode.TabIndex = 4;
            this.EmailCode.TextChanged += new System.EventHandler(this.InfoChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(415, 222);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(237, 20);
            this.label6.TabIndex = 13;
            this.label6.Text = "Text Message Verification Code:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(475, 305);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(177, 20);
            this.label7.TabIndex = 14;
            this.label7.Text = "Email Varification Code:";
            // 
            // TextCheck
            // 
            this.TextCheck.Image = global::ActiveAuthenticationDesktopClient.Properties.Resources.greenCheck1;
            this.TextCheck.InitialImage = null;
            this.TextCheck.Location = new System.Drawing.Point(725, 222);
            this.TextCheck.Name = "TextCheck";
            this.TextCheck.Size = new System.Drawing.Size(31, 31);
            this.TextCheck.TabIndex = 15;
            this.TextCheck.TabStop = false;
            this.TextCheck.Visible = false;
            // 
            // MailCheck
            // 
            this.MailCheck.Image = global::ActiveAuthenticationDesktopClient.Properties.Resources.greenCheck1;
            this.MailCheck.InitialImage = null;
            this.MailCheck.Location = new System.Drawing.Point(725, 305);
            this.MailCheck.Name = "MailCheck";
            this.MailCheck.Size = new System.Drawing.Size(31, 31);
            this.MailCheck.TabIndex = 16;
            this.MailCheck.TabStop = false;
            this.MailCheck.Visible = false;
            // 
            // Textx
            // 
            this.Textx.Image = global::ActiveAuthenticationDesktopClient.Properties.Resources.redX;
            this.Textx.InitialImage = null;
            this.Textx.Location = new System.Drawing.Point(725, 222);
            this.Textx.Name = "Textx";
            this.Textx.Size = new System.Drawing.Size(31, 31);
            this.Textx.TabIndex = 17;
            this.Textx.TabStop = false;
            this.Textx.Visible = false;
            // 
            // Mailx
            // 
            this.Mailx.Image = global::ActiveAuthenticationDesktopClient.Properties.Resources.redX;
            this.Mailx.InitialImage = null;
            this.Mailx.Location = new System.Drawing.Point(725, 305);
            this.Mailx.Name = "Mailx";
            this.Mailx.Size = new System.Drawing.Size(31, 31);
            this.Mailx.TabIndex = 18;
            this.Mailx.TabStop = false;
            this.Mailx.Visible = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(24, 81);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(748, 120);
            this.label5.TabIndex = 19;
            this.label5.Text = resources.GetString("label5.Text");
            // 
            // Enrollment
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 401);
            this.Controls.Add(this.TextCode);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.Mailx);
            this.Controls.Add(this.Textx);
            this.Controls.Add(this.MailCheck);
            this.Controls.Add(this.TextCheck);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.EmailCode);
            this.Controls.Add(this.showMessage);
            this.Controls.Add(this.ReSend);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Email);
            this.Controls.Add(this.Phone);
            this.Controls.Add(this.Carrier);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Enrollment";
            this.Text = "Enrollment";
            ((System.ComponentModel.ISupportInitialize)(this.TextCheck)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MailCheck)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Textx)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Mailx)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox Carrier;
        private System.Windows.Forms.TextBox Phone;
        private System.Windows.Forms.TextBox Email;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox TextCode;
        private System.Windows.Forms.Button ReSend;
        private System.Windows.Forms.LinkLabel showMessage;
        private System.Windows.Forms.TextBox EmailCode;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.PictureBox TextCheck;
        private System.Windows.Forms.PictureBox MailCheck;
        private System.Windows.Forms.PictureBox Textx;
        private System.Windows.Forms.PictureBox Mailx;
        private System.Windows.Forms.Label label5;
    }
}