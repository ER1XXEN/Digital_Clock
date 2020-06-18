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

        private Alarm ActiveAlarm = null;

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

        #region Custom_Methods

        /// <summary>
        /// Start default procedures
        /// </summary>
        public void SetUp()
        {
            TimerClock.Tick += new EventHandler(Timer_Click);
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
            Alarm_Listbox.ItemsSource = Alarms;

            SnoozeTime_lbl.Content = string.Format("({0} Min)", snoozeTime);

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
                if (item.Name.Split('_')[0] == Panel)
                    item.Visibility = Visibility.Visible;
                else
                    item.Visibility = Visibility.Hidden;
            }
        }

        private void SetActiveAlarmData()
        {
            ActiveAlarmContent_lbl.Text = ActiveAlarm.Content;
            ActiveAlarm_lbl.Content = ActiveAlarm._AlarmTime;
        }

        #endregion Custom_Methods

        #region Control_Methods

        #region Interval_Methods

        /// <summary>
        /// Set Time for clock
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Click(object sender, EventArgs e)
        {
            Clock_Index_lbl.Content = DateTime.Now.ToString("HH':'mm':'ss");
            if (Alarms.Any(x => x.AlarmTime.TotalSeconds == Math.Floor(DateTime.Now.TimeOfDay.TotalSeconds) && x.Activated /*&& x.DaysToRepeat.Any(y => (int)y == (int)DateTime.Now.DayOfWeek)*/))
            {
                Alarm alarm = Alarms.FirstOrDefault(x => x.AlarmTime.TotalSeconds == Math.Floor(DateTime.Now.TimeOfDay.TotalSeconds) && x.Activated /*&& x.DaysToRepeat.Any(y => (int)y == (int)DateTime.Now.DayOfWeek)*/);
                notifier.ShowInformation(alarm.Content);

                ActiveAlarm = alarm;
                SetActiveAlarmData();
                Change_Panel("ActiveAlarm");

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
        private void StopWatchTimer(object sender, EventArgs e) => StopWatch_Index_lbl.Content = stopWatch.Elapsed.ToString("HH':'mm':'ss");

        /// <summary>
        /// Updates value for countdown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CountDownTimer(object sender, EventArgs e)
        {
            TimeLeft = TimeLeft - 1;
            TimeSpan t = TimeSpan.FromSeconds(TimeLeft);
            CountDown_Index_txt.Text = t.ToString("HH':'mm':'ss");
        }

        /// <summary>
        /// Remind user of alarm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActiveAlarmTimer(object sender, EventArgs e)
        {
            if (ActiveAlarm != null)
                SystemSounds.Beep.Play();
        }

        //Countdown is done
        private void CountDownDone(object sender, EventArgs e)
        {
            CountDownClock.Stop();
            TimerCountDown.Stop();
            if (CountDownClock.Interval == new TimeSpan())
            {
                Start_CountDown_img.Source = new BitmapImage(new Uri(@"Resources\play-button.png", UriKind.Relative));
                return;
            }

            TimeLeft = TimeLeft - 1;
            TimeSpan t = TimeSpan.FromSeconds(TimeLeft);
            CountDown_Index_txt.Text = t.ToString("HH':'mm':'ss");

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

        #endregion Interval_Methods

        /// <summary>
        /// Change panel according to tag of image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu_img_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => Change_Panel(((Image)sender).Tag.ToString());

        /// <summary>
        /// Start StopWatch
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Reset StopWatch
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Reset_StopWatch_img_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Start_StopWatch_img.Tag = "Start";
            Start_StopWatch_img.Source = new BitmapImage(new Uri(@"Resources\play-button.png", UriKind.Relative));
            stopWatch.Reset();
            TimerStopWatch.Stop();
            StopWatch_Index_lbl.Content = stopWatch.Elapsed.ToString("HH':'mm':'ss");
        }

        /// <summary>
        /// Reset Countdown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Reset_CountDown_img_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Start_StopWatch_img.Tag = "Start";
            Start_StopWatch_img.Source = new BitmapImage(new Uri(@"Resources\play-button.png", UriKind.Relative));
            TimerCountDown.Stop();
            CountDown_Index_txt.Text = "00:00:00";
        }

        /// <summary>
        /// Set Countdown time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CountDown_Index_txt_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (CountDown_Index_txt.Text.Length != 8)
            {
                CountDown_Index_txt.Text = "00:00:00";
                notifier.ShowError("Something went wrong");
                return;
            }
            string[] test = CountDown_Index_txt.Text.Split(':');
            if (test.Count() == 3 && !test.Any(x => x.Length != 2) && !test.Any(x => x.Any(ch => ch < '0' || ch > '9')))
            {
                CountDownClock.Interval = new TimeSpan(Convert.ToInt32(test[0]), Convert.ToInt32(test[1]), Convert.ToInt32(test[2]));
                TimeLeft = (int)CountDownClock.Interval.TotalSeconds;
                notifier.ShowSuccess("Time set");
            }
            else
            {
                CountDown_Index_txt.Text = "00:00:00";
                notifier.ShowError("Something went wrong");
            }
        }

        /// <summary>
        /// Begins CountDown from inserted time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Start_CountDown_img_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
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
        /// Pause CountDown
        /// to allow editing of value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CountDown_Index_txt_GotFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Start_CountDown_img.Tag = "Start";
            Start_CountDown_img.Source = new BitmapImage(new Uri(@"Resources\play-button.png", UriKind.Relative));
            TimerCountDown.Stop();
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

        public T NumToEnum<T>(int number)
        {
            return (T)Enum.ToObject(typeof(T), number);
        }

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

        #endregion Helper_Methods

        public void ToggleAlarm()
        {
            Alarm alarm = (Alarm)Alarm_Listbox.SelectedItem;
            alarm.Activated = !alarm.Activated;
            Alarm_Listbox.Items.Refresh();
            Alarm_Listbox.SelectedIndex = -1;
        }

        private void Alarm_txt_ValueChange(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;
            if (!AreAllValidNumericChars(txt.Text))
                txt.Text = "00";
            txt.CaretIndex = txt.Text.Length;
        }

        private void Alarm_txt_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = (TextBox)sender;
            if (txt.Text.Length != 2)
                txt.Text = "00";
        }

        private void AddAlarm(object sender, RoutedEventArgs e)
        {
            Alarm newAlarm = new Alarm();
            newAlarm.AlarmTime = new TimeSpan(Convert.ToInt32(Hours_txt.Text), Convert.ToInt32(Minutes_txt.Text), 0);
            newAlarm.Content = AlarmContent_txt.Text;
            Alarms.Add(newAlarm);
            Alarm_Listbox.Items.Refresh();
            Hours_txt.Text = "00";
            Minutes_txt.Text = "00";
            AlarmContent_txt.Text = "";
        }

        private void CancelAlarm(object sender, RoutedEventArgs e)
        {
            Hours_txt.Text = "00";
            Minutes_txt.Text = "00";
            AlarmContent_txt.Text = "";
        }

        private void StopAlarm(object sender, RoutedEventArgs e)
        {
            ActiveAlarm.AlarmTime = ActiveAlarm.AlarmTime.Subtract(new TimeSpan(0, snoozeTime * ActiveAlarm.SnoozeTime, 0));
            ActiveAlarm.Snooze = false;
            ActiveAlarm.SnoozeTime = 0;
            Alarm_Listbox.Items.Refresh();
            Change_Panel("Alarm");
            ActiveAlarm = null;
        }

        private void SnoozeAlarm(object sender, RoutedEventArgs e)
        {
            ActiveAlarm.AlarmTime = ActiveAlarm.AlarmTime.Add(new TimeSpan(0, snoozeTime, 0));
            ActiveAlarm.Snooze = true;
            ActiveAlarm.SnoozeTime = ActiveAlarm.SnoozeTime + 1;
            Alarm_Listbox.Items.Refresh();
            Change_Panel("Alarm");
            ActiveAlarm = null;
        }
    }
}