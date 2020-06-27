using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Digital_Clock.Templates
{
    /// <summary>
    /// Interaction logic for ClockTemplate.xaml
    /// </summary>
    public partial class ClockTemplate : UserControl
    {

        #region Variables
        private RotateTransform MinHandTr = new RotateTransform();
        private RotateTransform HourHandTr = new RotateTransform();
        private RotateTransform SecHandTr = new RotateTransform();

        private DispatcherTimer dT = new DispatcherTimer();

        #endregion
        /// <summary>
        /// Initialize controls in Template
        /// </summary>
        public ClockTemplate()
        {
            Loaded += Control_Loaded;
            this.InitializeComponent();
        }

        /// <summary>
        /// Begins the needed timers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            dT.Tick += dispatcher_Tick;
            //Set the interval of the Tick event to 1 sec
            dT.Interval = new TimeSpan(0, 0, 1);
            //Start the DispatcherTimer
            dT.Start();
        }

        /// <summary>
        /// Update clock values
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void dispatcher_Tick(object source, EventArgs e)
        {
            ClockTime_lbl.Content = DateTime.Now.ToString("hh:mm tt");
            MinHandTr.Angle = (DateTime.Now.Minute * 6);
            SecHandTr.Angle = (DateTime.Now.Second * 6);
            HourHandTr.Angle = (DateTime.Now.Hour * 30) + (DateTime.Now.Minute * 0.5);
            SecondHand.RenderTransform = SecHandTr;
            MinuteHand.RenderTransform = MinHandTr;
            HourHand.RenderTransform = HourHandTr;
        }


    }
}
