using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace TailwindTraders.ShippingManagement.Models
{
    public class PackagingSlip
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        public string Reference { get; set; }
        public string Date { get; set; }
        public string ReceivedBy { get; set; }
        public string Customer { get; set; }
        public decimal Amount { get; set; }
        public string Provider { get; set; }
        public string Location { get; set; }
        public bool LocationMatchs { get; set; }
        public int Boxes{ get; set; }
        public List<Item> Items { get; set; }

        public void SetProperty<T>(string property, T value)
        {
            if (GetType().GetProperty(property) != null)
            {
                GetType().GetProperty(property)
                         .SetValue(this, value);
            }
        }

        public T GetProperty<T>(string property)
        {
            try
            {
                object obj = GetType().GetProperty(property).GetValue(this, null);
                Type conversionType = typeof(T);
                return (T)Convert.ChangeType(obj, conversionType: conversionType);
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}
