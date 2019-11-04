using Newtonsoft.Json;
using System.Collections.Generic;

namespace TailwindTraders.ShippingManagement.Models
{
    public class WrapForm
    {
        [JsonProperty("keys")]
        public List<Key_> Keys { get; set; }
        [JsonProperty("tables")]
        public List<Table> Tables { get; set; }
    }

    public class Table
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("headers")]
        public List<Header> Headers { get; set; }
    }
    
    public class Key_ 
    {
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("property")]
        public string Property { get; set; }
        [JsonProperty("keypos")]
        public int KeyPos { get; set; }
        [JsonProperty("keyvalue")]
        public int KeyValue { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class Header
    {
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("property")]
        public string Property { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
