using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using KP._KP11_01DataSetTableAdapters;
using KP.AdminWindow;

namespace KP.UserWindow
{
    /// <summary>
    /// Логика взаимодействия для GeneralWindow.xaml
    /// </summary>
    public partial class GeneralWindow : Window
    {
        PremisesTableAdapter premises = new PremisesTableAdapter();
        CommunicationsTableAdapter communicationsAdapter = new CommunicationsTableAdapter();
        Premises_CommunicationsTableAdapter premisesCommunicationsAdapter = new Premises_CommunicationsTableAdapter();
        RentTableAdapter rentAdapter = new RentTableAdapter();

        private int? editingPremiseId = null;
        private List<Premise> allPremises;
        private int userId;

        public GeneralWindow(int userId)
        {
            this.userId = userId;
            InitializeComponent();
            try
            {
                LoadPremises();
                LoadAllCommunications();
                DataContext = this;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        private List<Premise> GetPremises()
        {
            var premisesList = new List<Premise>();
            var dataTable = premises.GetData();

            foreach (DataRow row in dataTable.Rows)
            {
                try
                {
                    var premiseId = Convert.ToInt32(row["ID_Premises"]);
                    premisesList.Add(new Premise
                    {
                        ID_Premises = premiseId,
                        Floor = row["Floor"].ToString(),
                        Square = Convert.ToDouble(row["Square"]),
                        Price = Convert.ToInt32(row["Price"]),
                        Status = Convert.ToBoolean(row["Status"]),
                        Description = row["Description"].ToString(),
                        Communications = LoadCommunicationsForPremise(premiseId)
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обработке строки данных: {ex.Message}");
                }
            }

            return premisesList;
        }

        private ObservableCollection<Communication> LoadCommunicationsForPremise(int premiseId)
        {
            var communicationsList = new ObservableCollection<Communication>();
            var comforprem = premisesCommunicationsAdapter.GetData();
            var allCommunications = communicationsAdapter.GetData();
            string comName = "dasdada";
            foreach (var item in comforprem)
            {
                if (item.ID_Premises == premiseId)
                {
                    foreach (var community in allCommunications)
                    {
                        if (community.ID_Communications == item.ID_Communications)
                        {
                            comName = community.Name;
                        }
                    }
                    communicationsList.Add(new Communication
                    {
                        ID_Communications = item.ID_Communications,
                        Name = comName
                    });

                }
            }
            return communicationsList;
        }

        private void LoadAllCommunications()
        {
            var communicationsList = communicationsAdapter.GetData().AsEnumerable()
                .Select(row => new Communication
                {
                    ID_Communications = (int)row["ID_Communications"],
                    Name = row["Name"].ToString()
                }).ToList();
        }

        private void LoadPremises()
        {
            allPremises = GetPremises();
            PremisesItemsControl.ItemsSource = allPremises;
        }

        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Premise selectedPremise)
            {
                editingPremiseId = selectedPremise.ID_Premises;
                MessageBox.Show($"Вы выбрали помещение: {selectedPremise.Description}");
            }
        }


        private void MeetClick(object sender, RoutedEventArgs e)
        {
            MeetWindow meetWindow = new MeetWindow(userId);
            meetWindow.Show();
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

        private void RentButton_Click(object sender, RoutedEventArgs e)
        {
            if (editingPremiseId == null)
            {
                MessageBox.Show("Пожалуйста, выберите помещение.");
                return;
            }

            try
            {
                var selectedPremise = allPremises.FirstOrDefault(p => p.ID_Premises == editingPremiseId);

                if (selectedPremise == null)
                {
                    MessageBox.Show("Выбранное помещение не найдено.");
                    return;
                }

                if (!selectedPremise.Status)
                {
                    MessageBox.Show("Помещение уже занято. Выберите другое помещение.");
                    return;
                }

                rentAdapter.InsertQuery(editingPremiseId.Value, userId);

                premises.UpdateStatus(false, editingPremiseId.Value); 

                MessageBox.Show("Помещение успешно арендовано!");

                LoadPremises();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при аренде помещения: {ex.Message}");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SideMenu.Visibility = SideMenu.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
