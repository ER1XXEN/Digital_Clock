using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Digital_Clock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer TimerClock = new DispatcherTimer();
        private DispatcherTimer TimerStopWatch = new DispatcherTimer();
        private Stopwatch stopWatch = new Stopwatch();

        public MainWindow()
        {
            InitializeComponent();
            SetUp();
        }

        #region Custom_Methods

        /// <summary>
        /// Start default procedures
        /// </summary>
        public void SetUp()
        {
            TimerClock.Tick += new EventHandler(Timer_Click);
            TimerClock.Interval = new TimeSpan(0, 0, 1);
            TimerClock.Start();
            TimerStopWatch.Tick += new EventHandler(StopWatchTimer);
            TimerStopWatch.Interval = new TimeSpan(0, 0, 1);
            TimerStopWatch.Start();
            Change_Panel("Home");
        }

        /// <summary>
        /// Change panel to Targeted panel
        /// </summary>
        /// <param name="Panel">Panel to switch to</param>
        private void Change_Panel(string Panel)
        {
            foreach (Canvas item in FindWindowChildren<Canvas>(Body))
            {
                if (item.Name.Split('_')[0].Contains(Panel))
                    item.Visibility = Visibility.Visible;
                else
                    item.Visibility = Visibility.Hidden;
            }
        }

        #endregion Custom_Methods

        #region Control_Methods

        private void Timer_Click(object sender, EventArgs e) => Clock_Index_lbl.Content = string.Format("{0:HH:mm:ss}", DateTime.Now);

        private void StopWatchTimer(object sender, EventArgs e) => StopWatch_Index_lbl.Content = string.Format("{0:00}:{1:00}:{2:00}", stopWatch.Elapsed.Hours, stopWatch.Elapsed.Minutes, stopWatch.Elapsed.Seconds);

        private void Menu_img_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => Change_Panel(((Image)sender).Tag.ToString());

        private void Start_StopWatch_img_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image img = (Image)sender;
            if (img.Tag.ToString() == "Start")
            {
                img.Tag = "Pause";
                img.Source = new BitmapImage(new Uri(@"Resources\pause-button.png", UriKind.Relative));
                stopWatch.Start();
                TimerStopWatch.Start();
            }
            else
            {
                img.Tag = "Start";
                img.Source = new BitmapImage(new Uri(@"Resources\play-button.png", UriKind.Relative));
                stopWatch.Stop();
                TimerStopWatch.Stop();
            }
        }

        private void Reset_StopWatch_img_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Start_StopWatch_img.Tag = "Start";
            Start_StopWatch_img.Source = new BitmapImage(new Uri(@"Resources\play-button.png", UriKind.Relative));
            stopWatch.Reset();
            TimerStopWatch.Stop();
            StopWatch_Index_lbl.Content = string.Format("{0:00}:{1:00}:{2:00}", stopWatch.Elapsed.Hours, stopWatch.Elapsed.Minutes, stopWatch.Elapsed.Seconds);
        }

        #endregion Control_Methods

        #region Helper_Methods

        /// <summary>
        /// Get controls of type in children of control
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dObj"></param>
        /// <returns></returns>
        public static IEnumerable<T> FindWindowChildren<T>(DependencyObject dObj) where T : DependencyObject
        {
            if (dObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dObj); i++)
                {
                    DependencyObject ch = VisualTreeHelper.GetChild(dObj, i);
                    if (ch != null && ch is T)
                    {
                        yield return (T)ch;
                    }

                    foreach (T nestedChild in FindWindowChildren<T>(ch))
                    {
                        yield return nestedChild;
                    }
                }
            }
        }

        #endregion Helper_Methods

        private void Reset_CountDown_img_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Start_StopWatch_img.Tag = "Start";
            Start_StopWatch_img.Source = new BitmapImage(new Uri(@"Resources\play-button.png", UriKind.Relative));
            stopWatch.Reset();
            TimerStopWatch.Stop();
            CountDown_Index_txt.Text = "00:00:00";
        }
    }
}