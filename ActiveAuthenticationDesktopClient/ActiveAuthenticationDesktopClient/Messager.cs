using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Mail;

namespace ActiveAuthenticationDesktopClient
{
    public static class Messager
    {
        // Text messages and emails sent by Active Authentication
        public const string emailSubjectLine = "Active Authentication Verification Code";
        public const string standardTextMessage = "The typing pattern of the individual currently using your computer does not appear to match that user’s profile.  As a precautionary measure Active Authentication has locked your computer.  If this is a false alarm we apologize for the interruption.  Enter the Verification Code below to unlock your machine. \n";
        public const string standardEmailMessage = "The typing pattern of the individual currently using your computer does not appear to match that user’s profile.  As a precautionary measure Active Authentication has locked your computer.  If this is a false alarm we apologize for the interruption.  Enter the Verification Code below to unlock your machine. \n";
        public static string newUserText = "An individual has logged on to your computer using the account  " + Environment.UserName + ".  If you recognize this account and would like to provide the owner of that account with access to your computer please contact that individual and provide him or her with the Verification Code below. \n";
        public static string newUserEmail = "An individual has logged on to your computer using the account " + Environment.UserName + ".  If you recognize this account and would like to provide the owner of that account with access to your computer please contact that individual and provide him or her with the Verification Code below. \n";

        public const string protectedFeatureTextpt1 = "Active Authentication has been asked to perform the protected Administrative Action: ";
        public const string protectedFeatureTextpt2 = ". To proceed with this action please enter the Verification Code provided below. To abort this action simply press the \"Cancel\" button.  You will then be asked to re-authenticate to regain access to your machine. \n";
        public const string protectedFeatureEmailpt1 = "Active Authentication has been asked to perform the protected Administrative Action: ";
        public const string protectedFeatureEmailpt2 = ". To proceed with this action please enter the Verification Code provided below.  To abort this action simply press the \"submit\" button.  You will then be asked to re-authenticate to regain access to your machine. \n";

        public const string ownerFeatureTextpt1 = "Active Authentication has been asked to perform the following restricted Administrative Action that is limited to the Owner of the machine: ";
        public const string ownerFeatureTextpt2 = ". To proceed with this action please enter the Verification Code provided below. To abort the action simply press the \"Cancel\" button. You will then be asked to re-authenticate to regain access to your machine.\n";
        public const string ownerFeatureEmailpt1 = "Active Authentication has been asked to perform the following restricted Administrative Action that is limited to the Owner of the machine: ";
        public const string ownerFeatureEmailpt2 = ". To proceed with this action please enter the Verification Code provided below. To abort the action simply press the \"Cancel\" button.  You will then be asked to re-authenticate to regain access to your machine.\n";

        public const string endOpenTrustText = "Active Authentication is in the process of constructing your typing profile.  In order to validate your identity please enter the code below and then continue using your machine normally.  We apologize for the interruption. \n";
        public const string endOpenTrustEmail = "Active Authentication is in the process of constructing your typing profile.  In order to validate your identity please enter the code below and then continue using your machine normally.  We apologize for the interruption. \n";
        public const string newKeyboardEmail = "Active Authentication detected a new keyboard being connected to your computer.  Please verify you are aware of and approve of this system change by entering the Verification Code below. \n";
        public const string newKeyboardText = "Active Authentication detected a new keyboard being connected to your computer.  Please verify you are aware of and approve of this system change by entering the Verification Code below. \n";
        public static string uninstallEmail = "The Active Authentication application has been asked to uninstall itself.  If you wish to delete Active Authentication from your computer please enter the Verification Code provided below. To abort this action simply press the \"Cancel\" button.  You will then be asked to re-authenticate to regain access to your machine. \n";
        public static string uninstallText = "The Active Authentication application has been asked to uninstall itself.  If you wish to delete Active Authentication from your computer please enter the Verification Code provided below. To abort this action simply press the \"Cancel\" button.  You will then be asked to re-authenticate to regain access to your machine. \n";
        public static string EnrollmentTextMessage = "This is your Active Authentication confirmation text message. Please enter the Verification Code provided below to verify you received this text message. \n";
        public static string EnrollmentEmailSubjectLine = "Active Authentication Confirmation Message";
        public static string EnrollmentEmail = "This is your Active Authentication confirmation email message. Please enter the Verification Code provided below to verify you received this email message. \n";
        /// <summary>
        /// Looks up the messaging address for the phone number
        /// </summary>
        /// <param name="cc">Country Code</param>
        /// <param name="num">Phone Number</param>
        /// <returns>The messaging address to that phone number</returns>
        public static string GetAddressForNumber(string cc, string num)
        {
            byte[] b = System.Text.Encoding.ASCII.GetBytes("cc=" + cc + "&phonenum=" + num);
            WebRequest r = WebRequest.Create("http://www.freecarrierlookup.com/getcarrier.php");
            r.ContentType = "application/x-www-form-urlencoded";
            r.ContentLength = b.Length;
            r.Method = "POST";
            Stream requestStream = r.GetRequestStream();
            requestStream.Write(b, 0, b.Length);
            requestStream.Close();
            HttpWebResponse response;
            response = (HttpWebResponse)r.GetResponse();
            string responseStr;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream responseStream = response.GetResponseStream();
                responseStr = new StreamReader(responseStream).ReadToEnd();
                string[] respTok = responseStr.Split(new string[] { "<", ">" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string token in respTok)
                {
                    if (token.Contains("@"))
                    {
                        return token;
                    }
                }
                return null;
            }
            else
            {
                Console.WriteLine("RETRY.");
                return null;
            }
        }
        /// <summary>
        /// Dynamically sends a text message to a phone number
        /// </summary>
        /// <param name="cc">Country Code</param>
        /// <param name="num">Phone number with area code</param>
        /// <param name="text">The message to send</param>
        public static void SendTextMessageDynamic (string cc, string num, string text)
        {
            string address = GetAddressForNumber(cc, num);
            SendEmail(address, "", text);
        }
        /// <summary>
        /// Dynamically sends a text message to a messaging address
        /// </summary>
        /// <param name="address">The messaging address</param>
        /// <param name="text">The text message</param>
        public static void SendTextMessageTo(string address, string text)
        {
            SendEmail(address, "", text);
        }
        /// <summary>
        /// Sends an email to the address given. Uses a gmail address registered for this project.
        /// </summary>
        /// <param name="address">The address to send to</param>
        /// <param name="subject">The subject of the message</param>
        /// <param name="body">The body of the message</param>
        public static void SendEmail(string address, string subject, string body)
        {
            MailMessage mail = new MailMessage("activeauth1@gmail.com", address);
            SmtpClient client = new SmtpClient();
            NetworkCredential cred = new NetworkCredential();
            cred.UserName = "activeauth1";
            cred.Password = "ILikeUnicorns!";
            client.Port = 587;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.EnableSsl = true;
            client.Credentials = cred;
            client.Host = "smtp.gmail.com";
            mail.Subject = subject;
            mail.Body = body;
            try
            {
                client.Send(mail);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
