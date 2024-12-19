using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KP.AdminWindow
{
    public class Meeting
    {
        public int ID_Meeting { get; set; }
        public string Status { get; set; }
        public DateTime Date { get; set; }
        public int ID_User { get; set; }
        public int ID_Rent { get; set; }
        public string UserName { get; set; }  
        public int PremisesDescription { get; set; }
    }
}
