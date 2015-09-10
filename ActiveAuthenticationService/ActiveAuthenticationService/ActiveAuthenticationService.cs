using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.IO;

namespace ActiveAuthenticationService
{
    public partial class ActiveAuthenticationService : ServiceBase
    {
#if Install
        public static string PATHBEGIN = Path.GetPathRoot(Environment.SystemDirectory) + @"Users\";
        public const string PATHEND = @"\AppData\Roaming\Louisiana Tech University\Active Authentication\Profiles";
        public static string installDir = System.Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\Louisiana Tech University\Active Authentication";
        public static string PATHAPP = installDir + @"\ActiveAuthenticationDesktopClient.exe";
        public static int numThreads = 4;
#else
        private const string PATH = Path.GetPathRoot(Environment.SystemDirectory) + @"Workspace\ActiveAuthenticationDesktopClient\Templates";
		private const string PATHAPP = Path.GetPathRoot(Environment.SystemDirectory) + @"Workspace\ActiveAuthenticationDesktopClient\ActiveAuthenticationDesktopClient\bin\Debug\ActiveAuthenticationDesktopClient.exe";
#endif
        private bool running = true;
        private Thread _thread;
        private DateTime lastBeat;
        private DataSet dsOwnerDetails;
        private static IntPtr hookptr = IntPtr.Zero;
        private static List<ServerWorker> servers;
        public static EventLog LOG;

        private enum commands : int
        {
            Beat = 129,
            DisableTaskMgr = 140,
            EnableTaskMgr = 141,
            LockFiles = 142,
            UnlockFiles = 143
        }

        public ActiveAuthenticationService()
        {
            InitializeComponent();
            LOG = this.EventLog;
            servers = new List<ServerWorker>();
        }

        protected override void OnStart(string[] args)
        {
            ACLDestroyer.KILL();
            _thread = new Thread(ThreadRun);
            _thread.Name = "Thread";
            _thread.Start();
        }

        protected override void OnStop()
        {
            running = false;
            try
            {
                foreach (FileStream f in ServerWorker.allFiles)
                {
                    f.Close();
                }
                ServerWorker.allFiles.Clear();
            }
            catch (Exception e){LOG.WriteEntry(e.ToString());}
            try
            {
                foreach (ServerWorker s in servers)
                {
                    s.Stop();
                }
            }
            catch (Exception e) { LOG.WriteEntry(e.ToString()); }
        }

        protected override void OnCustomCommand(int command)
        {
            switch ((commands)command)
            {
                case commands.Beat:
                    break;
                case commands.DisableTaskMgr:
                    break;
                case commands.EnableTaskMgr:
                    break;
                case commands.LockFiles:
                    break;
                case commands.UnlockFiles:
                    break;
            }
        }

        private void ThreadRun()
        {
            servers.Add(new ServerWorker());
            while (running)
            {
                try
                {
                    if (servers.Count < 1)
                        servers.Add(new ServerWorker());
                    else if (servers.Last<ServerWorker>().connected && servers.Count < ServerWorker.MAXCLIENTS || servers.Last<ServerWorker>().disconnected && servers.Count < ServerWorker.MAXCLIENTS)
                        servers.Add(new ServerWorker());
                    ServerWorker todelete = null;
                    foreach (ServerWorker server in servers)
                    {
                        if (server.disconnected) 
                        {
                            todelete = server;
                        }
                    }
                    if (todelete != null)
                    {
                        servers.Remove(todelete);
                        todelete.Cleanup();

                    }
                }
                catch (Exception e)
                {
                    LOG.WriteEntry(e.ToString());
                }
                Thread.Sleep(1000);
            }
        }
    }
}
