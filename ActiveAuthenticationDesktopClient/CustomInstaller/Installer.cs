using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;
using ActiveAuthenticationDesktopClient;
using System.ServiceProcess;
using System.Management;
using System.Net.NetworkInformation;

namespace CustomInstaller
{
    [RunInstaller(true)]
    public partial class Installer : System.Configuration.Install.Installer
    {
        public Installer()
        {
            ServiceController controller;
            ServiceController[] services = ServiceController.GetServices();
            var service = services.FirstOrDefault(s => s.ServiceName == "ActiveAuthenticationService");
            if (service != null)
            {
                controller = new ServiceController("ActiveAuthenticationService");
                if (controller.Status == ServiceControllerStatus.Stopped || controller.Status == ServiceControllerStatus.Paused)
                    controller.Start();
            }
            InitializeComponent();
        }
        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Install(IDictionary stateSaver)
        {

                Ping p = new Ping();
                try
                {
                    PingReply reply = p.Send("www.google.com", 3000);
                    if (reply.Status != IPStatus.Success)
                    {
                        throw new InstallException("An internet connection is required to install Active Authentication.  Please ensure your computer is connected to the internet and try installing Active Authentication again.");
                    }
                }
                catch 
                {
                     throw new InstallException("An internet connection is required to install Active Authentication.  Please ensure your computer is connected to the internet and try installing Active Authentication again.");
                }

            base.Install(stateSaver);
            ServiceController controller;
            ServiceController[] services = ServiceController.GetServices();
            var service = services.FirstOrDefault(s => s.ServiceName == "ActiveAuthenticationService");
            if(service != null)
            {
                controller = new ServiceController("ActiveAuthenticationService");
                if (controller.Status == ServiceControllerStatus.Running)
                    controller.ExecuteCommand(143);
            }
            else
            {
                string path = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)) + @"Windows\Microsoft.NET\Framework\v4.0.30319\" + "installUtil.exe";
                string arg = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)) + @"Program Files (x86)\Louisiana Tech University\Active Authentication\ActiveAuthenticationService.exe";
                Process srvinst = Process.Start(path, "\"" + arg + "\"");
                srvinst.WaitForExit();
                Process srvsc = Process.Start("cmd", @"/c sc sdset ActiveAuthenticationService D:(A;;LCRPDTLO;;;WD)(A;;CCLCSWRPWPDTLOCRRC;;;SY)(A;;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;BA)(A;;CCLCSWLOCRRC;;;IU)(A;;CCLCSWLOCRRC;;;SU)S:(AU;FA;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;WD)");
                srvsc.WaitForExit();
                controller = new ServiceController("ActiveAuthenticationService");
                if (controller.Status == ServiceControllerStatus.Stopped || controller.Status == ServiceControllerStatus.Paused)
                    controller.Start();
            }
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);
            ServiceController controller = new ServiceController("ActiveAuthenticationService");
            if (controller.Status == ServiceControllerStatus.Stopped || controller.Status == ServiceControllerStatus.Paused)
                controller.Start();
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        protected override void OnBeforeUninstall(IDictionary savedState)
        {
            
            if (File.Exists(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Louisiana Tech University\Active Authentication\Owner.xml"))
            {
                try
                {
                    ReAuthForm f = new ReAuthForm(true, "Uninstall", "");
                    f.ShowDialog();
                    if (!f.auth)
                    {
                        throw new InstallException("Only an authenticated user can uninstall this application.");
                    }
                    if(f.canceled)
                    {
                        notsure();
                    }
                    else
                        base.OnBeforeInstall(savedState);   
                    f.Dispose();
                }
                catch(Exception e)
                {
                    throw new InstallException(e.Message);
                }
            }
            else
            {
                base.OnBeforeUninstall(savedState);
            }
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
            DialogResult r = MessageBox.Show("Are you sure you want to uninstall Active Authentication?", "Uninstall", MessageBoxButtons.YesNo);
            if (r != DialogResult.Yes)
            {
                notsure();
            }      
            string path = Path.GetPathRoot(Environment.SystemDirectory) + @"Windows\Microsoft.NET\Framework\v4.0.30319\" + "installutil.exe";
            string argument = "\"" + Path.GetPathRoot(Environment.SystemDirectory) + @"Program Files (x86)\Louisiana Tech University\Active Authentication\ActiveAuthenticationService.exe""";
            Process servDist = Process.Start(path, @"/u " + argument);
            servDist.WaitForExit();
            Process[] procs = Process.GetProcessesByName("ActiveAuthenticationDesktopClient");
            foreach (Process p in procs)
                p.Kill();
            procs = Process.GetProcessesByName("ActiveAuthenticationDesktopClient.exe");
            foreach (Process p in procs)
                p.Kill();
            procs = Process.GetProcessesByName("ActiveAuthenticationDesktopClient.vshost.exe");
            foreach (Process p in procs)
                p.Kill();
            string[] users = Directory.GetDirectories(Path.GetPathRoot(Environment.SystemDirectory) + "Users");
            foreach(string user in users)
            {
                if(Directory.Exists(user + @"\AppData\Roaming\Louisiana Tech University"))
                {
                    Directory.Delete(user + @"\AppData\Roaming\Louisiana Tech University", true);
                }
            }
            if(Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Louisiana Tech University"))
            {
                Directory.Delete(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Louisiana Tech University", true);
            }
        }

        private void notsure()
        {
           MessageBox.Show("You have chosen to cancel deletion of the Active Authentication application.  Active Authentication will continue to run normally. Please ignore the following error message, no files will be deleted and nothing will be uninstalled.");
           throw new InstallException("Ignore this message. Uninstallation was aborted and no files have been deleted. No error has occured.");       
        }
    }
}
