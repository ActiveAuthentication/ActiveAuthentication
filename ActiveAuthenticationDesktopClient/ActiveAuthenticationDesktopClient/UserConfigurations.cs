using System;
using System.Text;
using System.Data;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace ActiveAuthenticationDesktopClient
{
    // Set of configurations specific to the user or owner
    public static class UserConfigurations
    {
        private static DataSet dsUserInfo;
        private static DataTable dtUserInfo;
        private static DataSet dsOwner;
        static UserConfigurations()
        {

            dsUserInfo = new DataSet();
            dtUserInfo = new DataTable();
            dsUserInfo.Tables.Add(dtUserInfo);
            dtUserInfo.Columns.Add(new DataColumn("Registered", typeof(bool)));
            dtUserInfo.Columns.Add(new DataColumn("MessagingAddress", typeof(string)));
            dtUserInfo.Columns.Add(new DataColumn("EmailAddress", typeof(string)));
            dtUserInfo.Columns.Add(new DataColumn("WindowTime", typeof(int)));
            Load();
            dsOwner = new DataSet();
            OwnerLoad();

        }

        static void Load()
        {
            string path = Configuration.profilePath + @"\Users.xml";
            DataSet dsImport = new DataSet();
            if (File.Exists(path))
            {
                dsImport.ReadXml(path);
                bool sameCols = true;
                foreach (DataColumn c in dtUserInfo.Columns)
                    if (!(dsImport.Tables[0].Columns.Contains(c.ColumnName)))
                        sameCols = false;
                if (sameCols)
                {
                    DataRow row = dsImport.Tables[0].Rows[0];
                    dtUserInfo.Rows.Add(Convert.ToBoolean(row["Registered"]), row["MessagingAddress"], row["EmailAddress"], Convert.ToInt32(row["WindowTime"]));
                }
                else
                {
                    Console.WriteLine("User Configurations DNE!!!");
                }
            }
        }

        public static void OwnerLoad()
        {
            dsOwner.Clear();
            if (File.Exists(Configuration.ownerFile))
            {
                DataTable dtOwner = new DataTable();
                dtOwner.Columns.Add(new DataColumn("Registered", typeof(bool)));
                dtOwner.Columns.Add(new DataColumn("MessagingAddress", typeof(string)));
                dtOwner.Columns.Add(new DataColumn("EmailAddress", typeof(string)));
                dtOwner.Columns.Add(new DataColumn("WindowTime", typeof(int)));
                dtOwner.Rows.Clear();
                dsOwner.Tables.Add(dtOwner);
                dsOwner.ReadXml(Configuration.ownerFile, XmlReadMode.ReadSchema);
                
            }
        }

        public static string OwnerEmail
        {
            get
            {
                if (dsOwner.Tables.Count > 0)
                    if (dsOwner.Tables[0].Rows.Count > 0)
                        return (string)dsOwner.Tables[0].Rows[0]["EmailAddress"];
                return null;
            }
        }

        public static string OwnerMessagingAddress
        {
            get
            {
                if (dsOwner.Tables.Count > 0)
                    if (dsOwner.Tables[0].Rows.Count > 0)
                        return (string)dsOwner.Tables[0].Rows[0]["MessagingAddress"];
                return null;
            }
        }

        public static void Save()
        {
            string path = Configuration.profilePath + @"\Users.xml";
            AADesktopClient.unlockFiles();
            dsUserInfo.WriteXml(path);
            AADesktopClient.lockFiles();

        }

        public static void SetUser(string messagingAddress, string EmailAddress)
        {
            dtUserInfo.Clear();
            dtUserInfo.Rows.Add(false, messagingAddress, EmailAddress, 120);
            Save();
        }

        public static bool Registered
        {
            get
        {
            if (dtUserInfo.Rows.Count > 0)
                return (bool)dtUserInfo.Rows[0]["Registered"];
            return false;
        }
        }

        public static void RegisterUser()
        {
            if (dtUserInfo.Rows.Count > 0)
                dtUserInfo.Rows[0]["Registered"] = true;
            Save();
        }

        public static string EmailAddress
        {
            get
        {
            if (dtUserInfo.Rows.Count > 0)
                    return (string)dtUserInfo.Rows[0]["EmailAddress"];
            return null;
        }
        }

        public static string MessagingAddress
        {
            get
        {
            if (dtUserInfo.Rows.Count > 0)
                    return (string)dtUserInfo.Rows[0]["MessagingAddress"];
            return null;
        }
        }

        public static int TrustTime
        {
            get
        {
            if (dtUserInfo.Rows.Count > 0)
                return (int)dtUserInfo.Rows[0]["WindowTime"];
            return -1;
        }
            set
        {
            if (dtUserInfo.Rows.Count > 0)
                    dtUserInfo.Rows[0]["WindowTime"] = value;
            try
            {
                Save();
            }
            catch(Exception e)
            {

            }

        }
        }
    }
}
