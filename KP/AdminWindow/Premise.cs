using System.Collections.ObjectModel;
using System.ComponentModel;

namespace KP.AdminWindow
{
    public class Premise : INotifyPropertyChanged
    {
        private ObservableCollection<Communication> communications;
        private ObservableCollection<Purpose> purposes;
        public int ID_Premises { get; set; }
        public string Floor { get; set; }
        public double Square { get; set; }
        public double Price { get; set; }
        public bool Status { get; set; }
        public string Description { get; set; }
        public string StatusText => Status ? "Доступно" : "Забронировано";
        public ObservableCollection<Purpose> Purposes
        {
            get => purposes; 
            set
            {
                purposes = value;
                OnPropertyChanged(nameof(Purposes));
            }
        }

        public ObservableCollection<Communication> Communications
        {
            get => communications;
            set
            {
                communications = value;
                OnPropertyChanged(nameof(Communications));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public class Communication
    {
        public int ID_Communications { get; set; }
        public string Name { get; set; }
    }

    public class Purpose
    {
        public int ID_Purpose { get; set; }
        public string Name { get; set; }
    }
}
