using Digital_Clock.Helper;
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

        private List<AlarmWithMethods> Alarms = new List<AlarmWithMethods>().OrderBy(x => x.AlarmTime).ToList();
        private List<LapTime> LapTimes = new List<LapTime>().OrderBy(x => x.Number).ToList();

        private DispatcherTimer TimerStopWatch = new DispatcherTimer();
        private DispatcherTimer TimerActiveAlarm = new DispatcherTimer();
        private Stopwatch stopWatch = new Stopwatch();

        private AlarmWithMethods _ActiveAlarm = null;

        public Notifier notifier = new Notifier(cfg =>
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

        private int snoozeTime = 10;

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
            TimerActiveAlarm.Tick += new EventHandler(ActiveAlarmTimer);
            TimerActiveAlarm.Interval = new TimeSpan(0, 0, 1);
            TimerActiveAlarm.Start();

            TimerStopWatch.Tick += new EventHandler(StopWatchTimer);
            TimerStopWatch.Interval = new TimeSpan(0, 0, 1);



            AlarmWithMethods TestAlarm = new AlarmWithMethods();
            TestAlarm.AlarmTime = TestAlarm.AlarmTime.Add(new TimeSpan(0, 1, 0));
            int[] DArr = { 0, 2, 4, 5 };
            foreach (int item in DArr)
                TestAlarm.DaysToRepeat.Add(HelperMethods.NumToEnum<WeekDays>(item));
            Alarms.Add(TestAlarm);
            Alarms.Add(new AlarmWithMethods());
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
            foreach (Canvas item in HelperMethods.FindWindowChildren<Canvas>(Body))
            {
                if (item.Parent == Body)
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
            ActiveAlarmIndex.Content = _ActiveAlarm.GetTimeString();
        }

        /// <summary>
        /// Toggle alarm and refresh items in Alarm_Listbox
        /// </summary>
        public void ToggleAlarm()
        {
            AlarmWithMethods alarm = (AlarmWithMethods)AlarmListbox.SelectedItem;
            alarm.Activated = !alarm.Activated;
            AlarmListbox.Items.Refresh();
            AlarmListbox.SelectedIndex = -1;
        }

        /// <summary>
        /// Update selected alarm after edit
        /// </summary>
        /// <param name="Hours"></param>
        /// <param name="Minutes"></param>
        public void EditAlarm(string Hours, string Minutes)
        {
            AlarmWithMethods alarm = (AlarmWithMethods)AlarmListbox.SelectedItem;
            alarm.AlarmTime = new TimeSpan(Convert.ToInt32(Hours), Convert.ToInt32(Minutes), 0);
            AlarmListbox.Items.Refresh();
            AlarmListbox.SelectedIndex = -1;
        }

        /// <summary>
        /// Toggle chosen day for selected alarm
        /// </summary>
        /// <param name="number"></param>
        public void ToggleAlarmDays(int number)
        {
            AlarmWithMethods alarm = (AlarmWithMethods)AlarmListbox.SelectedItem;
            if (alarm.DaysToRepeat.Any(x => (int)x == number))
                alarm.DaysToRepeat.Remove(HelperMethods.NumToEnum<WeekDays>(number));
            else
                alarm.DaysToRepeat.Add(HelperMethods.NumToEnum<WeekDays>(number));
            alarm.DaysToRepeat.OrderBy(x => (int)x);
            AlarmListbox.Items.Refresh();
            AlarmListbox.SelectedIndex = -1;
        }

        #endregion CustomMethods

        #region ControlMethods

        #region IntervalMethods

        /// <summary>
        /// Updates value for stopwatch
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopWatchTimer(object sender, EventArgs e) => StopWatchIndex.Content = stopWatch.Elapsed.ToString("hh':'mm':'ss");


        /// <summary>
        /// Remind user of alarm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActiveAlarmTimer(object sender, EventArgs e)
        {
            if (ActiveAlarm_Panel.Visibility == Visibility.Visible)
                SystemSounds.Beep.Play();
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
            StopWatchIndex.Content = stopWatch.Elapsed.ToString("hh':'mm':'ss");
            LapTimes.Clear();
            LapTimeList.Items.Refresh();
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
        /// Create new Alarm and add to current list of alarms
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddNewAlarm(object sender, RoutedEventArgs e)
        {
            AlarmWithMethods newAlarm = new AlarmWithMethods();
            newAlarm.AlarmTime = new TimeSpan(Convert.ToInt32(AddAlarmHours.Text), Convert.ToInt32(AddAlarmMinutes.Text), 0);
            newAlarm.Content = AddAlarmContent.Text;
            foreach (Label item in HelperMethods.FindWindowChildren<Label>(AddAlarmWeekDays_Panel))
                if (item.Foreground == Brushes.Red)
                    newAlarm.DaysToRepeat.Add(HelperMethods.NumToEnum<WeekDays>(Convert.ToInt32(item.Tag)));
            Alarms.Add(newAlarm);
            AlarmListbox.Items.Refresh();
            AddAlarmHours.Text = "00";
            AddAlarmMinutes.Text = "00";
            AddAlarmContent.Text = "";
            foreach (Label item in HelperMethods.FindWindowChildren<Label>(AddAlarmWeekDays_Panel))
                item.Foreground = Brushes.White;
            ChangePanel("Alarm");
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
            _ActiveAlarm.ResetAlarm(snoozeTime);
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
            _ActiveAlarm.SnoozeAlarm(snoozeTime);
            AlarmListbox.Items.Refresh();
            ChangePanel("Alarm");
            _ActiveAlarm = null;
        }

        /// <summary>
        /// Add a new lap to the stopwatch
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LapTime(object sender, MouseButtonEventArgs e)
        {
            LapTimes.Add(new Models.LapTime(LapTimes.Count + 1, stopWatch.Elapsed, LapTimes.Count == 0 ? new TimeSpan(0, 0, 0) : LapTimes.ElementAt(LapTimes.Count - 1).TotalTime));
            LapTimeList.ItemsSource = LapTimes;
            LapTimeList.Items.Refresh();
        }

        /// <summary>
        /// Toggle DaysToRepeat for current alarm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleAddAlarmDay(object sender, MouseButtonEventArgs e)
        {
            Label item = (Label)sender;
            if (item.Foreground == Brushes.White)
                item.Foreground = Brushes.Red;
            else
                item.Foreground = Brushes.White;
        }

        /// <summary>
        /// Add new CountDown to list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddCountDown(object sender, MouseButtonEventArgs e) => CountDownListbox.Items.Add(null);
        #endregion ControlMethods

    }
}