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
using System.Windows.Shapes;

namespace KP.AdminWindow
{
    /// <summary>
    /// Логика взаимодействия для PaymentWindow.xaml
    /// </summary>
    public partial class PaymentWindow : Window
    {
        public PaymentWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SideMenu.Visibility = SideMenu.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void General_Click(object sender, RoutedEventArgs e)
        {
            GeneralWindow general = new GeneralWindow();
            general.Show();
            Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Contract_Click(object sender, RoutedEventArgs e)
        {
            ContractWindow contract = new ContractWindow();
            contract.Show();
            Close();
        }

        private void Meet_Click(object sender, RoutedEventArgs e)
        {
            MeetWindow meetWindow = new MeetWindow();
            meetWindow.Show();
            Close();
        }

        private void User_Click(object sender, RoutedEventArgs e)
        {
            UserWindow userWindow = new UserWindow();
            userWindow.Show();
            Close();
        }

        private void CleanBtn1_Click(object sender, RoutedEventArgs e)
        {
            StatusCB.SelectedItem = null;
        }

        private void CleanBtn_Click(object sender, RoutedEventArgs e)
        {
            RentCB.SelectedItem = null;
        }

        private void Clean2Btn_Click(object sender, RoutedEventArgs e)
        {
            UtilitCB.SelectedItem = null;
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
