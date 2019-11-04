using System;

namespace TailwindTraders.ShippingManagement.Models
{
    public class Item
    {
        public int ID { get; set; }
        public bool HasPotentialErrors { get; set; }
        public ItemProperty Reference { get; set; }
        public ItemProperty Description { get; set; }
        public ItemProperty Quantity { get; set; }
        public ItemProperty Amount { get; set; }

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
