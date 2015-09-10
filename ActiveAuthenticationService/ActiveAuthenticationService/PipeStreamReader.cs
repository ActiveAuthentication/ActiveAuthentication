using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ActiveAuthenticationService
{
    // Defines the data protocol for reading and writing strings on our stream 
    public class PipeStreamReader
    {
        public const int Timeout = 10000;
        private NamedPipeServerStream streamserver;
        private UnicodeEncoding streamEncoding;
        private Timer timeoutTimer;

        public PipeStreamReader(NamedPipeServerStream StreamServer)
        {
            streamserver = StreamServer;
            streamEncoding = new UnicodeEncoding();
            this.timeoutTimer = new Timer();
            timeoutTimer.Elapsed += Timeup;
            timeoutTimer.Interval = Timeout;
        }

        public string ReadString()
        {
            int len = 0;
            timeoutTimer.Start();
            len = streamserver.InBufferSize;
            byte[] inBuffer = new byte[len];
            int result = -1;
            try
            {
                result = streamserver.Read(inBuffer, 0, len);
            }
            catch (Exception e)
            {
                ActiveAuthenticationService.LOG.WriteEntry( e.ToString());
            }
            if (result == 0)
            {
                return "TimeOut";
            }
            else if (result == -1)
            {
                ActiveAuthenticationService.LOG.WriteEntry("There was an error");
                return "TimeOut";
            }
            else
            {
                timeoutTimer.Stop();
                StringBuilder shortString = new StringBuilder();
                foreach (byte b in inBuffer)
                {
                    if (b >= 32)
                        shortString.Append((char)b);
                }
                return shortString.ToString();
            }
        }


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
                streamserver.Write(outBuffer, 0, len);
                streamserver.Flush();
            }
            catch (Exception e)
            {
                return -1;
            }
            return len + 2;
        }

        public void Timeup(object sender, EventArgs e)
        {
            timeoutTimer.Stop();
            streamserver.Close();
        }
    }
}
