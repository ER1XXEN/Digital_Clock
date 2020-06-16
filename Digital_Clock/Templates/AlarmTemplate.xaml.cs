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
        public ObservableCollection<int> MyListViewBinding { get; set; }

        public AlarmTemplate()
        {
            InitializeComponent();

            MyListViewBinding = new ObservableCollection<int>();
        }

        private void Bell_Img_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow win = (MainWindow)Window.GetWindow(this);
            win.ToggleAlarm();
        }
    }
}