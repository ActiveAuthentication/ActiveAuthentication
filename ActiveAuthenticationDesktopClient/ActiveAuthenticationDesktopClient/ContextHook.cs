using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace ActiveAuthenticationDesktopClient
{
    static class ContextHook
    {
        // Create a null event templated by the ContextHookEventArgs class.  This points to the client's
        // custom event handler (i.e. that receives context event arguments and passes them through a 
        // messaging system.
        public static event EventHandler<ContextHookEventArgs> ContextChange = null;

        // Declare WinEventProcCallback to be used as a reference for the hook callback function.
        private delegate void WinEventProcCallback(IntPtr hWinEventHook, int dwEvent, IntPtr hwnd, long idObject, long idChild, int dwEventThread, int dwmsEventTime);

        // Create 'proc' the WinEventProcCallback function pointer to the hook call back function.
        private static WinEventProcCallback proc = CallbackMethod;

        //Import a method to get the processID from the window's handle
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr HWND, out IntPtr processID);

        //Import a method to create windows event hooks
        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventProcCallback proc, int idProcess, int idThread, uint dwflags);

        [DllImport("user32.dll")]
        static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        private static IntPtr eventHook = IntPtr.Zero;

        public static void Start()
        {
            if (eventHook!= IntPtr.Zero)
            {
                Stop();
            }
            //Create an event hook to detect window focus changes
            try
            {
                eventHook = SetWinEventHook((uint)3, (uint)3, IntPtr.Zero, proc, 0, 0, (uint)(0 | 2));
            }catch (Exception e){}
        }


        public static void Stop()
        {
            try
            {
                UnhookWinEvent(eventHook);
                eventHook = IntPtr.Zero;
            }catch (Exception e){}
        }

        /// <summary>
        /// This is the callback method for detecting a focus change event
        /// </summary>
        /// <param name="hWinEventHook"></param>
        /// <param name="dwEvent"></param>
        /// <param name="hwnd">Contains a handle to the window that became focused</param>
        /// <param name="idObject"></param>
        /// <param name="idChild"></param>
        /// <param name="dwEventThread"></param>
        /// <param name="dwmsEventTime"></param>
        private static void CallbackMethod(IntPtr hWinEventHook, int dwEvent, IntPtr hwnd, long idObject, long idChild, int dwEventThread, int dwmsEventTime)
        {
            long time = DateTime.Now.Ticks;
            string str;
            try
            {
                IntPtr procId;
                GetWindowThreadProcessId(hwnd, out procId);
                str = Process.GetProcessById((int)procId).MainModule.FileName;
            }
            catch (Exception exception)
            {
                str = "ERROR!!!";
                Console.WriteLine(exception);
            }
            ContextHookEventArgs args = new ContextHookEventArgs();
            args.Context = str;
            args.time = time;
            if (ContextChange != null)
                ContextChange(null, args);
        }
    }
    // Custom event argument class for data transport.
    public class ContextHookEventArgs : EventArgs
    {
        public string Context;
        public long time;
    }
}
