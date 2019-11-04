namespace TailwindTraders.ShippingManagement.Models
{
    public class Request
    {

        private string _source;
        public string Source 
        { 
            get { return _source;  }
            set { _source = value.Trim('"'); }
        }
       
        public string CurrentLocation { get; set; }
    }
}
