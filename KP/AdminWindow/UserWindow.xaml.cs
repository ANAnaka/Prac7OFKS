using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ControlzEx.Standard;
using KP._KP11_01DataSetTableAdapters;

namespace KP.AdminWindow
{
    public partial class UserWindow : Window
    {
        private UserTableAdapter usersAdapter = new UserTableAdapter();
        UserTableAdapter userTable = new UserTableAdapter();
        private ObservableCollection<User> allUsers;
        private int? editingUserId = null;

        public UserWindow()
        {
            InitializeComponent();
            try
            {
                LoadUsers(); 
                DataContext = this;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        private ObservableCollection<User> GetUsers()
        {
            var usersList = new ObservableCollection<User>();
            var dataTable = usersAdapter.GetData();

            foreach (var row in dataTable)
            {
                usersList.Add(new User
                {
                    ID_User = (int)row["ID_User"],
                    Surname = row["Surname"].ToString(),
                    Name = row["Name"].ToString(),
                    PhoneNum = row["PhoneNum"].ToString(),
                    Login = row["Login"].ToString(),
                    Role = row["Role"].ToString()
                });
            }

            return usersList;
        }


        private void LoadUsers()
        {
            allUsers = GetUsers();
            UsersItemsControl.ItemsSource = allUsers;  // Bind users to the ItemsControl (ListView or DataGrid)
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (editingUserId.HasValue)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить пользователя с ID {editingUserId}?", "Подтверждение удаления", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    usersAdapter.DeleteQuery(editingUserId.Value);
                    LoadUsers();  
                    ClearFields();
                    editingUserId = null; 
                }
            }
        }

        private void Card_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is User user)
            {
                editingUserId = user.ID_User;
                SurnameTb.Text = user.Surname;
                NameTb.Text = user.Name;
                PhoneTb.Text = user.PhoneNum;
                LoginTb.Text = user.Login;
                RoleCB.SelectedItem = user.Role;
            }
        }

        private void PremiseClick(object sender, RoutedEventArgs e)
        {
            GeneralWindow generalWindow = new GeneralWindow();
            generalWindow.Show();
            Close();
        }

        private void MeetClick(object sender, RoutedEventArgs e)
        {
            MeetWindow meetWindow = new MeetWindow();
            meetWindow.Show();
            Close();
        }

        private void ContractClick(object sender, RoutedEventArgs e)
        {
            ContractWindow contractWindow = new ContractWindow();
            contractWindow.Show();
            Close();
        }

        private void PaymentClick(object sender, RoutedEventArgs e)
        {
            PaymentWindow paymentWindow = new PaymentWindow();
            paymentWindow.Show();
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SideMenu.Visibility = SideMenu.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CleanBtn_Click(object sender, RoutedEventArgs e)
        {
            RoleCB.SelectedItem = null;
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            if (IsValidInput())
            {
                bool isAvailable = RoleCB.SelectedItem.ToString().Contains("Администратор");
                string encryptedPassword = SecurityHelper.EncryptPassword(PasswordTb.Password);

                userTable.InsertQuery(SurnameTb.Text, NameTb.Text, PhoneTb.Text, LoginTb.Text, encryptedPassword, isAvailable);

                ClearFields();
            }
        }

        private void PhoneTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            string input = PhoneTb.Text;
            input = new string(input.Where(char.IsDigit).ToArray());

            if (input.Length > 11)
            {
                input = input.Substring(0, 11);
            }

            string formattedInput = "+7";
            if (input.Length > 1)
            {
                formattedInput += "(" + input.Substring(1, Math.Min(3, input.Length - 1)) + ") ";
            }

            if (input.Length > 4)
            {
                formattedInput += input.Substring(4, Math.Min(3, input.Length - 4)) + "-";
            }

            if (input.Length > 7)
            {
                formattedInput += input.Substring(7, Math.Min(2, input.Length - 7)) + "-";
            }

            if (input.Length > 9)
            {
                formattedInput += input.Substring(9);
            }

            PhoneTb.Text = formattedInput;
            PhoneTb.SelectionStart = PhoneTb.Text.Length;
        }

        private bool IsValidInput()
        {
            if (RoleCB.SelectedItem == null ||
                String.IsNullOrWhiteSpace(SurnameTb.Text) ||
                String.IsNullOrWhiteSpace(NameTb.Text) ||
                String.IsNullOrWhiteSpace(LoginTb.Text) ||
                String.IsNullOrWhiteSpace(PhoneTb.Text) )
            {
                MessageBox.Show("Все поля должны быть заполнены");
                return false;
            }

            if (PhoneTb.Text.Length < 10)
            {
                MessageBox.Show("Номер телефона должен содержать 10 цифр.");
                return false;
            }

            return true;
        }

        private void ClearFields()
        {
            SurnameTb.Clear();
            NameTb.Clear();
            PhoneTb.Clear();
            LoginTb.Clear();
            PasswordTb.Clear();
            RoleCB.SelectedItem = null;
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (editingUserId.HasValue && IsValidInput())
            {
                try
                {
                    bool role = RoleCB.SelectedItem.ToString().Contains("Администратор");

                    usersAdapter.UpdateQuery(SurnameTb.Text, NameTb.Text, PhoneTb.Text, LoginTb.Text, role, editingUserId.Value);

                    LoadUsers();  
                    ClearFields();
                    editingUserId = null;  

                    MessageBox.Show("Данные пользователя успешно обновлены.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}");
                }
            }
        }

    }
}
