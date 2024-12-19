using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.UI.WebControls.Adapters;
using System.Windows;
using KP._KP11_01DataSetTableAdapters;

namespace KP.AdminWindow
{
    public partial class MeetWindow : Window
    {
        UserTableAdapter userAdapter = new UserTableAdapter();
        MeetingTableAdapter meetingsAdapter = new MeetingTableAdapter();
        RentTableAdapter rentAdapter = new RentTableAdapter();
        ObservableCollection<Meeting> allMeetings;
        public MeetWindow()
        {
            InitializeComponent();
            LoadStaffComboBox(); 
            LoadMeetings();
        }

        private ObservableCollection<Meeting> GetMeetings()
        {
            var meetingsList = new ObservableCollection<Meeting>();
            var dataTable = meetingsAdapter.GetData();

            foreach (var row in dataTable)
            {
                // Получаем имя пользователя по ID_User
                var user = userAdapter.GetData().FirstOrDefault(u => u.ID_User == (int)row["ID_User"]);
                var userName = user?.Surname ?? "Неизвестный пользователь";

                var rent = rentAdapter.GetData().FirstOrDefault(r => r.ID_Rent == (int)row["ID_Rent"]);
                var premisesDescription = rent?.ID_User ?? 0;

                meetingsList.Add(new Meeting
                {
                    ID_Meeting = (int)row["ID_Meeting"],
                    Status = row["Status"].ToString(),
                    Date = Convert.ToDateTime(row["Date"]),
                    UserName = userName,
                    PremisesDescription = premisesDescription
                });
            }

            return meetingsList;
        }

        private void LoadMeetings()
        {
            allMeetings = GetMeetings();
            MeetingsItemsControl.ItemsSource = allMeetings;
        }

        private void DeleteMeetingButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as FrameworkElement;
            if (button?.DataContext is Meeting meeting)
            {
                meetingsAdapter.DeleteQuery(meeting.ID_Meeting);
                allMeetings.Remove(meeting);
            }
        }
        private void LoadStaffComboBox()
        {
            var usersTable = userAdapter.GetData();
            var activeUsers = usersTable
                .Where(u => u.Role == true)
                .Select(u => new { u.ID_User, u.Surname })
                .ToList();

            StaffCB.ItemsSource = activeUsers;
            StaffCB.DisplayMemberPath = "Surname";
            StaffCB.SelectedValuePath = "Id";
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
