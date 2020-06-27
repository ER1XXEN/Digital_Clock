using Digital_Clock.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
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
using ToastNotifications.Messages;

namespace Digital_Clock.Templates
{
    /// <summary>
    /// Interaction logic for CountDownTemplate.xaml
    /// </summary>
    public partial class CountDownTemplate : UserControl
    {
        #region Variables
        private DispatcherTimer TimerCountDown = new DispatcherTimer();
        private DispatcherTimer CountDownClock = new DispatcherTimer();
        public MainWindow win;

        private int TimeLeft = 0;
        #endregion

        /// <summary>
        /// Initialize Template
        /// </summary>
        public CountDownTemplate()
        {
            InitializeComponent();

            TimerCountDown.Tick += new EventHandler(CountDownTimer);
            TimerCountDown.Interval = new TimeSpan(0, 0, 1);

            CountDownClock.Tick += new EventHandler(CountDownDone);
        }

        /// <summary>
        /// Updates value for countdown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CountDownTimer(object sender, EventArgs e)
        {
            TimeLeft = TimeLeft - 1;
            TimeSpan t = TimeSpan.FromSeconds(TimeLeft);
            CountDownHours.Text = t.ToString("hh");
            CountDownMinutes.Text = t.ToString("mm");
            CountDownSeconds.Text = t.ToString("ss");
        }

        /// <summary>
        /// Countdown is done
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CountDownDone(object sender, EventArgs e)
        {
            CountDownClock.Stop();
            TimerCountDown.Stop();
            if (CountDownClock.Interval == new TimeSpan())
            {
                StartCountDownImage.Source = new BitmapImage(new Uri(@"..\Resources\play-button.png", UriKind.Relative));
                return;
            }

            TimeLeft = TimeLeft - 1;
            TimeSpan t = TimeSpan.FromSeconds(TimeLeft);
            CountDownHours.Text = "00";
            CountDownMinutes.Text = "00";
            CountDownSeconds.Text = "00";

            SystemSounds.Exclamation.Play();

            //Brings window to front
            if (win.WindowState == WindowState.Minimized) win.WindowState = WindowState.Normal;
            win.Activate();
            win.Topmost = true;  // important
            win.Topmost = false; // important

            //Shows notification
            win.notifier.ShowInformation("Time's Up");
        }

        /// <summary>
        /// Reset Countdown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetCountDown(object sender, MouseButtonEventArgs e)
        {
            StartCountDownImage.Tag = "Start";
            StartCountDownImage.Source = new BitmapImage(new Uri(@"..\Resources\play-button.png", UriKind.Relative));
            TimerCountDown.Stop();
            CountDownHours.Text = "00";
            CountDownMinutes.Text = "00";
            CountDownSeconds.Text = "00";
        }

        /// <summary>
        /// Begins CountDown from inserted time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartCountDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
            CountDownClock.Interval = new TimeSpan(Convert.ToInt32(CountDownHours.Text), Convert.ToInt32(CountDownMinutes.Text), Convert.ToInt32(CountDownSeconds.Text));
            TimeLeft = (int)CountDownClock.Interval.TotalSeconds;
            Image img = (Image)sender;
            if (img.Tag.ToString() == "Start")
            {
                img.Tag = "Pause";
                img.Source = new BitmapImage(new Uri(@"..\Resources\pause-button.png", UriKind.Relative));
                CountDownClock.Start();
                TimerCountDown.Start();
            }
            else
            {
                img.Tag = "Start";
                img.Source = new BitmapImage(new Uri(@"..\Resources\play-button.png", UriKind.Relative));
                CountDownClock.Stop();
                TimerCountDown.Stop();
            }
        }

        /// <summary>
        /// Pause CountDown
        /// to allow editing of value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PauseCountDown(object sender, RoutedEventArgs e)
        {
            StartCountDownImage.Tag = "Start";
            StartCountDownImage.Source = new BitmapImage(new Uri(@"..\Resources\play-button.png", UriKind.Relative));
            CountDownClock.Stop();
            TimerCountDown.Stop();
        }

        /// <summary>
        /// Check if string is all numerals
        /// If not reset value to '00'
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextboxValueCheck(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;
            if (!HelperMethods.AreAllValidNumericChars(txt.Text))
            {
                txt.Text = "00";
                txt.CaretIndex = txt.Text.Length;
            }
        }

        /// <summary>
        /// Add Characters to get correct length
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextboxLostFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = (TextBox)sender;
            if (txt.Text.Length != 2)
                while (txt.Text.Length != 2)
                    txt.Text = "0" + txt.Text;
        }

        /// <summary>
        /// Set 'win' to MainWindow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e) => win = (MainWindow)Window.GetWindow(this);
    }
}
