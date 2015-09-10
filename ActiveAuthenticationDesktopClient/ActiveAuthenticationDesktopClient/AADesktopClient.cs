using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Security.Principal;
using System.ServiceProcess;
using System.Web.Security;
using System.Timers;


namespace ActiveAuthenticationDesktopClient
{
    static class AADesktopClient
    {
        public static bool Pause;
        public static NotifyIcon ni;
        public static HiddenForm HF;
        public static StreamString ss { get; private set; }
        private static ServiceController service;
        private static IdentityVerifier IDverify;
        private static NamedPipeClientStream pipeClient;
        private static bool HeartBeatLock;
        private static bool alreadyLocked = false;
        private static int numberOfKeyboards = 1;
        private static int failCount;
        private static double failscore = 0;
        private static bool matchFound = false;
        private static bool activeSession = true;
        private static Thread _thread;
        private static FileStream[] files;
        private static bool running = true;
        public static bool internetAvailable = true;
        public static bool openTrust = false;
        public static Mutex Highlander;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (AlreadyRunning())
            {
                return;
            }
            Application.EnableVisualStyles();
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(Cleanup);
            ni = SetupNI();
            pipeClient = new NamedPipeClientStream(".", "testpipe", PipeDirection.InOut, PipeOptions.Asynchronous, TokenImpersonationLevel.Impersonation);
            HeartBeatLock = false;
            System.Timers.Timer HeartBeat = new System.Timers.Timer();
            HeartBeat.Interval = 1000;
            HeartBeat.Elapsed += beat;
            HeartBeat.Start();
            service = new ServiceController("ActiveAuthenticationService", Environment.MachineName);
            HF = new HiddenForm();
            if (!UserConfigurations.Registered) // User is new to active authentication, or has been invalidated by the owner.
            {
                while (!pipeClient.IsConnected)
                {
                    // Wait for connection.
                }
                Enrollment EnrollmentForm = new Enrollment("", "", "");
                while (!EnrollmentForm.Registered) // Keep popping up new enrollment forms until it is properly filled out. 
                {
                    EnrollmentForm.ShowDialog();
                    if (!EnrollmentForm.Registered)
                    {
                        // If the user tries to avoid giving contact information we explain why it is needed.
                        MessageBox.Show("This system requires a valid set of contact information to function properly and securly." +
                            " If you can not provide a valid set of contact information we appologize for the inconvienience, and this application can be uninstalled either with the control panel " +
                            "uninstall applications function or by running the Installer.msi executable that you used to install the program, and selecting the remove option.");
                    }
                }
                HF.openTrustWindow(); // Start training mode without the initial lock screen.
            }
            else if (!File.Exists(Configuration.ownerFile)) // If there is not an owner than this is the original installation of the program. 
            {                                               // In that case we set the user that installed the program (this user) as the owner.
                try
                {
                    ss.WriteString("ChangeOwner");
                    if (ss.ReadString().Equals("Ready"))
                    {
                        ss.WriteString(Environment.UserName);
                    }
                    if (!ss.ReadString().Equals("Success"))
                        MessageBox.Show("Error");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
           
            IDverify = new IdentityVerifier();
            failCount = 0;

            //Start the keyboard hook then bind the eventhandler(s).
            KeyboardHook.Start();
            Highlander = new Mutex();
            KeyboardHook.KeyboardAction += new EventHandler<KeyboardHookEventArgs>(KeyboardEventCollect);
#if DEBUG
            // KeyboardHook.KeyboardAction += new EventHandler<KeyboardHookEventArgs>(KeyboardEventBalloon);
#endif
#if Kahona
            DataSet TBC = new DataSet();
#if Nick1
            TBC.ReadXml(Path.GetPathRoot(Environment.SystemDirectory)+@"Workspace\ActiveAuthenticationDesktopClient\KeyStrokeData\TheBigKahona.xml");
#elif Nick2
            TBC.ReadXml(Path.GetPathRoot(Environment.SystemDirectory)+@"Workspace\ActiveAuthenticationDesktopClient\KeyStrokeData\KSData.xml");
#elif Nick3
            TBC.ReadXml(Path.GetPathRoot(Environment.SystemDirectory)+@"Workspace\ActiveAuthenticationDesktopClient\KeyStrokeData\TheBigKahona2.xml");
#elif AZ1
            TBC.ReadXml(Path.GetPathRoot(Environment.SystemDirectory)+@"Workspace\ActiveAuthenticationDesktopClient\KeyStrokeData\AZKahona.xml");
#endif
            DataTable BIG_ONE = TBC.Tables[0];
            foreach (DataRow row in BIG_ONE.Rows)
            {
                IDverify.FileEvent(row);
            }
#endif
            //Setup other hooks to catch events
          //  ContextHook.Start();
          //  ContextHook.ContextChange += new EventHandler<ContextHookEventArgs>(ContextEvent);   used for context specific version --N
            RawKeyboard.NewKeyboard += new EventHandler<RawKeyboardEventArgs>(New_Keyboard);

            NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(NetworkAvailabilityChange);
            Microsoft.Win32.SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(SessionSwitch);
            Application.Run();
        }
        /// <summary>
        /// An Event hook method to cleanup the program
        /// </summary>
        private static void Cleanup(object sender, System.EventArgs e)
        {
            KeyboardHook.Stop();
           // ContextHook.Stop();
            ni.Dispose();
        }
        /// <summary>
        /// An Event Hook method to close the application
        /// </summary>
        private static void Close(object sender, System.EventArgs e)
        {
            KeyboardHook.Stop();
           // ContextHook.Stop();
            ni.Dispose();
            running = false;
            Application.Exit();
        }
        /// <summary>
        /// Demands authentication and then allows the user to change his or her contact information
        /// </summary>
        private static void PhoneEmail(object sender, System.EventArgs e)
        {
#if !DEBUG
            bool isCanceled = Lock("ProtectedFeature", "Change Contact Information"); 
#endif
            if (!isCanceled)
            {
                string ma = UserConfigurations.MessagingAddress;
                int i = ma.IndexOf('@');
                string em = UserConfigurations.EmailAddress;
                Enrollment f = new Enrollment(ma.Remove(i), ma.Remove(0, i), em);
                f.ShowDialog();
                f.Dispose();
            }
        }
        /// <summary>
        /// Demands authentication and then clears the profiles associated with this user
        /// </summary>
        private static void ClearProfile(object sender, System.EventArgs e)
        {
#if !DEBUG
            bool isCanceled = Lock("ProtectedFeature", "Clear Profiles"); 
#endif
            if (!isCanceled)
            {
                //Display profile clearing dialog
                DialogResult result = MessageBox.Show("WARNING!  If you continue with this operation your typing profile will be deleted and Active "
                    + "Authentication will enter “training mode” to construct a new profile. This operation should only be performed if Active Authentication is frequently "
                    + "misidentifying you as an imposter.  Do you wish to Continue?", "Warning!", MessageBoxButtons.YesNo);
                if (result.Equals(DialogResult.Yes))
                {
                    unlockFiles();
                    string[] files = Directory.GetFiles(Configuration.profilePath);
                    UserConfigurations.TrustTime = 60;
                    foreach (string file in files)
                    {
                        if (!file.Equals(Configuration.profilePath + @"\Users.xml") && !file.Equals(Configuration.profilePath + @"\ValidKeyboards.xml") && !file.Equals(Configuration.profilePath + @"config.xml"))
                        {
                            File.Delete(file);
                        }
                    }
                    lockFiles();
                    HF.openTrustWindow();
                }
            }
        }
        /// <summary>
        /// Demands authentication before the application can be paused
        /// </summary>
        public static void PauseAuth(object sender, System.EventArgs e)
        {
#if !DEBUG
            bool isCanceled = Lock("ProtectedFeature", "Pause"); 
#endif
            if (!isCanceled)
            {
                PauseLength P = new PauseLength();
                P.ShowDialog();
                P.Dispose();
            }
        }
        /// <summary>
        /// Locks the screen with the authentication failed message
        /// </summary>
        private static void ReAuth(object sender, System.EventArgs e)
        {
            Lock("Failed", "");
        }
        /// <summary>
        /// Shows a balloon in the bottom right corner of the screen
        /// </summary>
        /// <param name="Title">The title displayed in the balloon</param>
        /// <param name="Body">The text displayed in the balloon</param>
        /// <param name="Seconds">The number of seconds to display the balloon</param>
        private static void ShowBalloon(String Title, String Body, float Seconds)
        {
            ni.BalloonTipTitle = Title;
            ni.BalloonTipText = Body;
            ni.ShowBalloonTip((int)(Seconds * 1000));
        }
#if DEBUG
        /// <summary>
        /// Used to show a balloon when typing. Hooked into KeyboardHook.
        /// </summary>
        private static void KeyboardEventBalloon(object sender, KeyboardHookEventArgs args)
        {
            if (!Pause)
            {
                char c = (char)args.AsciiCode;
                char d = args.FlagUp ? '↑' : '↓';
                ShowBalloon("You Typed", "" + c + d, 0.5f);
            }
        }
#endif
        /// <summary>
        /// Sends the keyboard events into the collector
        /// </summary>
        private static void KeyboardEventCollect(object sender, KeyboardHookEventArgs args)
        {
            if(Highlander.WaitOne())
            {
                
                if (!Pause && activeSession)
                {
                    double score = IDverify.KeyEvent(args);
                    if (!openTrust && Verifiers.trainingUser && !alreadyLocked)
                    {
                        alreadyLocked = true;
                        Lock("EndOpenTrust", "");
                        HF.openTrustWindow();
                        alreadyLocked = false;
                    }
                    if (score < 50)
                    {
                        if (!openTrust)
                        {
                            alreadyLocked = true;
                            Lock("Failed", "");
                            HF.openTrustWindow();
                            IdentityVerifier.TrustValue = 70;
                            failCount = 0;
                            alreadyLocked = false;
                        }
                        else
                        {
                            if (score != failscore || score == 0)
                            {
                                failCount++;
                                failscore = score;
                                if (failCount == 100)
                                {
                                    failCount = 0;
                                    if (File.Exists(Configuration.profilePath + @"\" + Verifiers.currentUser + Verifiers.keyboardNameCaptured /*+ Verifiers.contextChange*/ + @".xml"))
                                    {
                                        unlockFiles();
                                        File.Delete(Configuration.profilePath + @"\" + Verifiers.currentUser + Verifiers.keyboardNameCaptured /*+ Verifiers.contextChange*/ + @".xml");
                                        lockFiles();
                                        Verifiers.keyboardChange = true; // Note the keyboard has not actually changed we are just using this to force the system into trainin mode --N
                                    }
                                }
                            }
                        }
                        ni.Icon = ActiveAuthenticationDesktopClient.Properties.Resources.ARed;
                    }
                    else if (score < 70)
                    {
                        failCount = 0;
                        ni.Icon = ActiveAuthenticationDesktopClient.Properties.Resources.AYellow;
                    }
                    else
                    {
                        failCount = 0;
                        ni.Icon = ActiveAuthenticationDesktopClient.Properties.Resources.AGreen;
                    }
                    Highlander.ReleaseMutex();
                }
            }
            // Raises a flag if the keyboard in use changes --N
            if (Verifiers.keyboardNameCaptured != Verifiers.keyboardNameOnRecord)
            {
                Verifiers.keyboardNameOnRecord = Verifiers.keyboardNameCaptured;
                Verifiers.keyboardChange = true;
                IDverify.changeKeyboard(Verifiers.keyboardNameCaptured);
            }
            
        }
        /// <summary>
        /// Catches context changes and updates the context.
        /// </summary>
       /* private static void ContextEvent(object sender, ContextHookEventArgs args)
        {
            Console.WriteLine(args.Context);
            Verifiers.contextCaptured = args.Context.Replace(@"\", "");
            Verifiers.contextCaptured = Verifiers.contextCaptured.Replace(":", "");
            Verifiers.contextChange = true;
            IDverify.changeContext(Verifiers.contextCaptured);
        }*/                                                              // above method used to catch context changes --N
        /// <summary>
        /// Creates and sets up a notification icon
        /// </summary>
        /// <returns>The newly created notification icon</returns>
        private static NotifyIcon SetupNI()
        {
            NotifyIcon NI = new NotifyIcon();
            NI.Text = "Active Authentication";
            NI.ContextMenu = new ContextMenu();
            NI.ContextMenu.MenuItems.Add(new MenuItem("Pause", new EventHandler(PauseAuth)));
            NI.ContextMenu.MenuItems.Add(new MenuItem("Change Phone Number or Email Address", new EventHandler(PhoneEmail)));
            NI.ContextMenu.MenuItems.Add(new MenuItem("Clear Profiles", new EventHandler(ClearProfile)));
            if (UserConfigurations.OwnerEmail != null)
            {
                if (UserConfigurations.OwnerMessagingAddress == UserConfigurations.MessagingAddress && UserConfigurations.OwnerEmail == UserConfigurations.EmailAddress)
                {
                    // Make sure this user is the owner before giving them these options
                    NI.ContextMenu.MenuItems.Add(new MenuItem("Invalidate Users", new EventHandler(ManageAccounts)));
                    NI.ContextMenu.MenuItems.Add(new MenuItem("Change Machine Owner", new EventHandler(ChangeOwner)));
                }
            }         
#if DEBUG
            NI.ContextMenu.MenuItems.Add(new MenuItem("Close", new EventHandler(Close)));
            NI.ContextMenu.MenuItems.Add(new MenuItem("Re-Auth", new EventHandler(ReAuth)));
#endif
            NI.Icon = ActiveAuthenticationDesktopClient.Properties.Resources.AGreen;
            NI.Visible = true;
            return NI;
        }
        /// <summary>
        /// This method is run as a thread to lock down profiles.
        /// </summary>
        private static void FileThread()
        {
            while (running)
            {
                String[] filesPath = Directory.GetFiles(Configuration.profilePath);
                files = new FileStream[filesPath.Count()];
                for (int i = 0; i < filesPath.Count(); i++)
                {
                    files[i] = new FileStream(filesPath[i], FileMode.Open, FileAccess.Read, FileShare.Read);
                }
            }
        }
        /// <summary>
        /// Locks the screen and demands authentication when a new keyboard is introduced to the system.
        /// </summary>
        private static void New_Keyboard(object sender, RawKeyboardEventArgs e)
        {
            if (e.keyboardCount > numberOfKeyboards)
            {
                DataSet dsValidKeyboards = KeyboardList.readKeyboardList(Configuration.profilePath);
                for (int j = 0; j < dsValidKeyboards.Tables[0].Rows.Count; j++)
                {
                    if (dsValidKeyboards.Tables[0].Rows[j].ItemArray[0].Equals(e.deviceName.Substring(8, 17)))
                    {
                        matchFound = true;
                        break;
                    }
                }
                if (matchFound == true)
                {
                    if (!alreadyLocked)
                    {
                        alreadyLocked = true;
                        Lock("NewKeyboard", "");
                        alreadyLocked = false;
                    }
                }
                else
                {
                    if (!alreadyLocked)
                    {
                        alreadyLocked = true;
                        Lock("NewKeyboard", "");
                        alreadyLocked = false;
                    }
                    dsValidKeyboards.Tables[0].Rows.Add(e.deviceName.Substring(8, 17));
                    KeyboardList.SaveKeyboardList(dsValidKeyboards, Configuration.profilePath);
                }
            }
        }
        /// <summary>
        /// Pauses the program if the session is inactive
        /// </summary>
        private static void SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionLock)
            {
                activeSession = false;
            }
            else if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionUnlock)
            {
                activeSession = true;
            }
        }
        /// <summary>
        /// Checks if an internet connection is available every time the network connection changes
        /// </summary>
        private static void NetworkAvailabilityChange(object sender, NetworkAvailabilityEventArgs e)
        {
            string warning = "ATTENTION:  Active Authentication has detected the loss of your internet connection.\n\n";
            warning = warning + "If possible, we STRONGLY recommend that you IMMEDIATELY RECONNECT TO THE INTERNET and then PAUSE Active Authentication before continuing to work offline.\n\n";
            warning = warning + "Do NOT attempt to PAUSE Active Authentication without a working internet connection or you will be locked out of your machine until internet can be reestablished.\n\n";
            warning = warning + "While you may be able to continue to use your machine offline normally for the time being, without internet if you attempt to perform an action that requires secondary authentication (or if you are inadvertently marked as a potential imposter) Active Authentication will be unable to send you a verification code and you will be locked out of your machine until you can reestablish an internet connection and receive a verification code.";
            if (e.IsAvailable)
            {
                Ping p = new Ping();
                try
                {
                    PingReply reply = p.Send("www.google.com", 3000);
                    if (reply.Status == IPStatus.Success)
                    {
                        internetAvailable = true;
                    }
                    else
                    {
                        MessageBox.Show(warning, "Internet Lost:");
                        internetAvailable = false;
                    }
                }
                catch { }

            }
            else
            {
                MessageBox.Show(warning, "Internet Lost:");
                internetAvailable = false;
            }
        }
        /// <summary>
        /// Locks the screen and all user I/O
        /// </summary>
        /// <returns>whether the operation that caused the program to lock has been canceled<\returns>
        public static bool Lock(string reason, string Feature)
        {
            ReAuthForm f = new ReAuthForm(reason, Feature);
            while (!f.auth)
            {
                f.ShowDialog();
            }
            bool canceled = f.canceled;
            f.Dispose();
            if (IdentityVerifier.TrustValue < 70)
            {
                IdentityVerifier.TrustValue = 70;
            }
            return canceled;
        }
        /// <summary>
        /// Same as the lock, but sends the reauthentication information to the owner instead of the user
        /// </summary>
        /// <returns>whether the operation that caused the program to lock has been canceled
        public static bool OwnerLock(string reason, string Feature)
        {
            ReAuthForm f = new ReAuthForm(true, reason, Feature);
            while (!f.auth)
            {
                f.ShowDialog();
            }
            bool canceled = f.canceled;
            f.Dispose();
            IdentityVerifier.TrustValue = 100;
            return canceled;
        }
        /// <summary>
        /// Checks to see if the process is already running
        /// </summary>
        /// <returns>Whether the process is running</returns>
        public static bool AlreadyRunning()
        {
            Process curr = Process.GetCurrentProcess();
            var currSesID = Process.GetCurrentProcess().SessionId;
            Process[] procs = Process.GetProcessesByName(curr.ProcessName);
            try
            {
                foreach (Process p in procs)
                {
                    if (p.SessionId == currSesID)
                    {
                        try{
                            if ((p.Id != curr.Id) &&
                                (p.MainModule.FileName == curr.MainModule.FileName))
                                return true;
                        }
                        catch(Exception e)
                        {
                            MessageBox.Show("inside: " + e.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("outside: "+e.ToString());
            }
            return false;
        }
        /// <summary>
        /// Demands authentication from the owner and changes the owner of the machine<\returns>
        /// </summary>
        private static void ChangeOwner(object sender, System.EventArgs e)
        {
            bool isCanceled = OwnerLock("OwnerFeature", "Change Owner");
            if (!isCanceled)
            {
                UserSelection us = new UserSelection("Owner");
                us.ShowDialog();
                if (!us.canceled)
                {
                    try
                    {
                        ss.WriteString("ChangeOwner");
                        if (ss.ReadString().Equals("Ready"))
                        {
                            ss.WriteString(us.newOwner);
                        }
                        if (!ss.ReadString().Equals("Success"))
                            MessageBox.Show("Change Owner Error");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
                us.Dispose();
            }
        }
        /// <summary>
        /// Demands authentication from the owner and then allows the owner to select users the he or she would like to deny access to their computer
        /// </summary>
        private static void ManageAccounts(object sender, System.EventArgs e)
        {
            bool isCanceled = OwnerLock("OwnerFeature", "Invalidate Users");
            if (!isCanceled)
            {
                UserSelection us = new UserSelection("Delete");
                us.ShowDialog();
                if (!us.canceled)
                {
                    try
                    {
                        ss.WriteString("DeleteUsers");
                        if (ss.ReadString().Equals("Ready"))
                        {
                            foreach (string user in us.invalidUsers)
                            {
                                ss.WriteString(user);
                                if (!ss.ReadString().Equals("Next"))
                                {
                                    MessageBox.Show("Error");
                                    break;
                                }
                            }
                        }
                        ss.WriteString("Done");
                        if (!ss.ReadString().Equals("Success"))
                            MessageBox.Show("Manage Accounts Error");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
                us.Dispose();
            }
        }
        /// <summary>
        /// Asks the service to lock the critical files in the file system
        /// </summary>
        public static void lockFiles()
        {
            ss.WriteString("LockFiles");
            string response = ss.ReadString();
            //if (!response.Equals("Success"))
                //MessageBox.Show("Lock Files Error");
        }
        /// <summary>
        /// Asks the service to unlock the critical files in the file system
        /// </summary>
        public static void unlockFiles()
        {
            ss.WriteString("UnlockFiles");
            string response = ss.ReadString();
            //if (!response.Equals("Success"))
                //MessageBox.Show("Unlock Files Error");
        }
        /// <summary>
        /// Sends a heartbeat to the service and listens for a response if the serivce. The only reason why this should
        /// fail to send or recieve a heartbeat is if the application or service is closed. In that case the running process will
        /// restart the closed one. Meaning the application is watching the service and the service is watching the application.
        /// </summary>
        private static void beat(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (HeartBeatLock)
                return;
            else
                HeartBeatLock = true;
            if (pipeClient != null)
            {
                if (pipeClient.IsConnected)
                {
                    int ret = ss.WriteString("LUB");
                    if (ret < 0)
                    {
                        initializePipe();
                    }
                    else
                    {
                        string response = ss.ReadString();
                        if (response.Equals("TimeOut"))
                        {
                            MessageBox.Show("time out");
                        }
                        else if (!response.Equals("DUB"))
                        {
                            //something bad happened
                        }
                    }
                }
                else
                {
                    initializePipe();
                }
            }
            else
            {
                initializePipe();
            }
            HeartBeatLock = false;
        }
        /// <summary>
        /// Initializes and connects the named pipe for communication with the service
        /// </summary>
        private static void initializePipe()
        {
            //Connection Flag starts off false
            bool con = false;
            while (!con) //While not connected
            {
                if (pipeClient != null)
                {
                    pipeClient.Dispose();
                }
                //Create new pipeClient
                pipeClient = new NamedPipeClientStream(".", "testpipe", PipeDirection.InOut, PipeOptions.Asynchronous, TokenImpersonationLevel.Impersonation);
               
                try
                {
                    //Make sure the service is running
                    service.Refresh();
                    if (service.Status == ServiceControllerStatus.Stopped || service.Status == ServiceControllerStatus.Paused)
                    {
                        //Start the service if it is stopped
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running);
                    }
                    //Wait for the pipeclient to connect
                    pipeClient.Connect();
                }
                catch (Exception ex)
                {
                    // this only appears to happen during uninstallation --N
                }
                //Create the streamstring object from the pipe
                ss = new StreamString(pipeClient);
                //Verify the pipe is working
                ss.WriteString("REG");
                if (ss.ReadString().Equals("CON"))
                {
                    con = true;
                }
                Console.WriteLine("con = "+con.ToString());
            }
        }

    }
    // Defines the data protocol for reading and writing strings on our stream 
    public class StreamString
    {
        private NamedPipeClientStream ioStream;
        private UnicodeEncoding streamEncoding;
        System.Timers.Timer timeout;

        public StreamString(NamedPipeClientStream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
            timeout = new System.Timers.Timer();
            timeout.Interval = 1000;
            timeout.Elapsed += timeUp;
        }
        /// <summary>
        /// Reads the string from the named pipe
        /// </summary>
        /// <returns>Message from the service</returns>
        public string ReadString()
        {
            int len = 0;
            timeout.Start();
            len = ioStream.InBufferSize;
            byte[] inBuffer = new byte[len];
            int result = -1;
            try
            {
                result = ioStream.Read(inBuffer, 0, len);
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.ToString());
            }
            if (result == 0)
            {
                return "TimeOut";
            }
            else if (result == -1)
            {
                //MessageBox.Show("There was an error Reading");
                return "TimeOut";
            }
            else
            {
                timeout.Stop();
                string value = streamEncoding.GetString(inBuffer);
                StringBuilder shortString = new StringBuilder();
                foreach (char c in value.ToCharArray())
                {
                    byte b = (byte)c;
                    if (b >= 32)
                        shortString.Append(c);
                }
                return shortString.ToString();
            }
        }
        /// <summary>
        /// timer event that allows the readstring to time out
        /// </summary>
        public void timeUp(object sender, EventArgs e)
        {
            timeout.Stop();
            ioStream.Close();
        }
        /// <summary>
        /// Write a string to the named pipe
        /// </summary>
        /// <returns>Buffer length plus 2 or an error code of -1</returns>
        public int WriteString(string outString)
        {
            if (outString.Length > 4998)
            {
                outString.Remove(4998);
            }
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            try
            {
                ioStream.Write(outBuffer, 0, len);
                ioStream.Flush();
            }
            catch(Exception e)
            {
                return -1;
            }
            return len + 2;
        }
    }

}
