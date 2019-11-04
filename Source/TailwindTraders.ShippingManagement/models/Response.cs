using Newtonsoft.Json;

namespace TailwindTraders.ShippingManagement.Models
{
    public class Response<T>
    {
        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }
    }

}
