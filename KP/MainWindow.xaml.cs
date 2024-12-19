using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using KP._KP11_01DataSetTableAdapters;

namespace KP
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MeetingTableAdapter meetingTableAdapter = new MeetingTableAdapter();
        }

        UserTableAdapter user = new UserTableAdapter();

        private void AuthBtn_Click(object sender, RoutedEventArgs e)
        {
            var detales = user.GetData().Rows;
            bool uncinfo = true;

            if (String.IsNullOrWhiteSpace(LoginTb.Text) || String.IsNullOrWhiteSpace(PasswordTb.Password))
                MessageBox.Show("Имеются пустые поля");
            else
            {
                for (int i = 0; i < detales.Count; i++)
                {
                    string storedLogin = detales[i][4].ToString(); 
                    string storedPasswordHash = detales[i][5].ToString();  
                    bool isAdmin = Convert.ToBoolean(detales[i][6]);
                    int userId = Convert.ToInt32(detales[i][0]);

                    if (storedLogin == LoginTb.Text)
                    {
                        if (VerifyPassword(PasswordTb.Password, storedPasswordHash))
                        {
                            if (isAdmin)
                            {
                                AdminWindow.GeneralWindow generalWindow = new AdminWindow.GeneralWindow();
                                generalWindow.Show();
                                Close();
                            }
                            else
                            {
                                UserWindow.GeneralWindow userWindow = new UserWindow.GeneralWindow(userId);
                                userWindow.Show();
                                Close();
                            }

                            uncinfo = true;
                            break;
                        }
                    }

                }

                if (!uncinfo)
                {
                    MessageBox.Show("Неверен логин или пароль");
                }
            }
        }

        private void RegBtn_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow register = new RegisterWindow();
            register.Show();
            Close();
        }

        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            byte[] hashBytes = Convert.FromBase64String(storedHash);
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 10000);
            byte[] enteredHash = pbkdf2.GetBytes(20);

            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != enteredHash[i])
                    return false;
            }
            return true;
        }
    }
}
