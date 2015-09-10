using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Data;

namespace ActiveAuthenticationService
{
    static class InteractiveProcessLauncher
    {
        #region structs
        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO { 
            public int cb; 
            public String lpReserved;
            public String lpDesktop; 
            public String lpTitle; 
            public uint dwX; 
            public uint dwY; 
            public uint dwXSize; 
            public uint dwYSize; 
            public uint dwXCountChars; 
            public uint dwYCountChars; 
            public uint dwFillAttribute; 
            public uint dwFlags; 
            public short wShowWindow;
            public short cbReserved2; 
            public IntPtr lpReserved2; 
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError; }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION { 
            public IntPtr hProcess; 
            public IntPtr hThread; 
            public uint dwProcessId; 
            public uint dwThreadId; }

        public enum TOKEN_TYPE 
        { TokenPrimary = 1, TokenImpersonation }

        public enum SECURITY_IMPERSONATION_LEVEL 
        { SecurityAnonymous, SecurityIdentification, SecurityImpersonation, SecurityDelegation }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES { 
            public int nLength; 
            public IntPtr lpSecurityDescriptor; 
            public int bInheritHandle; }

        public struct WTS_SESSION_INFO
        {
            public int SessionID;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string WinStationName;
            public ConnectionState State;
        }
        #endregion
        #region dllimport
        [DllImport("wtsapi32.dll", EntryPoint = "WTSEnumerateSessions", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Int32 WTSEnumerateSessions(IntPtr hServer, int reserved, int version, ref IntPtr sessionInfo, ref int count);

        [DllImport("kernel32.dll", EntryPoint = "WTSGetActiveConsoleSessionId")]
        public static extern uint WTSGetActiveConsoleSessionId();

        [DllImport("wtsapi32.dll", EntryPoint = "WTSQueryUserToken", SetLastError = true)]
        public static extern bool WTSQueryUserToken(UInt32 sessionId, out IntPtr Token);

        [DllImport("advapi32.dll", EntryPoint = "CreateProcessAsUser", SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public extern static bool CreateProcessAsUser(IntPtr hToken, String lpApplicationName, String lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandle, int dwCreationFlags, IntPtr lpEnvironment,
            String lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);


        [DllImport("advapi32.dll", EntryPoint = "DuplicateTokenEx", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool DuplicateTokenEx(
            IntPtr hExistingToken,
            uint dwDesiredAccess,
            ref SECURITY_ATTRIBUTES lpTokenAttributes,
            SECURITY_IMPERSONATION_LEVEL ImpersonationLevel,
            TOKEN_TYPE TokenType,
            out IntPtr phNewToken);

        [DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public extern static bool CloseHandle(IntPtr handle);

        [DllImport("wtsapi32.dll", EntryPoint = "WTSFreeMemory")]
        public static extern void WTSFreeMemory(IntPtr memory);
        #endregion

        private static IntPtr htoken, htoken2;

        static InteractiveProcessLauncher()
        {
            htoken = IntPtr.Zero;
            htoken2 = IntPtr.Zero;
        }

        public static bool LaunchProcessAsConsoleUser(string path)
        {
            bool ret = false;     
            if (htoken != IntPtr.Zero)
            {
                CloseHandle(htoken);
                htoken = IntPtr.Zero;
            }
            if (htoken2 != IntPtr.Zero)
            {
                CloseHandle(htoken2);
                htoken = IntPtr.Zero;
            }
            List<WTS_SESSION_INFO> sessions = ListSessions();
            foreach (WTS_SESSION_INFO session in sessions)
            {
                UInt32 sessionId = (UInt32)session.SessionID;
                if (sessionId != 0 && sessionId != 0xFFFFFFFF)
                {
                    bool result = WTSQueryUserToken(sessionId, out htoken);
                    if (result)
                    {
                        const uint MAXIMUM_ALLOWED = 0x02000000;
                        SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
                        result = DuplicateTokenEx(htoken, MAXIMUM_ALLOWED, ref sa, SECURITY_IMPERSONATION_LEVEL.SecurityIdentification, TOKEN_TYPE.TokenPrimary, out htoken2);
                        if (result)
                        {
                            STARTUPINFO si = new STARTUPINFO();
                            si.cb = Marshal.SizeOf(si);
                            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
                            result = CreateProcessAsUser(htoken2, null, path, ref sa, ref sa, false, 0, (IntPtr)0, null, ref si, out pi);
                            if (result)
                            {
                                CloseHandle(pi.hProcess);
                                CloseHandle(pi.hThread);
                                ret = true;
                                ActiveAuthenticationService.LOG.WriteEntry("Application Started in session: " + session.SessionID.ToString());
                            }
                        }
                        if (htoken2 != IntPtr.Zero)
                        {
                            CloseHandle(htoken2);
                            htoken2 = IntPtr.Zero;
                        }
                    }
                    if (htoken != IntPtr.Zero)
                    {
                        CloseHandle(htoken);
                        htoken = IntPtr.Zero;
                    }
                }
            }
            return ret;
        }

        public static List<WTS_SESSION_INFO> ListSessions()
        {
            IntPtr server = IntPtr.Zero;
            List<WTS_SESSION_INFO> ret = new List<WTS_SESSION_INFO>();

            try
            {
                IntPtr ppSessionInfo = IntPtr.Zero;

                Int32 count = 0;
                Int32 retval = WTSEnumerateSessions(IntPtr.Zero, 0, 1, ref ppSessionInfo, ref count);
                Int32 dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));

                Int64 current = (int)ppSessionInfo;

                if (retval != 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        WTS_SESSION_INFO si = (WTS_SESSION_INFO)Marshal.PtrToStructure((System.IntPtr)current, typeof(WTS_SESSION_INFO));
                        current += dataSize;
                        ret.Add(si);
                    }

                    WTSFreeMemory(ppSessionInfo);
                }
            }
            catch (Exception exception)
            {
                ActiveAuthenticationService.LOG.WriteEntry(exception.ToString());
            }

            return ret;
        }
    }
}
