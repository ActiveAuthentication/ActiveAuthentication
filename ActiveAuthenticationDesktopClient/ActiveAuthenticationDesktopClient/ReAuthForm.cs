using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.ServiceProcess;
using System.Security.Principal;
using System.Net.NetworkInformation;

namespace ActiveAuthenticationDesktopClient
{
    public enum ServiceCommands : int
    {
        Beat = 129,
        DisableTaskMgr = 140,
        EnableTaskMgr = 141,
        LockFiles = 142,
        UnlockFiles = 143
    }
    public partial class  ReAuthForm : Form
    {
        const int WM_COMMAND = 0x111;
        const int MIN_ALL = 419;
        const int MIN_ALL_UNDO = 416;
        const int SW_HIDE = 0;
        const int SW_SHOW = 1;
        const int CP_NOCLOSE_BUTTON = 0x200;
        public const int WH_KEYBOARD_LL = 13;

        public bool auth { get; private set; }
        public bool canceled { get; private set; }
        private oneTimePassword otp;
        private static List<IntPtr> windowHandles = new List<IntPtr>();
        private static IntPtr hwnd;
        private IntPtr hEvent;
        private static IntPtr hookptr = IntPtr.Zero;
        private static Thread _thread, _thread2;
        private static bool SendToOwner = false;
        private static bool running = true;
        private static bool Uninstalling = false;
        private static string Reason;
        private static String s;
        private static string Feature;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true)]
        static extern IntPtr SendMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("User32", EntryPoint = "ShowWindow", SetLastError = true)]
        private static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32", EntryPoint = "UnhookWindowsHookEx", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int UnhookWindowsHookEx(IntPtr hHook);
        public delegate int LowLevelKeyboardProc(int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam);
        [DllImport("user32", EntryPoint = "CallNextHookEx", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int CallNextHookEx(int hHook, int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        public static extern short GetKeyState(int keyCode);

        public struct KBDLLHOOKSTRUCT
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }
        /// <summary>
        /// Lock the screen and request authentication from the user
        /// </summary>
        /// <param name="reason"></param>
        /// <param name="feature"></param>
        public ReAuthForm(string reason, string feature)
        {
            Reason = reason;
            Feature = feature;
            canceled = false;
            InitializeComponent();
            auth = false;
            SendToOwner = false;
            hwnd = this.Handle;
            _thread2 = new Thread(threadFocus);
            _thread2.IsBackground = true;
            _thread2.Start();
            //_thread = new Thread(hookthread);
            //_thread.IsBackground = true;
            //_thread.Start();
            SetTaskManager(false);
            DisableKeyCombos();
            this.Bounds = Screen.PrimaryScreen.Bounds;
            WindowState = FormWindowState.Maximized;
            panel1.Location = new Point(this.Width / 2 - panel1.Width / 2, this.Height / 2 - panel1.Height / 2);
            SetForegroundWindow(hwnd);
            s = UserConfigurations.MessagingAddress;
            PhoneBox.Text = "A verification code has been sent to your mobile phone and registered email account.";
            otp = new oneTimePassword();
            if(reason.Equals("EndOpenTrust"))
            {
                CancelButton.Enabled = false;
                CancelButton.Visible = false;
                submitButton.Size = new System.Drawing.Size(252, 35);
                label1.Text = "To continue constructing your profile\nActive Authentication requires secondary authentication.";
                Messager.SendEmail(UserConfigurations.EmailAddress, Messager.emailSubjectLine, Messager.endOpenTrustEmail + otp.otp);
                Messager.SendTextMessageTo(s, Messager.endOpenTrustText + otp.otp);
            }
            else if(reason.Equals("Failed"))
            {
                CancelButton.Enabled = false;
                CancelButton.Visible = false;
                submitButton.Size = new System.Drawing.Size(252, 35);
                Messager.SendTextMessageTo(s, Messager.standardTextMessage + otp.otp);
                Messager.SendEmail(UserConfigurations.EmailAddress, Messager.emailSubjectLine, Messager.standardEmailMessage + otp.otp);
            }
            else if(reason.Equals("ProtectedFeature"))
            {
                label1.Text = "This feature requires secondary authentication.";        
                Messager.SendTextMessageTo(s, Messager.protectedFeatureTextpt1 + feature + Messager.protectedFeatureTextpt2 + otp.otp);
                Messager.SendEmail(UserConfigurations.EmailAddress, Messager.emailSubjectLine, Messager.protectedFeatureEmailpt1 + feature + Messager.protectedFeatureEmailpt2 + otp.otp);
            }
            else if(reason.Equals("NewKeyboard"))
            {
                label1.Text = "New Keyboard Detected.  As a security precaution secondary authentication is required to continue using this machine.";
                CancelButton.Enabled = false;
                CancelButton.Visible = false;
                submitButton.Size = new System.Drawing.Size(252, 35);
                Messager.SendTextMessageTo(s, Messager.newKeyboardText + otp.otp);
                Messager.SendEmail(UserConfigurations.EmailAddress, Messager.emailSubjectLine, Messager.newKeyboardEmail + otp.otp);
            }
            else // Should never get here
            {
                Messager.SendTextMessageTo(s, "" + otp.otp);
                Messager.SendEmail(UserConfigurations.EmailAddress, Messager.emailSubjectLine, "" + otp.otp); 
            }
            #if !DEBUG
            HideAllOthers();
            #endif
            if(AADesktopClient.internetAvailable == false)
            {
                internetNotification.Visible = true;
            }
            NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(NetworkAvailabilityChange);
        }
        /// <summary>
        /// Lock the screen and request authentication from the owner
        /// </summary>
        /// <param name="ownerAuth"></param>
        /// <param name="reason"></param>
        /// <param name="feature"></param>
        public ReAuthForm(bool ownerAuth, string reason, string feature)
        {
            Reason = reason;
            Feature = feature;
            canceled = false;
            InitializeComponent();
            auth = false;
            hwnd = this.Handle;
            _thread2 = new Thread(threadFocus);
            _thread2.IsBackground = true;
            _thread2.Start();
            DisableKeyCombos();
            this.Bounds = Screen.PrimaryScreen.Bounds;
            WindowState = FormWindowState.Maximized;
            panel1.Location = new Point(this.Width / 2 - panel1.Width / 2, this.Height / 2 - panel1.Height / 2);
            SetForegroundWindow(hwnd);
            String s = UserConfigurations.OwnerMessagingAddress;
            PhoneBox.Text = "A verification code has been sent to your mobile phone and registered email account.";
            otp = new oneTimePassword();
            if(reason.Equals("NewUser"))
            {
                SetTaskManager(false);
                PhoneBox.Visible = false;
                label1.Size = new System.Drawing.Size(280, 125);
                label1.Font = new Font(label1.Font.Name, 9);
                label1.Text = "To continue using this account on this machine you must register with Active Authentication. "
                    + "To procede with registration please contact the owner of this machine and request the verification code. "
                    + "Once you have the code please enter it below and press submit.";
                CancelButton.Enabled = false;
                CancelButton.Visible = false;
                submitButton.Size = new System.Drawing.Size(252, 35);
                Messager.SendEmail(UserConfigurations.OwnerEmail, Messager.emailSubjectLine, Messager.newUserEmail + otp.otp);
                Messager.SendTextMessageTo(s, Messager.newUserText + otp.otp);
            }
            else if (reason.Equals("OwnerFeature"))
            {
                SetTaskManager(false);
                label1.Text = "This feature requires secondary authentication.";
                Messager.SendEmail(UserConfigurations.OwnerEmail, Messager.emailSubjectLine, Messager.ownerFeatureEmailpt1 + feature + Messager.ownerFeatureEmailpt2 + otp.otp);
                Messager.SendTextMessageTo(s, Messager.ownerFeatureTextpt1 + feature + Messager.ownerFeatureTextpt2 + otp.otp);
            }
            else if(reason.Equals("Uninstall"))
            {
                running = false;
                label1.Text = "This feature requires secondary authentication.";
                Uninstalling = true;
                Messager.SendEmail(UserConfigurations.OwnerEmail, Messager.emailSubjectLine, Messager.uninstallEmail + otp.otp);
                Messager.SendTextMessageTo(s, Messager.uninstallText + otp.otp);
            }
            else // Should never get here
            {
                SetTaskManager(false);
                Messager.SendTextMessageTo(s, otp.otp);
                Messager.SendEmail(UserConfigurations.OwnerEmail, "Active Authentication Code", "Your new code is: " + otp.otp);
            }
            SendToOwner = true;
        }
        /// <summary>
        /// Send the one time password
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void reSendButton_Click(object sender, EventArgs e)
        {
            if (otp.IsExpired())
                otp = new oneTimePassword();
            if (SendToOwner)
            {
                if (Reason.Equals("NewUser"))
                {
                    Messager.SendEmail(UserConfigurations.OwnerEmail, Messager.emailSubjectLine, Messager.newUserEmail + otp.otp);
                    Messager.SendTextMessageTo(s, Messager.newUserText + otp.otp);
                }
                else if (Reason.Equals("OwnerFeature"))
                {
                    Messager.SendEmail(UserConfigurations.OwnerEmail, Messager.emailSubjectLine, Messager.ownerFeatureEmailpt1 + Feature + Messager.ownerFeatureEmailpt2 + otp.otp);
                    Messager.SendTextMessageTo(s, Messager.ownerFeatureTextpt1 + Feature + Messager.ownerFeatureTextpt2 + otp.otp);
                }
                else if (Reason.Equals("Uninstall"))
                {
                    Messager.SendEmail(UserConfigurations.OwnerEmail, Messager.emailSubjectLine, Messager.uninstallEmail + otp.otp);
                    Messager.SendTextMessageTo(s, Messager.uninstallText + otp.otp);
                }
                else
                {
                    Messager.SendTextMessageTo(s, otp.otp);
                    Messager.SendEmail(UserConfigurations.OwnerEmail, "Active Authentication Code", "Your new code is: " + otp.otp);
                }
            }
            else
            {
                if (Reason.Equals("EndOpenTrust"))
                {
                    Messager.SendEmail(UserConfigurations.EmailAddress, Messager.emailSubjectLine, Messager.endOpenTrustEmail + otp.otp);
                    Messager.SendTextMessageTo(s, Messager.endOpenTrustText + otp.otp);
                }
                else if (Reason.Equals("Failed"))
                {
                    Messager.SendTextMessageTo(s, Messager.standardTextMessage + otp.otp);
                    Messager.SendEmail(UserConfigurations.EmailAddress, Messager.emailSubjectLine, Messager.standardEmailMessage + otp.otp);
                }
                else if (Reason.Equals("ProtectedFeature"))
                {
                    Messager.SendTextMessageTo(s, Messager.protectedFeatureTextpt1 + Feature + Messager.protectedFeatureTextpt2 + otp.otp);
                    Messager.SendEmail(UserConfigurations.EmailAddress, Messager.emailSubjectLine, Messager.protectedFeatureEmailpt1 + Feature + Messager.protectedFeatureEmailpt2 + otp.otp);
                }
                else if (Reason.Equals("NewKeyboard"))
                {
                    Messager.SendTextMessageTo(s, Messager.newKeyboardText + otp.otp);
                    Messager.SendEmail(UserConfigurations.EmailAddress, Messager.emailSubjectLine, Messager.newKeyboardEmail + otp.otp);
                }
                else
                {
                    Messager.SendTextMessageTo(s, "" + otp.otp);
                    Messager.SendEmail(UserConfigurations.EmailAddress, Messager.emailSubjectLine, "" + otp.otp);
                }
            }
            
        }
        /// <summary>
        /// Check if the password recieved matches the one that was sent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void submitButton_Click(object sender, EventArgs e)
        {
            submitButton.Enabled = false;
            progressBar1.Value = 0;
            #if DEBUG
            if (codeBox.Text == "")
            {
                auth = true;
                backgroundWorkerFocus.Dispose();
                #if !DEBUG
                ShowAllOthers();
                #endif
                EnableKeyCombos();
                this.Close();
            }
            #endif
            if (!otp.IsExpired())
            {
                for (int i = 1; i < 100; i++)
                {
                    progressBar1.Value = i;
                    progressBar1.Value = i-1;
                    Thread.Sleep(15);
                }
                if (otp.PasswordMatch(codeBox.Text))
                {

                    auth = true;
                    running = false;
                    _thread2.Join();
                    #if !DEBUG
                    ShowAllOthers();
                    #endif
                    if (!Uninstalling)
                        SetTaskManager(true);
                    EnableKeyCombos();
                    this.Close();


                }
            }
            else
            {
                otp = new oneTimePassword();
                if (SendToOwner)
                {
                    Messager.SendTextMessageTo(UserConfigurations.OwnerMessagingAddress, otp.otp);
                    Messager.SendEmail(UserConfigurations.OwnerEmail, "Your computer has been locked", "If this was you, please use this code to authenticate yourself with the code below \n" + otp.otp);
                }
                else
                {
                    Messager.SendTextMessageTo(UserConfigurations.MessagingAddress, otp.otp);
                    Messager.SendEmail(UserConfigurations.EmailAddress, "Your account has been locked", "If this was you, please use this code to authenticate yourself with the code below \n" + otp.otp);
                }
                MessageBox.Show("Code Expired, Re-sending...");
            }
            progressBar1.Value = 0;
            submitButton.Enabled = true;
        }
        /// <summary>
        /// Keep the focus locked so that the user can not leave the lock screen
        /// </summary>
        private void threadFocus()
        {
            int i = 0;
            while (running)
            {
                HideAllOthers();
                Thread.Sleep(200);
                i++;
                if(i == 5)
                {
                    i = 0;
                    SetForegroundWindow(hwnd);
                }
            }
            running = true;
        }
        /// <summary>
        /// Hide all other windows from the user so that they must authenticate to regain access to the machine
        /// </summary>
        private static void HideAllOthers()
        {
            IntPtr hWnds = FindWindow("Shell_TrayWnd", "");
            ShowWindow(hWnds, SW_HIDE);
            windowHandles.Add(hWnds);
            Process[] processRunning = Process.GetProcesses();
            foreach (Process pr in processRunning)
            {
                hWnds = pr.MainWindowHandle;
                if (!windowHandles.Contains(hWnds))
                    windowHandles.Add(hWnds);
                if (hWnds != hwnd && pr.MainWindowTitle != "Please Re-Authenticate")
                {
                    ShowWindow(hWnds, SW_HIDE);
                }
            }
        }
        /// <summary>
        /// Unhide all the windows once the user has authenticated
        /// </summary>
        private static void ShowAllOthers()
        {
            foreach (IntPtr hWnds in windowHandles)
            {
                ShowWindow(hWnds, SW_SHOW);
            }
        }
        /// <summary>
        /// Do not close if the user has not authenticated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReAuthForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!auth)
                e.Cancel = true;
            base.OnClosing(e);
        }
        /// <summary>
        /// Do not allow the user to use key combos to get out of the lock screen
        /// </summary>
        public void DisableKeyCombos()
        {

            if (hookptr != IntPtr.Zero)
            {
                UnhookWindowsHookEx(hookptr);
                hookptr = IntPtr.Zero;
            }
            using (Process curProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    // Alternatively, SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle("user32"), 0); since user32.dll WILL be loaded, 
                    // its implied the current process is too.  One forum stated curModule.ModuleName will not work in .NET 4.0+ and pre Windows 8
                    // environments.  This bug has never been identified when using this method for collection.
                    hookptr = SetWindowsHookEx(WH_KEYBOARD_LL, hookCallback, GetModuleHandle(curModule.ModuleName), 0);

                    // Error checking, if hookId wasn't set by the user32.dll p/Invoke call, throw a Win32Exception error.
                    if (hookptr == IntPtr.Zero)
                        throw new System.ComponentModel.Win32Exception();
                }
            }
        }
        /// <summary>
        /// Stop blocking key combos after the user has authenticated
        /// </summary>
        public static void EnableKeyCombos()
        {
            if (hookptr != IntPtr.Zero)
            {
                UnhookWindowsHookEx(hookptr);
                hookptr = IntPtr.Zero;
            }
        }
        /// <summary>
        /// Callback method used by DisableKeyCombos
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        public int hookCallback(int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam)
        {
            bool blnEat = false;
            blnEat = (lParam.vkCode == 9) || (lParam.vkCode == 18) || (lParam.vkCode == 27) || (lParam.vkCode == 91) || (lParam.vkCode == 92) || (lParam.vkCode == 35) || (lParam.vkCode == 46) || (lParam.vkCode == 17) || (lParam.vkCode == 115) || (GetKeyState(17) & 0x8000) != 0 || (GetKeyState(91) & 0x8000) != 0 || (GetKeyState(92) & 0x8000) != 0;
            Console.WriteLine(blnEat.ToString());
            if (blnEat == true)
            {
                return 1;
            }
            else
            {
                return CallNextHookEx(0, nCode, wParam, ref lParam);
            }
        }
        /// <summary>
        /// Thread used to disable key combos without interfearing with other operations
        /// </summary>
        private void hookthread()
        {
            DisableKeyCombos();
            Application.Run();
        }
        /// <summary>
        /// Uses the service to disable and enable the task manager
        /// </summary>
        /// <param name="enable"></param>
        private void SetTaskManager(bool enable)
        {
            try
            {
                AADesktopClient.ss.WriteString("DTM");
                if (AADesktopClient.ss.ReadString().Equals("Ready"))
                {
                    AADesktopClient.ss.WriteString(enable.ToString());
                    if (AADesktopClient.ss.ReadString().Equals("SID"))
                    {
                        AADesktopClient.ss.WriteString(Verifiers.currentUser);
                        if (!AADesktopClient.ss.ReadString().Equals("Success"))
                        {
                            MessageBox.Show("error");
                        }
                    }
                    else
                    {
                        MessageBox.Show("error");
                    }
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        /// <summary>
        /// Warns the user if internet connection can not be established
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NetworkAvailabilityChange(object sender, NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable)
            {
                Ping p = new Ping();
                try
                {
                    PingReply reply = p.Send("www.google.com", 3000);
                    if (reply.Status == IPStatus.Success)
                    {
                        internetNotification.Visible = false;
                    }
                    else
                    {
                        internetNotification.Visible = true;
                        this.Refresh();
                    }
                }
                catch { }

            }
            else
            {
                internetNotification.Visible = true;
                this.Refresh();
            }
        }
        /// <summary>
        /// Cancels a protected operation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            label1.Text = "Operation canceled, but authentication is still required.";
            canceled = true;
        }

    }
    
}
