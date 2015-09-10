using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ActiveAuthenticationDesktopClient
{
    public partial class PauseLength : Form
    {
        private static Timer pauseWindow = new Timer();
        /// <summary>
        /// Initialize the form and set the default value to one hour
        /// </summary>
        public PauseLength()
        {
            InitializeComponent();
            Hours.Value = 1;
        }
        /// <summary>
        /// Submit the selected time for Active Authentication to be paused
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Submit_Click(object sender, EventArgs e)
        {
            this.Hide();
            AADesktopClient.Pause = true;
            pauseWindow.Interval = ((int)Hours.Value * 60 * 60 * 1000) + ((int)Mintues.Value * 60 * 1000);
            pauseWindow.Tick += new EventHandler(Unpause);
            AADesktopClient.ni.ContextMenu.MenuItems[0].Text = "Unpause";
            AADesktopClient.ni.ContextMenu.MenuItems[0].Click -= AADesktopClient.PauseAuth;
            AADesktopClient.ni.ContextMenu.MenuItems[0].Click += new EventHandler(Unpause);
            pauseWindow.Start();
        }
        /// <summary>
        /// Cnacels the pause operation and closes the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// Unpauses Active Authentication 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Unpause(object sender, EventArgs e)
        {
            AADesktopClient.Pause = false;
            AADesktopClient.ni.ContextMenu.MenuItems[0].Text = "Pause";
            AADesktopClient.ni.ContextMenu.MenuItems[0].Click -= Unpause;
            AADesktopClient.ni.ContextMenu.MenuItems[0].Click += new EventHandler(AADesktopClient.PauseAuth);
            pauseWindow.Stop();
            pauseWindow.Dispose();
            this.Close();
        }
        /// <summary>
        /// Updates the number boxes when the slider is moved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimeSlider_Scroll(object sender, EventArgs e)
        {
            Hours.Value = TimeSlider.Value/4;
            int totalMinutes = TimeSlider.Value * 15;
            while(totalMinutes >= 60)
            {
                totalMinutes = totalMinutes - 60;
            }
            Mintues.Value = totalMinutes;
        }
        /// <summary>
        /// Updates the slider when the hour box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hours_ValueChanged(object sender, EventArgs e)
        {
            TimeSlider.Value = ((int)Hours.Value * 4) + ((int)Mintues.Value / 15);
        }
        /// <summary>
        /// Updates the slider when the minute box is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Mintues_ValueChanged(object sender, EventArgs e)
        {
            TimeSlider.Value = ((int)Mintues.Value / 15) + ((int)Hours.Value * 4); 
        }

    }
}