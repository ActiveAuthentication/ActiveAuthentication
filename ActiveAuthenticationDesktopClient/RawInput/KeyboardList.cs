using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.ServiceProcess;

namespace ActiveAuthenticationDesktopClient
{
    // This is the only class that was not a part of the RawInput project on Codeproject.
    public class KeyboardList
    {
        /// <summary>
        /// Reads the list of recognized keyboards
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static DataSet readKeyboardList(string path)
        {
            string ProfilePathFilename = path + @"\ValidKeyboards.xml";
            DataTable dtValidKeyboards;

            // Create the Profile data set.
            DataSet dsValidKeyboards = new DataSet();

            dtValidKeyboards = new DataTable("Keyboard");

            dtValidKeyboards.Columns.Add(new DataColumn("DeviceName", typeof(string)));
 
            dsValidKeyboards.Tables.Add(dtValidKeyboards);

            // Clear out the Profile data table.
            dtValidKeyboards.Rows.Clear();

            // Check to see if the Profile already exists in storage, load and return true.
            // If it does not exist, only return false.
            if (File.Exists(ProfilePathFilename))
            {
                dsValidKeyboards.ReadXml(ProfilePathFilename, XmlReadMode.ReadSchema);
            }
            else
            {
                // maybe do something here
            }
            // Return the file loaded status.  Default is false / bad load.
            return dsValidKeyboards;
        }
        /// <summary>
        /// Saves the list of recognized keyboards
        /// </summary>
        /// <param name="dsValidKeyboards"></param>
        /// <param name="path"></param>
        public static void SaveKeyboardList(DataSet dsValidKeyboards, string path)
        {
            try
            {
                string ProfilePathFilename = path + @"\ValidKeyboards.xml";
                ServiceController service = new ServiceController("ActiveAuthenticationService");
                service.ExecuteCommand(143);
                dsValidKeyboards.WriteXml(ProfilePathFilename);
                service.ExecuteCommand(142);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }



    }
}
