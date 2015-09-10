using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.IO;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Web.Security;
using System.Security.Principal;

namespace ActiveAuthenticationDesktopClient
{
    public partial class UserSelection : Form
    {
        public string newOwner { get; private set; }
        public List<string> invalidUsers { get; private set; }
        public bool canceled { get; private set; }
        private static string Purpose;
        /// <summary>
        /// Creates windows form that contains a list of users to either be invalidated or set as the owner depending on the circumstance
        /// </summary>
        /// <param name="purpose"></param>
        public UserSelection(string purpose)
        {
            InitializeComponent();
            canceled = true;
            Purpose = purpose;
            if (purpose.Equals("Owner"))
            {
                WarningMessage.Text = "WARNING:  You are about to change the individual Active Authentication considers the “owner” of this machine.  The owner can perform certain actions that no other authorized user is allowed to perform (such as removing users from the list of authorized users). Once you assign ownership to another individual you will no longer be considered the owner and will lose the ability to perform certain actions.  Are you sure you wish to proceed?";
                Label.Text = "Please select a new owner.";
                SelectQuery query = new SelectQuery("Win32_UserAccount");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                foreach (ManagementObject envVar in searcher.Get())
                {
                    if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\..\..\" + envVar["Name"] + @"\AppData\Roaming\Louisiana Tech University\Active Authentication\Profiles\Users.xml"))
                    {
                        if (!envVar["Name"].ToString().Equals(Environment.UserName))
                        {


                            PrincipalContext context = new PrincipalContext(ContextType.Machine);
                            UserPrincipal userPrincipal = UserPrincipal.FindByIdentity(context, envVar["Name"].ToString());
                            var groups = userPrincipal.GetGroups();
                            foreach (var group in groups.ToArray())
                            {
                                if (group.ToString().Equals("Administrators") || group.ToString().Equals("Administrator")) 
                                    UsersCheckList.Items.Add(envVar["Name"].ToString());
                            }

                        }
                    }
                }
            }
            if(purpose.Equals("Delete"))
            {
                WarningMessage.Text = "WARNING:  If you choose to remove a user from the list of authorized users that individual will no longer be considered a legitimate user of this machine, and Active Authentication will attempt to prevent him or her from accessing this machine.  Are you sure you wish to proceed?";
                Label.Text = "Select the user(s) to remove from the list of authorized users";
                SelectQuery query = new SelectQuery("Win32_UserAccount");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                foreach (ManagementObject envVar in searcher.Get())
                {
                    if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\..\..\" + envVar["Name"] + @"\AppData\Roaming\Louisiana Tech University\Active Authentication\Profiles\Users.xml")) 
                    {
                        if (!envVar["Name"].ToString().Equals(Environment.UserName))
                            UsersCheckList.Items.Add(envVar["Name"].ToString());
                    }

                }
            }
            UsersCheckList.ItemCheck += new ItemCheckEventHandler(UsersCheckList_ItemCheck);
        }
        /// <summary>
        /// Submits the check list and finalizes selections
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubmitButton_Click(object sender, EventArgs e)
        {
            if(Purpose.Equals("Owner"))
            {
                if (UsersCheckList.CheckedItems.Count > 0)
                {
                    DialogResult r = MessageBox.Show("Are you sure you want to change the owner of this computer?", "Confirmation", MessageBoxButtons.YesNo);
                    if (r.Equals(DialogResult.Yes))
                    {
                        newOwner = UsersCheckList.CheckedItems[0].ToString();
                        canceled = false;
                    }
                    else
                        MessageBox.Show("Owner Change Canceled", "Canceled");
                    this.Close();
                }

            }
            if(Purpose.Equals("Delete"))
            {
                if (UsersCheckList.CheckedItems.Count > 0)
                {
                    DialogResult r = MessageBox.Show("Are you sure you want to invalidate the selected users?", "Confirmation", MessageBoxButtons.YesNo);
                    if(r.Equals(DialogResult.Yes))
                    {
                        invalidUsers = new List<string>();
                        for(int i = 0; i < UsersCheckList.CheckedItems.Count; i++)
                        {
                            invalidUsers.Add(UsersCheckList.CheckedItems[i].ToString());
                        }
                        canceled = false;
                    }
                }
                this.Close();
            }
        }
        /// <summary>
        /// If the form is being used to select a new owner ensure that only one user is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UsersCheckList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (Purpose.Equals("Owner"))
            {
                for (int ix = 0; ix < UsersCheckList.Items.Count; ++ix)
                if (ix != e.Index) UsersCheckList.SetItemChecked(ix, false);
            }
        }
        /// <summary>
        /// Cancel this operation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
    }
}
