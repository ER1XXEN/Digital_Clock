using Digital_Clock.Helper;
using Digital_Clock.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Digital_Clock.Templates
{
    /// <summary>
    /// Interaction logic for AlarmTemplate.xaml
    /// </summary>
    public partial class AlarmTemplate : UserControl
    {
         public MainWindow win;

        public AlarmTemplate() => InitializeComponent();

        public void SetUp()
        {
            win = (MainWindow)Window.GetWindow(this);

            Alarm A = (Alarm)this.DataContext;
            foreach (Label item in HelperMethods.FindWindowChildren<Label>(WeekDays_Panel))
                if (A.DaysToRepeat.Any(x => (int)x == Convert.ToInt32(item.Tag)))
                    item.Foreground = Brushes.Red;
                else
                    item.Foreground = Brushes.White;
        }

        private void ToggleAlarm(object sender, MouseButtonEventArgs e) => win.ToggleAlarm();

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

        private void EditAlarm(object sender, MouseButtonEventArgs e)
        {
            Alarm A = (Alarm)this.DataContext;
            if (A.Snooze)
                return;
            Control control = (Control)sender;
            control.Visibility = Visibility.Hidden;
            EditAlarm_Panel.Visibility = Visibility.Visible;
            EditAlarmHours.Text = A.AlarmTime.Hours.ToString("##");
            EditAlarmMinutes.Text = A.AlarmTime.ToString("mm");
            EditAlarmHours.Focus();
            EditAlarmHours.CaretIndex = 2;
        }

        private void EditAlarm_Panel_MouseLeave(object sender, MouseEventArgs e)
        {
            win.EditAlarm(EditAlarmHours.Text, EditAlarmMinutes.Text);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) => SetUp();

        private void ToggleDay(object sender, MouseButtonEventArgs e)
        {
            Label item = (Label)sender;
            win.ToggleAlarmDays(Convert.ToInt32(item.Tag));
        }
    }
}