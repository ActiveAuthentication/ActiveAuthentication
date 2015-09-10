using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ActiveAuthenticationDesktopClient;
using System.Security.Principal;
using System.IO;


namespace ActiveAuthenticationDesktopClient
{
    public partial class Enrollment : Form
    {
        public bool Registered = false;
        private DataSet dsMessagingAddresses;
        private DataTable dtMessagingAddresses;
        private oneTimePassword textOtp;
        private oneTimePassword emailOtp;
        private bool submitReady = false;
        /// <summary>
        /// Form to create or modify contact information for a single user
        /// </summary>
        /// <param name="inputPhoneNumber">Phone number on file</param>
        /// <param name="inputCarrier">Service provider on file</param>
        /// <param name="inputEmail">Email address on file</param>
        public Enrollment(string inputPhoneNumber, string inputCarrier, string inputEmail) //Constructor is full of empty strings if the user does not have anything on file yet
        {
            InitializeComponent();
            textOtp = new oneTimePassword();
            dsMessagingAddresses = new DataSet();
            dsMessagingAddresses.ReadXml(Configuration.messagingAddressFile); // Grabs the file with the list of carriers to populate the drop down menu
            dtMessagingAddresses = dsMessagingAddresses.Tables[0];
            Carrier.DataSource = dsMessagingAddresses.Tables[0];
            Carrier.DisplayMember = "Carrier";
            Carrier.ValueMember = "GatewayAddress";
            try
            {
                if(!inputCarrier.Equals(""))
                    Carrier.SelectedValue = inputCarrier;
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
            }
            Email.Text = inputEmail;
            Phone.Text = inputPhoneNumber;
            emailOtp = new oneTimePassword();
            // Grey out and disable the code entry portion of the form until the contact information is provided
            ReSend.Enabled = false;
            EmailCode.Enabled = false;
            TextCode.Enabled = false;
            label6.ForeColor = Color.Gray;
            label7.ForeColor = Color.Gray;

            if(File.Exists(Configuration.ownerFile) && !UserConfigurations.Registered) //If this user is not registered have them get permission to use the computer from the owner
            {
                UserConfigurations.OwnerLoad();
                AADesktopClient.OwnerLock("NewUser", "");
            }
        }
        /// <summary>
        /// Send, ReSend, or Continue has been pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReSend_Click(object sender, EventArgs e)
        {
            if (submitReady == false)
            {
                // send a code to the contact information provided
                if (textOtp.IsExpired() || emailOtp.IsExpired())
                {
                    textOtp = new oneTimePassword();
                    ReSend.Text = "Re-Send Code";
                    submitReady = false;
                    Messager.SendTextMessageTo(Phone.Text + Carrier.SelectedValue, Messager.EnrollmentTextMessage + textOtp.otp);
                    emailOtp = new oneTimePassword();
                    Messager.SendEmail(Email.Text, Messager.EnrollmentEmailSubjectLine, Messager.EnrollmentEmail + emailOtp.otp);
                    MessageBox.Show("Code Expired, Resending...");
                }
                if (Phone.Text != "" && Carrier.SelectedIndex != -1 && Email.Text != "" && Email.Text.Contains('@') && Phone.Text.Length == 10)
                {
                    Messager.SendTextMessageTo(Phone.Text + Carrier.SelectedValue, Messager.EnrollmentTextMessage + textOtp.otp);
                    Messager.SendEmail(Email.Text, Messager.emailSubjectLine, Messager.EnrollmentEmail + emailOtp.otp);
                    ReSend.Text = "Re-Send Code";
                    
                }
                else
                {
                    MessageBox.Show("Invalid email or phone information. Please try again.");
                }
            }
            else if(submitReady == true)
            {  
                // code confirmed and ready to complete registration
                Registered = true;
                UserConfigurations.SetUser(Phone.Text + Carrier.SelectedValue, Email.Text);
                UserConfigurations.RegisterUser();
                UserConfigurations.Save();
                if (!File.Exists(Configuration.ownerFile))
                {
                    try
                    {
                        AADesktopClient.ss.WriteString("ChangeOwner");
                        if (AADesktopClient.ss.ReadString().Equals("Ready"))
                        {
                            AADesktopClient.ss.WriteString(Environment.UserName);
                        }
                        if (!AADesktopClient.ss.ReadString().Equals("Success"))
                        { 
                            // Error
                        }
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.ToString());
                    }
                }
                this.Close();      
            }
        }
        /// <summary>
        /// Check the text boxes everytime they are modified
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InfoChanged(object sender, EventArgs e)
        {
           
            if (Phone.Text != "" && Carrier.SelectedIndex != -1 && Email.Text != "" && Email.Text.Contains('@') && Phone.Text.Length == 10)
            {
                EmailCode.Enabled = true;
                TextCode.Enabled = true;
                ReSend.Enabled = true;
                label7.ForeColor = Color.Black;
                label6.ForeColor = Color.Black;
            }
            else
            {
                EmailCode.Enabled = false;
                TextCode.Enabled = false;
                label7.ForeColor = Color.Gray;
                label6.ForeColor = Color.Gray;
            }
            if(textOtp.PasswordMatch(TextCode.Text) && emailOtp.PasswordMatch(EmailCode.Text))
            {
                ReSend.Text = "Continue";
                submitReady = true;
                Textx.Visible = false;
                Mailx.Visible = false;
                TextCheck.Visible = true;
                MailCheck.Visible = true;
            }
            else if (textOtp.PasswordMatch(TextCode.Text) && !emailOtp.PasswordMatch(EmailCode.Text))
            {
                submitReady = false;
                Textx.Visible = false;
                TextCheck.Visible = true;
                if (!EmailCode.Text.Equals(""))
                {
                    MailCheck.Visible = false;
                    Mailx.Visible = true;
                }
                else
                {
                    MailCheck.Visible = false;
                    Mailx.Visible = false;
                }
                    
            }
            else if (!textOtp.PasswordMatch(TextCode.Text) && emailOtp.PasswordMatch(EmailCode.Text))
            {
                submitReady = false;
                Mailx.Visible = false;
                MailCheck.Visible = true;
                if(!TextCode.Text.Equals(""))
                {
                    TextCheck.Visible = false;
                    Textx.Visible = true;
                }
                else
                {
                    Textx.Visible = false;
                    TextCheck.Visible = false;
                }
                
            }
            else
            {
                submitReady = false;
                if (!TextCode.Text.Equals(""))
                {
                    TextCheck.Visible = false;
                    Textx.Visible = true;
                }
                else
                {
                    Textx.Visible = false;
                    TextCheck.Visible = false;
                }
                if (!EmailCode.Text.Equals(""))
                {
                    MailCheck.Visible = false;
                    Mailx.Visible = true;
                }
                else
                {
                    MailCheck.Visible = false;
                    Mailx.Visible = false;
                }
            }
            
        }
        /// <summary>
        /// Hyperlink to explain why contact information is required
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showMessage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Build string in parts to make modification of specific parts easier.
            string s = "The Active Authentication program uses text messages to your phone and email to an account that you can access remotely for secondary authentication – as a way for you to let Active Authentication know that you really are the authorized user of your computer. ";
            s = s + "Being able to prove your identity through a secondary means of authentication is important in two situations: ";
            s = s + "(1) when you want to modify the behavior of the Active Authentication program (e.g., to add a new authorized user or to pause or uninstall the program), ";
            s = s + "and (2) in those rare cases where inconsistencies in your typing behavior lead Active Authentication to suspect you may be an imposter.";

            s = s + "\n \n"; // new paragraph

            s = s + "Please note that your contact information is stored on your local computer and is not uploaded to the cloud. ";
            s = s + "No one, including the authors of the Active Authentication program, nor any other third party, will be given your contact information. ";
            s = s + "Your contact information will in no way be used to send advertisements or marketing materials of any nature. ";
            s = s + "Your contact information will be used by Active Authentication solely to send you verification codes by text message and email.";
            MessageBox.Show(s);
        }
    }
}
