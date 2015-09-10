using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Principal;


namespace ActiveAuthenticationDesktopClient
{
    public partial class HiddenForm : Form
    {
        private static Timer openTrust = new Timer();
        public static HiddenForm HF;
        private static bool VirtualMachine = false;
        /// <summary>
        /// Create the hidden form to handle the open trust window, and RawInput
        /// </summary>
        public HiddenForm()
        {
            RawInput rawinput = new RawInput(Handle, false);
            rawinput.KeyPressed += OnKeyPressed;
            openTrust.Tick += timeUp;
            HF = this;
            Verifiers.currentUser = WindowsIdentity.GetCurrent().User.ToString();
           
        }
        /// <summary>
        /// Save name of keyboard currently in use
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnKeyPressed(object sender, RawInputEventArg e)
        {
            if (!VirtualMachine)
            {
                try
                {
                    Verifiers.keyboardNameCaptured = e.KeyPressEvent.DeviceName.Substring(8, 17);
                }
                catch (ArgumentOutOfRangeException)
                {
                    VirtualMachine = true;
                    Verifiers.keyboardNameCaptured = "Virtual";
                }
            }
        }
        /// <summary>
        /// Open trust window is over
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timeUp(object sender, EventArgs e)
        {
            openTrust.Stop();
            AADesktopClient.openTrust = false;
        }
        /// <summary>
        /// Start the open trust window and decrease the length of the next open trust window
        /// until the windows are only 15 minutes in duration
        /// </summary>
        public void openTrustWindow()
        {
            AADesktopClient.openTrust = true;
            int trustTimer = UserConfigurations.TrustTime;
            openTrust.Interval = trustTimer * 60000;
            if (trustTimer > 15)
            {
                AADesktopClient.unlockFiles();
                UserConfigurations.TrustTime = trustTimer - 15;
            }
            openTrust.Start();
        }
    }
}
