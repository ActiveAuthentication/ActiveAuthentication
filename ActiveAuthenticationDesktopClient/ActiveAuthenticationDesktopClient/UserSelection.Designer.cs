namespace ActiveAuthenticationDesktopClient
{
    partial class UserSelection
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserSelection));
            this.UsersCheckList = new System.Windows.Forms.CheckedListBox();
            this.Label = new System.Windows.Forms.Label();
            this.SubmitButton = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.WarningMessage = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // UsersCheckList
            // 
            this.UsersCheckList.CheckOnClick = true;
            this.UsersCheckList.FormattingEnabled = true;
            this.UsersCheckList.Location = new System.Drawing.Point(16, 187);
            this.UsersCheckList.Name = "UsersCheckList";
            this.UsersCheckList.Size = new System.Drawing.Size(297, 64);
            this.UsersCheckList.TabIndex = 0;
            // 
            // Label
            // 
            this.Label.AutoSize = true;
            this.Label.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label.Location = new System.Drawing.Point(13, 168);
            this.Label.Name = "Label";
            this.Label.Size = new System.Drawing.Size(35, 13);
            this.Label.TabIndex = 1;
            this.Label.Text = "label1";
            // 
            // SubmitButton
            // 
            this.SubmitButton.Location = new System.Drawing.Point(198, 257);
            this.SubmitButton.Name = "SubmitButton";
            this.SubmitButton.Size = new System.Drawing.Size(75, 23);
            this.SubmitButton.TabIndex = 2;
            this.SubmitButton.Text = "Submit";
            this.SubmitButton.UseVisualStyleBackColor = true;
            this.SubmitButton.Click += new System.EventHandler(this.SubmitButton_Click);
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(76, 257);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 3;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // WarningMessage
            // 
            this.WarningMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WarningMessage.Location = new System.Drawing.Point(13, 18);
            this.WarningMessage.Name = "WarningMessage";
            this.WarningMessage.Size = new System.Drawing.Size(300, 142);
            this.WarningMessage.TabIndex = 4;
            this.WarningMessage.Text = "label1";
            // 
            // UserSelection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(341, 302);
            this.Controls.Add(this.WarningMessage);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.SubmitButton);
            this.Controls.Add(this.Label);
            this.Controls.Add(this.UsersCheckList);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "UserSelection";
            this.Text = "UserSelection";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox UsersCheckList;
        private System.Windows.Forms.Label Label;
        private System.Windows.Forms.Button SubmitButton;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Label WarningMessage;
    }
}