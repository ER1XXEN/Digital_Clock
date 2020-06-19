using Digital_Clock.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
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
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace Digital_Clock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variables

        private List<Alarm> Alarms = new List<Alarm>().OrderBy(x => x.AlarmTime).ToList();

        private DispatcherTimer TimerClock = new DispatcherTimer();
        private DispatcherTimer TimerStopWatch = new DispatcherTimer();
        private DispatcherTimer TimerActiveAlarm = new DispatcherTimer();
        private DispatcherTimer TimerCountDown = new DispatcherTimer();
        private DispatcherTimer CountDownClock = new DispatcherTimer();
        private Stopwatch stopWatch = new Stopwatch();

        private Alarm _ActiveAlarm = null;

        private Notifier notifier = new Notifier(cfg =>
        {
            cfg.PositionProvider = new WindowPositionProvider(
                parentWindow: Application.Current.MainWindow,
                corner: Corner.TopRight,
                offsetX: 10,
                offsetY: 10);

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(5),
                maximumNotificationCount: MaximumNotificationCount.FromCount(5));

            cfg.Dispatcher = Application.Current.Dispatcher;
        });

        private int TimeLeft = 0;
        private int snoozeTime = 1;

        #endregion Variables

        /// <summary>
        /// Initialize MainWindow
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            SetUp();
        }

        #region CustomMethods

        /// <summary>
        /// Start default procedures
        /// </summary>
        public void SetUp()
        {
            TimerClock.Tick += new EventHandler(TimerClick);
            TimerClock.Interval = new TimeSpan(0, 0, 1);
            TimerClock.Start();

            TimerActiveAlarm.Tick += new EventHandler(ActiveAlarmTimer);
            TimerActiveAlarm.Interval = new TimeSpan(0, 0, 1);
            TimerActiveAlarm.Start();

            TimerStopWatch.Tick += new EventHandler(StopWatchTimer);
            TimerStopWatch.Interval = new TimeSpan(0, 0, 1);

            CountDownClock.Tick += new EventHandler(CountDownDone);

            TimerCountDown.Tick += new EventHandler(CountDownTimer);
            TimerCountDown.Interval = new TimeSpan(0, 0, 1);

            Alarm TestAlarm = new Alarm();
            TestAlarm.AlarmTime = TestAlarm.AlarmTime.Add(new TimeSpan(0, 1, 0));
            TestAlarm.DaysToRepeat.Add(NumToEnum<WeekDays>((int)DateTime.Now.DayOfWeek));
            Alarms.Add(TestAlarm);
            Alarms.Add(new Alarm());
            AlarmListbox.ItemsSource = Alarms;

            SnoozeTime.Content = string.Format("({0} Min)", snoozeTime);

            ChangePanel("Home");
        }

        /// <summary>
        /// Change panel to Targeted panel
        /// </summary>
        /// <param name="Panel">Panel to switch to</param>
        private void ChangePanel(string Panel)
        {
            foreach (Canvas item in FindWindowChildren<Canvas>(Body))
            {
                if (item.Name.Split('_')[0] == Panel)
                    item.Visibility = Visibility.Visible;
                else
                    item.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Set correct data for active alarm
        /// </summary>
        private void SetActiveAlarmData()
        {
            ActiveAlarmContent.Text = _ActiveAlarm.Content;
            ActiveAlarm.Content = _ActiveAlarm._AlarmTime;
        }

        /// <summary>
        /// Toggle alarm and refresh items in Alarm_Listbox
        /// </summary>
        public void ToggleAlarm()
        {
            Alarm alarm = (Alarm)AlarmListbox.SelectedItem;
            alarm.Activated = !alarm.Activated;
            AlarmListbox.Items.Refresh();
            AlarmListbox.SelectedIndex = -1;
        }

        #endregion CustomMethods

        #region ControlMethods

        #region IntervalMethods

        /// <summary>
        /// Set Time for clock
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerClick(object sender, EventArgs e)
        {
            ClockIndex.Content = DateTime.Now.ToString("HH':'mm':'ss");
            if (Alarms.Any(x => x.AlarmTime.TotalSeconds == Math.Floor(DateTime.Now.TimeOfDay.TotalSeconds) && x.Activated /*&& x.DaysToRepeat.Any(y => (int)y == (int)DateTime.Now.DayOfWeek)*/))
            {
                Alarm alarm = Alarms.FirstOrDefault(x => x.AlarmTime.TotalSeconds == Math.Floor(DateTime.Now.TimeOfDay.TotalSeconds) && x.Activated /*&& x.DaysToRepeat.Any(y => (int)y == (int)DateTime.Now.DayOfWeek)*/);
                notifier.ShowInformation(alarm.Content);

                _ActiveAlarm = alarm;
                SetActiveAlarmData();
                ChangePanel("ActiveAlarm");

                //Brings window to front
                if (WindowState == WindowState.Minimized) WindowState = WindowState.Normal;
                Activate();
                Topmost = true;  // important
                Topmost = false; // important
                Focus();
            }
        }

        /// <summary>
        /// Updates value for stopwatch
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopWatchTimer(object sender, EventArgs e) => StopWatchIndex.Content = stopWatch.Elapsed.ToString("HH':'mm':'ss");

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
        /// Remind user of alarm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActiveAlarmTimer(object sender, EventArgs e)
        {
            if (_ActiveAlarm != null)
                SystemSounds.Beep.Play();
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
                StartCountDownImage.Source = new BitmapImage(new Uri(@"Resources\play-button.png", UriKind.Relative));
                return;
            }

            TimeLeft = TimeLeft - 1;
            TimeSpan t = TimeSpan.FromSeconds(TimeLeft);
            CountDownHours.Text = t.Hours.ToString("##");
            CountDownMinutes.Text = t.Minutes.ToString("##");
            CountDownSeconds.Text = t.Seconds.ToString("##");

            SystemSounds.Exclamation.Play();

            //Brings window to front
            if (WindowState == WindowState.Minimized) WindowState = WindowState.Normal;
            Activate();
            Topmost = true;  // important
            Topmost = false; // important
            Focus();

            //Shows notification
            notifier.ShowInformation("Time's Up");
        }

        #endregion IntervalMethods

        /// <summary>
        /// Change panel according to tag of image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuClick(object sender, MouseButtonEventArgs e) => ChangePanel(((Image)sender).Tag.ToString());

        /// <summary>
        /// Start StopWatch
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartStopWatch(object sender, MouseButtonEventArgs e)
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

        /// <summary>
        /// Reset StopWatch
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetStopWatch(object sender, MouseButtonEventArgs e)
        {
            StartStopWatchImage.Tag = "Start";
            StartStopWatchImage.Source = new BitmapImage(new Uri(@"Resources\play-button.png", UriKind.Relative));
            stopWatch.Reset();
            TimerStopWatch.Stop();
            StopWatchIndex.Content = stopWatch.Elapsed.ToString("HH':'mm':'ss");
        }

        /// <summary>
        /// Reset Countdown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetCountDown(object sender, MouseButtonEventArgs e)
        {
            StartStopWatchImage.Tag = "Start";
            StartStopWatchImage.Source = new BitmapImage(new Uri(@"Resources\play-button.png", UriKind.Relative));
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
                img.Source = new BitmapImage(new Uri(@"Resources\pause-button.png", UriKind.Relative));
                CountDownClock.Start();
                TimerCountDown.Start();
            }
            else
            {
                img.Tag = "Start";
                img.Source = new BitmapImage(new Uri(@"Resources\play-button.png", UriKind.Relative));
                CountDownClock.Stop();
                TimerCountDown.Stop();
            }
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
            if (!AreAllValidNumericChars(txt.Text))
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
        /// Create new Alarm and add to current list of alarms
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddNewAlarm(object sender, RoutedEventArgs e)
        {
            Alarm newAlarm = new Alarm();
            newAlarm.AlarmTime = new TimeSpan(Convert.ToInt32(AddAlarmHours.Text), Convert.ToInt32(AddAlarmMinutes.Text), 0);
            newAlarm.Content = AddAlarmContent.Text;
            Alarms.Add(newAlarm);
            AlarmListbox.Items.Refresh();
            AddAlarmHours.Text = "00";
            AddAlarmMinutes.Text = "00";
            AddAlarmContent.Text = "";
        }

        /// <summary>
        /// Reset content of AddAlarm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelAlarm(object sender, RoutedEventArgs e)
        {
            AddAlarmHours.Text = "00";
            AddAlarmMinutes.Text = "00";
            AddAlarmContent.Text = "";
            ChangePanel("Alarm");
        }

        /// <summary>
        /// Reset Alarm time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopAlarm(object sender, RoutedEventArgs e)
        {
            _ActiveAlarm.AlarmTime = _ActiveAlarm.AlarmTime.Subtract(new TimeSpan(0, snoozeTime * _ActiveAlarm.SnoozeTime, 0));
            _ActiveAlarm.Snooze = false;
            _ActiveAlarm.SnoozeTime = 0;
            AlarmListbox.Items.Refresh();
            ChangePanel("Alarm");
            _ActiveAlarm = null;
        }

        /// <summary>
        /// Snooze current Alarm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SnoozeAlarm(object sender, RoutedEventArgs e)
        {
            _ActiveAlarm.AlarmTime = _ActiveAlarm.AlarmTime.Add(new TimeSpan(0, snoozeTime, 0));
            _ActiveAlarm.Snooze = true;
            _ActiveAlarm.SnoozeTime = _ActiveAlarm.SnoozeTime + 1;
            AlarmListbox.Items.Refresh();
            ChangePanel("Alarm");
            ActiveAlarm = null;
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
            StartCountDownImage.Source = new BitmapImage(new Uri(@"Resources\play-button.png", UriKind.Relative));
            CountDownClock.Stop();
            TimerCountDown.Stop();
        }

        #endregion ControlMethods

        #region HelperMethods

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

        /// <summary>
        /// Convert number to enum of specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="number"></param>
        /// <returns></returns>
        public T NumToEnum<T>(int number)
        {
            return (T)Enum.ToObject(typeof(T), number);
        }

        /// <summary>
        ///  Checks if all character is numeral
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private bool AreAllValidNumericChars(string str)
        {
            foreach (char c in str)
            {
                if (c != '.')
                {
                    if (!Char.IsNumber(c))
                        return false;
                }
            }

            return true;
        }

        #endregion HelperMethods
    }
}