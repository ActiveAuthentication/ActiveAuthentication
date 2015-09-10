using System;
using System.Data;
using System.Collections.Generic;
using System.Text;

namespace ActiveAuthenticationDesktopClient.FeatureExtractors
{
    public class FeatureExtractorArguments
    {
        const int VK_LBUTTON = 0x01;
        const int VK_RBUTTON = 0x02;
        const int VK_CANCEL = 0x03;
        const int VK_MBUTTON = 0x04;
        const int VK_XBUTTON1 = 0x05;
        const int VK_XBUTTON2 = 0x06;
        const int VK_BACK = 0x08;
        const int VK_TAB = 0x09;
        const int VK_CLEAR = 0x0C;
        const int VK_RETURN = 0x0D;
        const int VK_SHIFT = 0x10;
        const int VK_CONTROL = 0x11;
        const int VK_MENU = 0x12;
        const int VK_PAUSE = 0x13;
        const int VK_CAPITAL = 0x14;
        const int VK_KANA = 0x15;
        const int VK_HANGUEL = 0x15;
        const int VK_HANGUL = 0x15;
        const int VK_JUNJA = 0x17;
        const int VK_FINAL = 0x18;
        const int VK_KANJI = 0x19;
        const int VK_ESCAPE = 0x1B;
        const int VK_CONVERT = 0x1C;
        const int VK_NONCONVERT = 0x1D;
        const int VK_ACCEPT = 0x1E;
        const int VK_MODECHANGE = 0x1F;
        const int VK_SPACE = 0x20;
        const int VK_PRIOR = 0x21;
        const int VK_NEXT = 0x22;
        const int VK_END = 0x23;
        const int VK_HOME = 0x24;
        const int VK_LEFT = 0x25;
        const int VK_UP = 0x26;
        const int VK_RIGHT = 0x27;
        const int VK_DOWN = 0x28;
        const int VK_SELECT = 0x29;
        const int VK_PRINT = 0x2A;
        const int VK_EXECUTE = 0x2B;
        const int VK_SNAPSHOT = 0x2C;
        const int VK_INSERT = 0x2D;
        const int VK_DELETE = 0x2E;
        const int VK_HELP = 0x2F;
        const int VK_0 = 0x30;
        const int VK_1 = 0x31;
        const int VK_2 = 0x32;
        const int VK_3 = 0x33;
        const int VK_4 = 0x34;
        const int VK_5 = 0x35;
        const int VK_6 = 0x36;
        const int VK_7 = 0x37;
        const int VK_8 = 0x38;
        const int VK_9 = 0x39;
        const int VK_LWIN = 0x5B;
        const int VK_RWIN = 0x5C;
        const int VK_APPS = 0x5D;
        const int VK_SLEEP = 0x5F;
        const int VK_NUMPAD0 = 0x60;
        const int VK_NUMPAD1 = 0x61;
        const int VK_NUMPAD2 = 0x62;
        const int VK_NUMPAD3 = 0x63;
        const int VK_NUMPAD4 = 0x64;
        const int VK_NUMPAD5 = 0x65;
        const int VK_NUMPAD6 = 0x66;
        const int VK_NUMPAD7 = 0x67;
        const int VK_NUMPAD8 = 0x68;
        const int VK_NUMPAD9 = 0x69;
        const int VK_MULTIPLY = 0x6A;
        const int VK_ADD = 0x6B;
        const int VK_SEPARATOR = 0x6C;
        const int VK_SUBTRACT = 0x6D;
        const int VK_DECIMAL = 0x6E;
        const int VK_DIVIDE = 0x6F;
        const int VK_NUMLOCK = 0x90;
        const int VK_SCROLL = 0x91;
        const int VK_LSHIFT = 0xA0;
        const int VK_RSHIFT = 0xA1;
        const int VK_LCONTROL = 0xA2;
        const int VK_RCONTROL = 0xA3;
        const int VK_LMENU = 0xA4;
        const int VK_RMENU = 0xA5;
        const int VK_OEM_1 = 0xBA;
        const int VK_OEM_PLUS = 0xBB;
        const int VK_OEM_COMMA = 0xBC;
        const int VK_OEM_MINUS = 0xBD;
        const int VK_OEM_PERIOD = 0xBE;
        const int VK_OEM_2 = 0xBF;
        const int VK_OEM_3 = 0xC0;
        const int VK_OEM_4 = 0xDB;
        const int VK_OEM_5 = 0xDC;
        const int VK_OEM_6 = 0xDD;
        const int VK_OEM_7 = 0xDE;
        const int VK_OEM_8 = 0xDF;
        const int VK_OEM_102 = 0xE2;
        const int VK_PROCESSKEY = 0xE5;
        const int VK_PACKET = 0xE7;
        const int VK_ATTN = 0xF6;
        const int VK_CRSEL = 0xF7;
        const int VK_EXSEL = 0xF8;
        const int VK_EREOF = 0xF9;
        const int VK_PLAY = 0xFA;
        const int VK_ZOOM = 0xFB;
        const int VK_PA1 = 0xFD;
        const int VK_OEM_CLEAR = 0xFE;



        private static long pauseListThreshold = 250;

        struct KeyStroke
        {
            public bool isKeyPress;
            public int cursorPosition;
            public char keyChar;
            public int keyCode;
            public long timestamp;
        }

        public string FinalText
        {
            get;
            set;
        }

        public string CharStream
        {
            get;
            set;
        }

        public string[] FTTokens
        {
            get;
            set;
        }

        public string[] CSTokens
        {
            get;
            set;
        }

        public string[] FTPosTags
        {
            get;
            set;
        }

        public string[] CSPosTags
        {
            get;
            set;
        }

        public string[] Sentences
        {
            get;
            set;
        }

        public List<int> FTStartIndices
        {
            get;
            set;
        }

        public List<int> CSStartIndices
        {
            get;
            set;
        }

        public List<int> FTEndIndices
        {
            get;
            set;
        }

        public List<int> PauseDownList
        {
            get;
            set;
        }

        public DataSet DsCollection
        {
            get;
            set;
        }

        public DataSet DsFeatures
        {
            get;
            set;
        }

        public FeatureExtractorArguments()
        {
            // Extend the data in the message received to include PauseMS
            DsCollection = new DataSet();
            DataTable dtCollection = new DataTable();
            dtCollection.Columns.Add(new DataColumn("SecurityId", typeof(string)));
            dtCollection.Columns.Add(new DataColumn("SampleId", typeof(string)));
            dtCollection.Columns.Add(new DataColumn("KeyEvent", typeof(string)));
            dtCollection.Columns.Add(new DataColumn("VkCode", typeof(ushort)));
            dtCollection.Columns.Add(new DataColumn("AbsoluteTimestamp", typeof(long)));
            dtCollection.Columns.Add(new DataColumn("PauseMS", typeof(long)));
            DsCollection.Tables.Add(dtCollection);
        }

        public void SetArgumentsFromKeystrokeData(ref DataSet dsCollector, ref DataSet dsFeatures)
        {
            DataTable dtCollection = DsCollection.Tables[0];
            dtCollection.Clear();

            // Merge data from message into new DataSet
            foreach (DataRow dr in dsCollector.Tables[0].Rows)
                dtCollection.Rows.Add(dr["SecurityId"], dr["SampleId"], dr["KeyEvent"], dr["VkCode"], dr["AbsoluteTimestamp"], 0); // dsFeaturesInliers.Tables[0].ImportRow(dr);

            DsFeatures = dsFeatures;

            // Generate the PauseMS (pause between KPLs of all key presses in the sample
            long? prevTimestamp = null;
            foreach (DataRow row in dtCollection.Rows)
            {
                // Only mesaure PauseMS for key presses
                if (Convert.ToInt32(row["KeyEvent"]) == 1)
                {
                    long currTimestamp = Convert.ToInt64(row["AbsoluteTimestamp"]);
                    row["PauseMS"] = (prevTimestamp != null) ? currTimestamp - prevTimestamp : 0;
                    prevTimestamp = currTimestamp;
                }
            }

            // Parse key strokes in Data Converter to generate final text and char stream
            ParseKeyStrokes(DsCollection);

            // Get start and end indices
            FTStartIndices = GetStartIndex(FinalText);
            CSStartIndices = GetStartIndex(CharStream);
            FTEndIndices = GetEndIndex(FinalText);

            PauseDownList = GetPauseDownList(dtCollection, pauseListThreshold);
        }

        public static List<int> GetPauseDownList(DataTable dtCollection, long pauseThreshold)
        {
            List<int> pauses = new List<int>();

            int prevDownRow = -1, keyPressIndex = 0;

            for (int i = 0; i < dtCollection.Rows.Count; i++)
            {
                DataRow row = dtCollection.Rows[i];
                if (Convert.ToInt16(row["KeyEvent"]) == 1)
                {
                    if (prevDownRow > -1)
                    {
                        long pause = Convert.ToInt64(row["AbsoluteTimestamp"]) - Convert.ToInt64(dtCollection.Rows[prevDownRow]["AbsoluteTimestamp"]);
                        if (pause > pauseThreshold)
                            pauses.Add(keyPressIndex);
                    }
                    keyPressIndex++;
                    prevDownRow = i;
                }
            }
            return pauses;
        }

        // retrieves the Spans (OpenNLP class) of the token; returns the starting index of each token
        public List<int> GetStartIndex(String rawStr)
        {
            List<int> startIndexes = new List<int>();

            return startIndexes;
        }

        public List<int> GetEndIndex(String rawStr)
        {
            List<int> endIndices = new List<int>();

            return endIndices;
        }

        private void ParseKeyStrokes(DataSet dsCollector)
        {
            CharStream = "";
            DataTable dtCollector = dsCollector.Tables[0];

            string sampleId = dsCollector.Tables[0].Rows[0].Field<string>("SampleId");
            string securityId = dsCollector.Tables[0].Rows[0].Field<string>("SecurityId");

            StringBuilder output = new StringBuilder();
            int startIndexOffset = 0, cursorPosition = 0;
            byte shiftDown = 0;
            SelectionBounds selection = new SelectionBounds();

            // Extract individual key strokes from each row
            for (int i = 0; i < dtCollector.Rows.Count; i++)
            {

                DataRow row = dtCollector.Rows[i];

                int eve = Convert.ToInt16(row["KeyEvent"]);
                long when = Convert.ToInt64(row["AbsoluteTimestamp"]);
                int vkCode = Convert.ToInt32(row["VkCode"]);
                char keyChar = VkCodeToChar(vkCode, shiftDown != 0);

                if (keyChar != 65535 && eve == 1)
                    CharStream += keyChar;

                if (eve == 1)
                {
                    if (vkCode == 8 || vkCode == 25)
                        cursorPosition--;
                    else
                        cursorPosition++;
                    if (cursorPosition < 0)
                        cursorPosition = 0;
                }
                KeyStroke key;
                key.isKeyPress = eve == 1;
                key.keyChar = keyChar;
                key.keyCode = vkCode;
                key.timestamp = when;
                key.cursorPosition = cursorPosition;

                shiftDown = ProcessKeyStroke(output, startIndexOffset, shiftDown, selection, key);
            }
            FinalText = output.ToString();
        }

        private static byte ProcessKeyStroke(StringBuilder output, int startIndexOffset, byte shiftDown, SelectionBounds selection, KeyStroke k)
        {
            int relativeIndex = k.cursorPosition - startIndexOffset;
            if (relativeIndex < 0)
                relativeIndex = 0;
            if (k.isKeyPress)
            {
                switch (k.keyCode)
                {
                    case VK_BACK: // Backspace
                        if (relativeIndex == 0)
                        {
                            break;
                        }
                        if(output.Length > 0)
                        {
                            if (!selection.isEmpty())
                            {
                                if (selection.start < 0)
                                    selection.start = 0;
                                else if (selection.start >= output.Length)
                                    selection.start = output.Length - 1;

                                if (selection.end < 0)
                                    selection.end = 0;
                                else if (selection.end >= output.Length)
                                    selection.end = output.Length - 1;

                                selection.checkOrder();

                                output.Remove(selection.start, selection.end - selection.start);
                                selection.start = selection.end = -1;
                            }
                            else
                            {
                                if (relativeIndex > output.Length - 1)
                                    relativeIndex = output.Length - 1;
                                output.Remove(relativeIndex, 1);
                            }
                        }
                        break;

                    case VK_SHIFT:
                        shiftDown |= 0x04;
                        if (selection.isActive())
                        { // There is already an active
                            // selection -- hitting select again
                            // shouldn't change anything
                            break;
                        }
                        // selection
                        selection.start = relativeIndex;
                        selection.end = relativeIndex;
                        break;
                    case VK_LSHIFT:
                        shiftDown |= 0x02;
                        if (selection.isActive())
                        { // There is already an active
                            // selection -- hitting select again
                            // shouldn't change anything
                            break;
                        }
                        // selection
                        selection.start = relativeIndex;
                        selection.end = relativeIndex;
                        break;
                    case VK_RSHIFT: // Shift
                        shiftDown |= 0x01;
                        if (selection.isActive())
                        { // There is already an active
                            // selection -- hitting select again
                            // shouldn't change anything
                            break;
                        }
                        // selection
                        selection.start = relativeIndex;
                        selection.end = relativeIndex;
                        break;
                    case VK_DELETE: // Delete
                        if (relativeIndex > output.Length - 1)
                        {
                            break;
                        }
                        if (!selection.isEmpty())
                        {
                            selection.checkOrder();
                            output.Remove(selection.start, selection.end - selection.start);
                        }
                        else
                        {
                            output.Remove(relativeIndex, 1);
                        }
                        selection.start = selection.end = -1;

                        break;
                    case VK_LEFT: // 37: // Arrows
                        if (shiftDown == 0)
                        {
                            // clear the selection, if not currently shifting.
                            selection.start = selection.end = -1;
                        }
                        else if (selection.isActive())
                        {
                            selection.end = relativeIndex - 1;
                        }
                        break;
                    case VK_RIGHT: // 39:
                        if (shiftDown == 0)
                        {
                            // clear the selection, if not currently shifting.
                            selection.start = selection.end = -1;
                        }
                        else if (selection.isActive())
                        {
                            selection.end = relativeIndex + 1;
                        }
                        break;
                    case 38:
                    case 40:
                        if (shiftDown == 0)
                        {
                            // clear the selection, if not currently shifting.
                            selection.start = selection.end = -1;
                        }
                        else if (selection.isActive())
                        {
                            selection.end = relativeIndex;
                        }
                        break;
                    default:
                        if (k.keyChar == 65535)
                        {
                            break;
                        }
                        if (k.keyChar < 9) // nonprinted
                        {
                            break;
                        }
                        if (k.keyChar > 11 && k.keyChar < 13)
                        {
                            break;
                        }
                        if (k.keyChar > 13 && k.keyChar < 32)
                        {
                            break;
                        }
                        // Printable characters
                        if (selection.isActive() && !selection.isEmpty())
                        {
                            selection.checkOrder();
                            if (selection.start < output.Length)
                            {
                                if (selection.end < output.Length)
                                    output.Remove(selection.start, selection.end - selection.start);
                                else
                                    output.Remove(selection.start, output.Length - selection.start);
                            }
                        }
                        if (relativeIndex < 0)
                        {
                            relativeIndex = 0;
                        }
                        if (relativeIndex > output.Length)
                        {
                            relativeIndex = output.Length;
                        }
                        output.Insert(relativeIndex, k.keyChar);
                        // After a printed character the selection is cleared.
                        selection.start = selection.end = -1;
                        break;
                }
            }
            else
            {
                if (k.keyCode == VK_LSHIFT)
                {
                    shiftDown &= 0xFD;
                }
                else if (k.keyCode == VK_SHIFT)
                {
                    shiftDown &= 0xFB;
                }
                else if (k.keyCode == VK_RSHIFT)
                {
                    shiftDown &= 0xFE;
                }
            }
            return shiftDown;
        }

        public static char VkCodeToChar(int vkCode, bool shiftDown)
        {
            // Check if character is alpha
            if (vkCode >= 65 && vkCode <= 90)
            {
                return (shiftDown) ? (char)vkCode : (char)(vkCode + 32);
            }
            // Check for specific characters we are interested in
            switch (vkCode)
            {

                case VK_0:

                    return (shiftDown) ? ')' : '0';

                case VK_1:

                    return (shiftDown) ? '!' : '1';

                case VK_2:

                    return (shiftDown) ? '@' : '2';

                case VK_3:

                    return (shiftDown) ? '#' : '3';

                case VK_4:

                    return (shiftDown) ? '$' : '4';

                case VK_5:

                    return (shiftDown) ? '%' : '5';

                case VK_6:

                    return (shiftDown) ? '^' : '6';

                case VK_7:

                    return (shiftDown) ? '&' : '7';

                case VK_8:

                    return (shiftDown) ? '*' : '8';

                case VK_9:

                    return (shiftDown) ? '(' : '9';

                case VK_TAB:

                    return '\t';

                case VK_OEM_3:

                    return (shiftDown) ? '~' : '`';

                case VK_OEM_5:

                    return (shiftDown) ? '|' : '\\';

                case VK_OEM_2:

                    return (shiftDown) ? '?' : '/';

                case VK_OEM_COMMA:

                    return (shiftDown) ? '<' : ',';

                case VK_OEM_1:

                    return (shiftDown) ? ':' : ';';

                case VK_OEM_7:

                    return (shiftDown) ? '"' : '\'';

                case VK_BACK:

                    return (char)8;

                case VK_RETURN:

                    return '\n';

                case VK_SPACE:

                    return ' ';

                case VK_OEM_PERIOD:

                    return (shiftDown) ? '>' : '.';

                case VK_OEM_4:

                    return (shiftDown) ? '{' : '[';

                case VK_OEM_6:

                    return (shiftDown) ? '}' : ']';

                case VK_NUMPAD0:

                    return '0';

                case VK_NUMPAD1:

                    return '1';

                case VK_NUMPAD2:

                    return '2';

                case VK_NUMPAD3:

                    return '3';

                case VK_NUMPAD4:

                    return '4';

                case VK_NUMPAD5:

                    return '5';

                case VK_NUMPAD6:

                    return '6';

                case VK_NUMPAD7:

                    return '7';

                case VK_NUMPAD8:

                    return '8';

                case VK_NUMPAD9:

                    return '9';

                case VK_ADD:

                    return '+';

                case VK_SUBTRACT:

                    return '-';

                case VK_DECIMAL:

                    return '.';

                case VK_DIVIDE:

                    return '/';

                case VK_MULTIPLY:

                    return '*';

                case VK_OEM_PLUS:

                    return (shiftDown) ? '+' : '=';
                    
                case VK_OEM_MINUS:

                    return (shiftDown) ? '_' : '-';

                default:

                    return (char)65535;
            }
        }
    }

    public class SelectionBounds
    {
        public int start;
        public int end;

        public SelectionBounds()
        {
            start = -1;
            end = -1;
        }

        public void checkOrder()
        {
            if (start > end)
            {
                int tmp = start;
                start = end;
                end = tmp;
            }
        }

        public bool isEmpty()
        {
            return start == end;
        }

        public bool isActive()
        {
            return (start != -1 && end != -1);
        }
    }
}
