using KP._KP11_01DataSetTableAdapters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KP.AdminWindow
{
    public partial class GeneralWindow : Window
    {
        int SelectedPemiseId = 0;
        PremisesTableAdapter premises = new PremisesTableAdapter();
        CommunicationsTableAdapter communicationsAdapter = new CommunicationsTableAdapter();
        Premises_CommunicationsTableAdapter premisesCommunicationsAdapter = new Premises_CommunicationsTableAdapter();
        PurposeTableAdapter PurposeTableAdapter = new PurposeTableAdapter();
        Purpose_PremisesTableAdapter purpose_permise = new Purpose_PremisesTableAdapter();
        ObservableCollection<Purpose> purposes = new ObservableCollection<Purpose>();
        int status = 0;

        private int? editingPremiseId = null;
        private List<Premise> allPremises;

        public GeneralWindow()
        {
            InitializeComponent();
            try
            {
                LoadPremises();
                LoadFloors();
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
                    var premisePurposes = LoadPurposesForPremise(premiseId);

                    premisesList.Add(new Premise
                    {
                        ID_Premises = premiseId,
                        Floor = row["Floor"].ToString(),
                        Square = Convert.ToDouble(row["Square"]),
                        Price = Convert.ToInt32(row["Price"]),
                        Status = Convert.ToBoolean(row["Status"]),
                        Description = row["Description"].ToString(),
                        Communications = LoadCommunicationsForPremise(premiseId),
                        Purposes = premisePurposes
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обработке строки данных: {ex.Message}");
                }
            }

            return premisesList;
        }
        private ObservableCollection<Purpose> LoadPurposesForPremise(int premiseId)
        {
            var purposesList = new ObservableCollection<Purpose>();
            var allPurpose = PurposeTableAdapter.GetData();
            var allPurposePremise = purpose_permise.GetData();

            foreach (var purposePremise in allPurposePremise)
            {
                if (purposePremise.ID_Premises == premiseId)
                {
                    var matchingPurpose = allPurpose.FirstOrDefault(p => p.ID_Purpose == purposePremise.ID_Purpose);
                    if (matchingPurpose != null)
                    {
                        purposesList.Add(new Purpose
                        {
                            ID_Purpose = matchingPurpose.ID_Purpose,
                            Name = matchingPurpose.Name
                        });
                    }
                }
            }

            return purposesList;
        }

        private ObservableCollection<Communication> LoadCommunicationsForPremise(int premiseId)
        {
            var communicationsList = new ObservableCollection<Communication>();
            var comforprem = premisesCommunicationsAdapter.GetData();
            var allCommunications = communicationsAdapter.GetData();
            string comName = "";
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
            purposes.Clear();
            var allPurpose = PurposeTableAdapter.GetData();
            var allpurpose_premise = purpose_permise.GetData();

            foreach (var purpose_Premise in allpurpose_premise)
            {
                if (purpose_Premise.ID_Premises == premiseId)
                {
                    var matchingPurpose = allPurpose.FirstOrDefault(item => item.ID_Purpose == purpose_Premise.ID_Purpose);
                    if (matchingPurpose != null)
                    {
                        // Проверяем, существует ли уже добавленный элемент
                        if (!purposes.Any(p => p.ID_Purpose == matchingPurpose.ID_Purpose))
                        {
                            purposes.Add(new Purpose
                            {
                                ID_Purpose = matchingPurpose.ID_Purpose,
                                Name = matchingPurpose.Name
                            });
                        }
                    }
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

            NewCommunicationCB.ItemsSource = communicationsList;
            NewCommunicationCB.DisplayMemberPath = "Name";
            NewCommunicationCB.SelectedValuePath = "ID_Communications";
        }

        private void LoadPremises()
        {
            allPremises = GetPremises();
            PremisesItemsControl.ItemsSource = allPremises;
        }

        public ObservableCollection<string> Floors { get; set; }

        private void LoadFloors()
        {
            var floorList = allPremises.Select(p => p.Floor).Distinct().ToList();
            Floors = new ObservableCollection<string>(floorList);
        }

        private void AddCommunicationButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewCommunicationCB.SelectedItem is Communication selectedCommunication && editingPremiseId.HasValue)
            {
                // Проверяем, существует ли такая связь в таблице `Premises_Communications`
                var existingCommunication = premisesCommunicationsAdapter.GetData()
                    .FirstOrDefault(row => row.ID_Communications == selectedCommunication.ID_Communications && row.ID_Premises == editingPremiseId.Value);

                if (existingCommunication == null)
                {
                    // Проверка, существует ли такая коммуникация в основной таблице `Communications`
                    var communicationExists = communicationsAdapter.GetData()
                        .Any(row => row.ID_Communications == selectedCommunication.ID_Communications);

                    if (communicationExists)
                    {
                        // Добавляем связь только если коммуникация существует
                        premisesCommunicationsAdapter.InsertQuery(selectedCommunication.ID_Communications, editingPremiseId.Value);
                        LoadPremises(); // Обновляем список помещений
                        LoadCommunicationsForCurrentPremise(); // Обновляем список коммуникаций для текущего помещения
                    }
                    else
                    {
                        MessageBox.Show($"Коммуникация с ID {selectedCommunication.ID_Communications} не найдена.");
                    }
                }
                else
                {
                    MessageBox.Show("Эта коммуникация уже добавлена для данного помещения.");
                }
            }
        }

        private void LoadCommunicationsForCurrentPremise()
        {
            if (editingPremiseId.HasValue)
            {
                var communications = LoadCommunicationsForPremise(editingPremiseId.Value);
                CommunicationsListBox.ItemsSource = communications;
            }
        }

        private void RemoveCommunicationButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CommunicationsListBox.SelectedItem is Communication selectedCommunication && editingPremiseId.HasValue)
            {
                try
                {
                    premisesCommunicationsAdapter.DeleteQuery(editingPremiseId.Value, selectedCommunication.ID_Communications);

                    LoadPremises();
                    LoadCommunicationsForCurrentPremise();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении коммуникации: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Выберите коммуникацию для удаления.");
            }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Ошибка при удалении коммуникации: {ex.Message}");
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SideMenu.Visibility = SideMenu.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void CleanBtn_Click(object sender, RoutedEventArgs e)
        {
            FloorCB.SelectedItem = null;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Pay_Click(object sender, RoutedEventArgs e)
        {
            PaymentWindow payment = new PaymentWindow();
            payment.Show();
            Close();
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            if (IsValidInput())
            {
                string floorText = ((ComboBoxItem)FloorCB.SelectedItem).Content.ToString();
                bool isAvailable = StatusCB.SelectedItem.ToString().Contains("Доступен");
                premises.InsertQuery(floorText, Convert.ToDouble(SquareTb.Text), Convert.ToInt32(PriceTb.Text), isAvailable, DiscriptionTb.Text);
                LoadPremises();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var premise = (sender as Button)?.DataContext as Premise;
            if (premise != null)
            {
                MessageBoxResult result = MessageBox.Show($"Вы уверены, что хотите удалить помещение ID: {premise.ID_Premises}?", "Подтверждение удаления", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    premises.DeleteQuery(premise.ID_Premises);
                    LoadPremises();
                    ClearFields();
                }
            }
        }

        private void ClearFields()
        {
            FloorCB.SelectedItem = null;
            SquareTb.Clear();
            PriceTb.Clear();
            StatusCB.SelectedItem = null;
            DiscriptionTb.Clear();
            CommunicationsListBox.ItemsSource = null;
        }

        private bool IsValidInput()
        {
            if (FloorCB.SelectedItem == null ||
                String.IsNullOrWhiteSpace(SquareTb.Text) ||
                String.IsNullOrWhiteSpace(PriceTb.Text) ||
                StatusCB.SelectedItem == null)
            {
                MessageBox.Show("Все поля должны быть заполнены");
                return false;
            }
            return true;
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            int minPrice = 0;
            int maxPrice = int.MaxValue;
            string floor = null;

            if (!string.IsNullOrWhiteSpace(MinPriceTb.Text))
            {
                int.TryParse(MinPriceTb.Text, out minPrice);
            }

            if (!string.IsNullOrWhiteSpace(MaxPriceTb.Text))
            {
                int.TryParse(MaxPriceTb.Text, out maxPrice);
            }

            if (FilterFloorCB.SelectedItem != null)
            {
                floor = (FilterFloorCB.SelectedItem as ComboBoxItem)?.Content.ToString();
            }

            var filteredPremises = allPremises.Where(p =>
                (string.IsNullOrEmpty(floor) || p.Floor == floor) &&
                (p.Price >= minPrice && p.Price <= maxPrice)
            ).ToList();

            PremisesItemsControl.ItemsSource = filteredPremises;
        }

        private void ResetFiltersBtn_Click(object sender, RoutedEventArgs e)
        {
            FilterFloorCB.SelectedItem = null;
            MinPriceTb.Clear();
            MaxPriceTb.Clear();
            LoadPremises();
        }

        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Premise premise)
            {
                SelectedPemiseId = premise.ID_Premises;
                editingPremiseId = premise.ID_Premises;
                var matchingItem = FloorCB.Items.Cast<ComboBoxItem>()
            .FirstOrDefault(item => item.Content.ToString() == premise.Floor);

                if (matchingItem != null)
                {
                    FloorCB.SelectedItem = matchingItem;
                }
                else
                {
                    MessageBox.Show($"Этаж '{premise.Floor}' не найден в выпадающем списке.");
                }

                SquareTb.Text = premise.Square.ToString();

                PriceTb.Text = premise.Price.ToString();
                StatusCB.SelectedIndex = premise.Status ? 0 : 1;
                DiscriptionTb.Text = premise.Description;

                premise.Communications = LoadCommunicationsForPremise(premise.ID_Premises);
                premise.Purposes = purposes;

                if (status == 0)
                {
                    CommunicationsListBox.ItemsSource = premise.Communications;
                }
                else if (status == 1)
                {
                    CommunicationsListBox.ItemsSource = premise.Purposes;

                }


            }
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
            editingPremiseId = null;
        }

        private void SquareTb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void SquareTb_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
        }

        private void PriceTb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void PriceTb_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
        }

        private void CleanBtn1_Click(object sender, RoutedEventArgs e)
        {
            FilterFloorCB.SelectedItem = null;
            MinPriceTb.Clear();
            MaxPriceTb.Clear();
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (editingPremiseId.HasValue && IsValidInput())
            {
                string floorText = ((ComboBoxItem)FloorCB.SelectedItem).Content.ToString();
                bool isAvailable = StatusCB.SelectedItem.ToString().Contains("Доступен");

                premises.UpdateQuery(floorText, Convert.ToDouble(SquareTb.Text), Convert.ToInt32(PriceTb.Text), isAvailable, DiscriptionTb.Text, editingPremiseId.Value);
                LoadPremises();
                ClearFields();
                editingPremiseId = null;
            }
        }

        private void User_Click(object sender, RoutedEventArgs e)
        {
            UserWindow user = new UserWindow();
            user.Show();
            Close();
        }

        private bool IsTextAllowed(string text)
        {
            return Regex.IsMatch(text, @"^[0-9]*(?:\.[0-9]*)?$");
        }

        private void ShowAssignmentsButton_Click(object sender, RoutedEventArgs e)
        {

            status = 1;
            CommunicationsListBox.ItemsSource = purposes;
            var PurposeList = PurposeTableAdapter.GetData().AsEnumerable()
                .Select(row => new Purpose
                {
                    ID_Purpose = (int)row["ID_Purpose"],
                    Name = row["Name"].ToString()
                }).ToList();
            NewCommunicationCB.ItemsSource = PurposeList;
            NewCommunicationCB.DisplayMemberPath = "Name";
            NewCommunicationCB.SelectedValuePath = "ID_Purpose";
        }

        private void ShowCommunicationsButton_Click(object sender, RoutedEventArgs e)
        {
            status = 0;
            var communicationsList = communicationsAdapter.GetData().AsEnumerable()
                .Select(row => new Communication
                {
                    ID_Communications = (int)row["ID_Communications"],
                    Name = row["Name"].ToString()
                }).ToList();

            NewCommunicationCB.ItemsSource = communicationsList;
            NewCommunicationCB.DisplayMemberPath = "Name";
            NewCommunicationCB.SelectedValuePath = "ID_Communications";
        }

        private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewCommunicationCB.SelectedValue == null || !editingPremiseId.HasValue)
            {
                MessageBox.Show("Выберите элемент и помещение для добавления.");
                return;
            }

            try
            {
                if (status == 1) // Добавляем Purpose в Purpose_PremisesTableAdapter
                {
                    MessageBox.Show("text +" + Convert.ToInt32(NewCommunicationCB.SelectedValue).ToString());
                    int selectedPurposeId = Convert.ToInt32(NewCommunicationCB.SelectedValue);

                    // Проверяем, существует ли такая связь
                    var existingPurpose = purpose_permise.GetData()
                        .FirstOrDefault(row => row.ID_Premises == editingPremiseId.Value && row.ID_Purpose == selectedPurposeId);

                    if (existingPurpose == null)
                    {
                        // Добавляем связь
                        purpose_permise.InsertQuery(selectedPurposeId, editingPremiseId.Value);
                        MessageBox.Show("Назначение успешно добавлено.");
                    }
                    else
                    {
                        MessageBox.Show("Это назначение уже добавлено для данного помещения.");
                    }
                }
                else if (status == 0) // Добавляем Communication в Premises_CommunicationsTableAdapter
                {
                    int selectedCommunicationId = Convert.ToInt32(NewCommunicationCB.SelectedValue);

                    // Проверяем, существует ли такая связь
                    var existingCommunication = premisesCommunicationsAdapter.GetData()
                        .FirstOrDefault(row => row.ID_Premises == editingPremiseId.Value && row.ID_Communications == selectedCommunicationId);

                    if (existingCommunication == null)
                    {
                        // Добавляем связь
                        premisesCommunicationsAdapter.InsertQuery(selectedCommunicationId, editingPremiseId.Value);
                        MessageBox.Show("Коммуникация успешно добавлена.");
                    }
                    else
                    {
                        MessageBox.Show("Эта коммуникация уже добавлена для данного помещения.");
                    }
                }
                else
                {
                    MessageBox.Show("Неизвестный статус. Пожалуйста, выберите корректный статус.");
                }

                // Обновляем данные
                LoadPremises();
                LoadCommunicationsForCurrentPremise();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}");
            }
        }


        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (CommunicationsListBox.SelectedItem == null || !editingPremiseId.HasValue)
            {
                MessageBox.Show("Выберите элемент для удаления.");
                return;
            }

            try
            {
                if (status == 0) // Удаляем связь из Premises_CommunicationsTableAdapter
                {
                    if (CommunicationsListBox.SelectedItem is Communication selectedCommunication)
                    {
                        MessageBox.Show(selectedCommunication.Name);
                        premisesCommunicationsAdapter.DeleteQuery(editingPremiseId.Value, selectedCommunication.ID_Communications);
                        MessageBox.Show("Коммуникация успешно удалена.");
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при выборе коммуникации.");
                    }
                }
                else if (status == 1) // Удаляем связь из Purpose_PremisesTableAdapter
                {
                    if (CommunicationsListBox.SelectedItem is Purpose selectedPurpose)
                    {
                        MessageBox.Show(selectedPurpose.Name);
                        purpose_permise.DeleteQuery(editingPremiseId.Value, selectedPurpose.ID_Purpose);
                        MessageBox.Show("Назначение успешно удалено.");
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при выборе назначения.");
                    }
                }
                else
                {
                    MessageBox.Show("Неизвестный статус. Пожалуйста, выберите корректный статус.");
                }

                // Обновляем данные
                LoadPremises();
                LoadCommunicationsForCurrentPremise();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}");
            }
        }


        private void NewCommunicationCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NewCommunicationCB.SelectedValue != null)
            {
                MessageBox.Show(NewCommunicationCB.SelectedValue.ToString());
            }
        }
    }
}

