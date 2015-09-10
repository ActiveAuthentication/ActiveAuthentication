using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ActiveAuthenticationDesktopClient
{
    public partial class fetchCarrierForm : Form
    {
        public string messagingAddress;

        public fetchCarrierForm()
        {
            InitializeComponent();
        }

        private void fetchButton_Click(object sender, EventArgs e)
        {
            messagingAddress = Messager.GetAddressForNumber(textBox1.Text,textBox2.Text);
            if (messagingAddress == "" || messagingAddress == null)
            {        
                addressLabel.Text = "Unable to fetch address, please check phone number and try again";
                messagingAddress = "Error";
            }
            else
                addressLabel.Text = messagingAddress;
        }

        private void confirmButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
