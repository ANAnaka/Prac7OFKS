using KP._KP11_01DataSetTableAdapters;
using System;
using System.Collections.Generic;
using System.Data;
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
using KP.AdminWindow;

namespace KP.UserWindow
{
    /// <summary>
    /// Логика взаимодействия для MeetWindow.xaml
    /// </summary>
    public partial class MeetWindow : Window
    {
        private int userId;
        
        MeetingTableAdapter meetingTableAdapter = new MeetingTableAdapter();
        RentTableAdapter rentTableAdapter = new RentTableAdapter();
        UserTableAdapter userTableAdapter = new UserTableAdapter();
        public MeetWindow(int userId)
        {
            this.userId = userId;
     
            InitializeComponent();
            LoadUserRentals();
            LoadUserMeetings();
        }

        private void LoadUserRentals()
        {
            DataTable rentals = rentTableAdapter.GetData(); 
            DataView userRentals = new DataView(rentals)
            {
                RowFilter = $"ID_User = {userId}" 
            };

            PremisesCB.ItemsSource = userRentals;
            PremisesCB.DisplayMemberPath = "ID_Premises"; 
            PremisesCB.SelectedValuePath = "ID_Rent";     
        }
        private List<Meeting> GetUserMeetings()
        {
            var meetingCards = new List<Meeting>();

            try
            {
                if (meetingTableAdapter == null)
                {
                    Console.WriteLine("meetingTableAdapter is null.");
                    return meetingCards;
                }

                DataTable meetings = meetingTableAdapter.GetData();

                if (meetings == null)
                {
                    Console.WriteLine("DataTable is null.");
                    return meetingCards;
                }

                Console.WriteLine($"Total meetings retrieved: {meetings.Rows.Count}");

                foreach (DataRow row in meetings.Rows)
                {
                    Console.WriteLine($"ID_Meeting: {row["ID_Meeting"]}, ID_User: {row["ID_User"]}, Date: {row["Date"]}, ID_Rent: {row["ID_Rent"]}");
                }

                Console.WriteLine($"Filtering meetings for userId: {userId}");
                DataView userMeetings = new DataView(meetings)
                {
                    RowFilter = $"ID_User = {userId}" // Adjust if ID_User is a string
                };

                Console.WriteLine($"Filtered meetings count: {userMeetings.Count}");

                foreach (DataRowView row in userMeetings)
                {
                    try
                    {
                        if (row["ID_Rent"] == DBNull.Value || row["Date"] == DBNull.Value)
                        {
                            Console.WriteLine($"Skipping meeting due to null values. ID_Meeting: {row["ID_Meeting"]}");
                            continue;
                        }

                        int rentId = Convert.ToInt32(row["ID_Rent"]);
                        DataTable rentData = rentTableAdapter.GetData();
                        DataRow[] rentDetails = rentData.Select($"ID_Rent = {rentId}");

                        string userName = "Не назначен";
                        if (row["ID_User"] != DBNull.Value)
                        {
                            int userIdFromRow = Convert.ToInt32(row["ID_User"]);
                            DataTable userData = userTableAdapter.GetData();
                            DataRow[] userDetails = userData.Select($"ID_User = {userIdFromRow}");

                            if (userDetails.Length > 0)
                            {
                                userName = userDetails[0]["LastName"].ToString();
                            }
                        }

                        if (rentDetails.Length > 0)
                        {
                            meetingCards.Add(new Meeting
                            {
                                ID_Meeting = Convert.ToInt32(row["ID_Meeting"]),
                                Status = Convert.ToBoolean(row["Status"]) ? "Назначена" : "Завершена",
                                Date = Convert.ToDateTime(row["Date"]),

                                ID_Rent = rentId
                            });
                        }
                        else
                        {
                            Console.WriteLine($"Rent details not found for ID_Rent: {rentId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing meeting row: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке встреч: {ex.Message}");
            }

            return meetingCards;
        }

        private void LoadUserMeetings()
        {
            var userMeetings = GetUserMeetings();
            if (userMeetings.Count == 0)
            {
                MessageBox.Show("У данного пользователя нет встреч для отображения.");
            }

            MeetingItemsControl.ItemsSource = userMeetings;
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
            PremisesCB.SelectedIndex = -1;
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs(out string errorMessage))
            {
                MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DateTime selectedDate = DateDP.SelectedDate.Value;
            int selectedRentId = (int)PremisesCB.SelectedValue;

            meetingTableAdapter.InsertQuery(false, selectedDate.ToString(), selectedRentId);

            MessageBox.Show("Встреча успешно добавлена!");
        }

        private bool ValidateInputs(out string errorMessage)
        {
            if (DateDP.SelectedDate == null)
            {
                errorMessage = "Выберите дату встречи.";
                return false;
            }

            if (DateDP.SelectedDate.Value < DateTime.Today)
            {
                errorMessage = "Дата встречи не может быть позже сегодняшней.";
                return false;
            }

            if (PremisesCB.SelectedIndex == -1)
            {
                errorMessage = "Выберите помещение.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            SideMenu.Visibility = SideMenu.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

    }
}
