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
using KP._KP11_01DataSetTableAdapters;

namespace KP
{
    /// <summary>
    /// Логика взаимодействия для RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }
        UserTableAdapter user = new UserTableAdapter();


        private void RegBtn_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(SurnameTb.Text) || String.IsNullOrWhiteSpace(NameTb.Text)
                || String.IsNullOrWhiteSpace(LoginTb.Text) || String.IsNullOrWhiteSpace(PasswordTb.Password) || String.IsNullOrWhiteSpace(PNumbTb.Text))
            {
                MessageBox.Show("Имеются пустые поля");
            }
            else
            {

                var detales = user.GetData().Rows;
                bool loginExists = false;

                for (int i = 0; i < detales.Count; i++)
                {
                    if (detales[i][4].ToString() == LoginTb.Text)
                    {
                        loginExists = true;
                        break;
                    }
                }

                if (loginExists)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует");
                }
                else
                {
                    try
                    {
                        string encryptedPassword = SecurityHelper.EncryptPassword(PasswordTb.Password); 

                        user.InsertQuery(SurnameTb.Text, NameTb.Text, PNumbTb.Text, LoginTb.Text, encryptedPassword, false);

                        MessageBox.Show("Регистрация прошла успешно");

                        MainWindow mainWindow = new MainWindow();
                        mainWindow.Show();
                        Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при регистрации: " + ex.Message);
                    }
                }
            }
        }

        private void AuthBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }


    }
}
