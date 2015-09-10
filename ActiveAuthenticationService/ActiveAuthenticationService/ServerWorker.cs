using System;
using System.Security.Principal;
using System.IO.Pipes;
using System.IO;
using System.Security.AccessControl;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Management;
using System.Data;
using System.Diagnostics;
using Microsoft.Win32;
using System.DirectoryServices.AccountManagement;

namespace ActiveAuthenticationService
{
    class ServerWorker
    {
        public const int MAXCLIENTS = 64;
        public bool connected { get; private set; }
        public bool disconnected { get; private set; }
        public static ArrayList allFiles { get; private set; }
        private NamedPipeServerStream pipeServer;
        private PipeStreamReader ss;
        private Thread thread;
        private List<FileSystemWatcher> watchList;
        private FileSystemEventHandler changeHandler;


        public ServerWorker()
        {
            connected = false;
            disconnected = false;
            PipeSecurity ps = new PipeSecurity();
            ps.SetAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), PipeAccessRights.ReadWrite, AccessControlType.Allow));
            ps.AddAccessRule(new PipeAccessRule(WindowsIdentity.GetCurrent().User, PipeAccessRights.FullControl, AccessControlType.Allow));
            pipeServer = new NamedPipeServerStream("testpipe", PipeDirection.InOut, MAXCLIENTS, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 5000, 5000, ps);
            thread = new Thread(ThreadRun);
            thread.Start();
        }

        private void ThreadRun()
        {
            Process[] procs = Process.GetProcessesByName("ActiveAuthenticationDesktopClient");
            if(procs.Length == 0)
            {
                InteractiveProcessLauncher.LaunchProcessAsConsoleUser(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\Louisiana Tech University\Active Authentication\ActiveAuthenticationDesktopClient.exe");
            }
            pipeServer.WaitForConnection();
            ss = new PipeStreamReader(pipeServer);
            connected = true;
            while (pipeServer.IsConnected)
            {
                try
                {
                    string request = ss.ReadString();
                    string response = "Error";
                    if (request.Equals("TimeOut"))
                    {
                        timeOut();
                    }
                    else
                    {
                        if (request.Equals("LUB"))
                        {
                            response = "DUB";
                        }
                        else if (request.Equals("REG"))
                        {
                            lockFiles();
                            response = "CON";
                        }
                        else if (request.Equals("LockFiles"))
                        {
                            lockFiles();
                            response = "Success";
                        }
                        else if (request.Equals("UnlockFiles"))
                        {
                            unlockFiles();
                            response = "Success";
                        }
                        else if (request.Equals("DeleteUsers"))
                        {
                            deleteUsers();
                            response = "Success";
                        }
                        else if (request.Equals("ChangeOwner"))
                        {
                            newOwner();
                            response = "Success";
                        }
                        else if(request.Equals("DTM"))
                        {
                            changeTaskManager();
                            response = "Success";
                        }
                        if (pipeServer.IsConnected)
                        {
                            int ret = ss.WriteString(response);
                            if (ret < 0)
                                pipeServer.Disconnect();
                        }
                    }
                }
                catch (IOException e)
                {
                    ActiveAuthenticationService.LOG.WriteEntry("IOException Caught: "+ e.ToString());
                }
                catch (Exception e)
                {
                    ActiveAuthenticationService.LOG.WriteEntry("Exception Caught: " + e.ToString());
                }
            }
            if (pipeServer != null)
            {
                if (pipeServer.IsConnected)
                {
                    pipeServer.Disconnect();
                }
                pipeServer.Dispose();
            }
            connected = false;
            disconnected = true;
        }

        public void Cleanup()
        {
            if (pipeServer != null)
            {
                if (pipeServer.IsConnected)
                {
                    pipeServer.Disconnect();
                }
                pipeServer.Dispose();
            }
            if (thread.IsAlive)
                thread.Abort();
        }

        public void Stop()
        {
            if (pipeServer != null)
            {
                if (pipeServer.IsConnected)
                {
                    pipeServer.Disconnect();
                }
                pipeServer.Dispose();
            }
            if (thread.IsAlive)
                thread.Abort();
            connected = false;
            disconnected = true;
        }

        private void lockFiles()
        {

            SelectQuery query = new SelectQuery("Win32_UserAccount");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            allFiles = new ArrayList();
            string[] files = Directory.GetFiles(ActiveAuthenticationService.installDir);
            for (int i = 0; i < files.Length; i++)
            {
                if (!files[i].Equals(ActiveAuthenticationService.installDir + @"\CustomInstaller.InstallState"))
                    allFiles.Add(new FileStream(files[i], FileMode.Open, FileAccess.Read, FileShare.Read));
            }
            files = Directory.GetFiles(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Louisiana Tech University\Active Authentication");
            for (int i = 0; i < files.Length; i++)
            {
                allFiles.Add(new FileStream(files[i], FileMode.Open, FileAccess.Read, FileShare.Read));
            }
            string startupfile = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup) + @"\Shortcut to ActiveAuthenticationDesktopClient.exe.lnk"; 
            if(File.Exists(startupfile))
            {
                allFiles.Add(new FileStream(startupfile, FileMode.Open, FileAccess.Read, FileShare.Read));
            }
            else
            {
                File.Copy(System.Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\Louisiana Tech University\Active Authentication\Shortcut to ActiveAuthenticationDesktopClient.exe.lnk", startupfile);

            }
            watchList = new List<FileSystemWatcher>();
            foreach (ManagementObject envVar in searcher.Get())
            {
                try
                {

                    string[] filesPath = Directory.GetFiles(ActiveAuthenticationService.PATHBEGIN + envVar["Name"].ToString() + ActiveAuthenticationService.PATHEND);
                    FileSystemWatcher fWatch = new FileSystemWatcher(ActiveAuthenticationService.PATHBEGIN + envVar["Name"].ToString() + ActiveAuthenticationService.PATHEND);
                    watchList.Add(fWatch);
                    for (int i = 0; i < filesPath.Length; i++)
                    {
                        if(filesPath[i].Equals(ActiveAuthenticationService.PATHBEGIN + envVar["Name"].ToString() + ActiveAuthenticationService.PATHEND + @"\Users.xml"))
                            allFiles.Add(new FileStream(filesPath[i], FileMode.Open, FileAccess.Read, FileShare.Read));
                        else
                            allFiles.Add(new FileStream(filesPath[i], FileMode.Open, FileAccess.Read, FileShare.None));
                    }
                    
                }
                catch (DirectoryNotFoundException e)
                {

                }
            }
            foreach(FileSystemWatcher watch in watchList)
            {
                changeHandler = new FileSystemEventHandler(invalidFile);
                watch.Created += changeHandler;
                watch.EnableRaisingEvents = true;
            }

        }
        private void unlockFiles()
        {
            foreach (FileStream f in allFiles)
            {
                f.Close();
            }
            foreach(FileSystemWatcher watch in watchList)
            {
                watch.Created -= changeHandler;
            }
            watchList.Clear(); 
            allFiles.Clear();
        }
        private void newOwner()
        {
            ss.WriteString("Ready");
            string ownerPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Louisiana Tech University\Active Authentication\Owner.xml";
            DataSet dsUserInfo = new DataSet();
            DataTable dtUserInfo = new DataTable();
            dtUserInfo.Columns.Add(new DataColumn("Registered", typeof(bool)));
            dtUserInfo.Columns.Add(new DataColumn("MessagingAddress", typeof(string)));
            dtUserInfo.Columns.Add(new DataColumn("EmailAddress", typeof(string)));
            dtUserInfo.Columns.Add(new DataColumn("WindowTime", typeof(int)));
            dsUserInfo.Tables.Add(dtUserInfo);
            string newOwner = ss.ReadString();
            string userPath = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)) + @"Users\" + newOwner + @"\AppData\Roaming\Louisiana Tech University\Active Authentication\Profiles\Users.xml";
            unlockFiles();
            dsUserInfo.ReadXml(userPath, XmlReadMode.ReadSchema);
            dsUserInfo.WriteXml(ownerPath);
            lockFiles();
        }
        private void deleteUsers()
        {
            bool done = false;
            ss.WriteString("Ready");
            string invalidUser = ss.ReadString();
            unlockFiles();
            while(!done)
            {
                File.Delete(Path.GetPathRoot(Environment.SystemDirectory) + @"Users\" + invalidUser + @"\AppData\Roaming\Louisiana Tech University\Active Authentication\Profiles\Users.xml");
                ss.WriteString("Next");
                invalidUser = ss.ReadString();
                if (invalidUser.Equals("Done"))
                    done = true;
            }
            lockFiles();
        }
        private void timeOut()
        {
            try
            {
                if (pipeServer.IsConnected)
                {
                    pipeServer.Disconnect();
                    pipeServer.Close();
                }
                InteractiveProcessLauncher.LaunchProcessAsConsoleUser(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\Louisiana Tech University\Active Authentication\ActiveAuthenticationDesktopClient.exe");
            
            }
            catch(Exception e)
            {
                ActiveAuthenticationService.LOG.WriteEntry("Timed out: " + e.ToString());
            }
        }
        private void changeTaskManager()
        {
            ss.WriteString("Ready");
            bool enable = Convert.ToBoolean(ss.ReadString());
            ss.WriteString("SID");
            string SID = ss.ReadString();
            try
            {
                RegistryKey objRegistryKey = Registry.Users.CreateSubKey(
                   SID + @"\Software\Microsoft\Windows\CurrentVersion\Policies\System");
                if (enable && objRegistryKey.GetValue("DisableTaskMgr") != null)
                {
                    objRegistryKey.DeleteValue("DisableTaskMgr");
                }
                else
                {
                    objRegistryKey.SetValue("DisableTaskMgr", "1");
                }
                objRegistryKey.Close();
            }
            catch(Exception e)
            {
                ActiveAuthenticationService.LOG.WriteEntry("Key name: " + SID + @"\Software\Microsoft\Windows\CurrentVersion\Policies\System" + " | Exception: " + e.ToString());
            }
        }

        private void invalidFile(object sender, FileSystemEventArgs args)
        {
            File.Delete(args.FullPath);
        }
    }
}
