using System.Text.Json.Serialization;

namespace Businfo.Manage
{
    public class VendorModel
    {
        [JsonPropertyName("DocId")]
        public string DocId { get; set; }

        [JsonPropertyName("VendorName")]
        public string VendorName { get; set; }

        [JsonPropertyName("NoOfVehicle")]
        public string NoOfVehicle { get; set; }

        [JsonPropertyName("Contact1")]
        public string Contact1 { get; set; }

        [JsonPropertyName("Contact2")]
        public string Contact2 { get; set; }

        [JsonPropertyName("Email")]
        public string Email { get; set; }

        [JsonPropertyName("Website")]
        public string Website { get; set; }

        [JsonPropertyName("Address")]
        public string Address { get; set; }

        [JsonPropertyName("About")]
        public string About { get; set; }

        public string TotalBus { get; set; } = "n";

        public string MobileNo
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Contact1)) return Contact1;
                else if (!string.IsNullOrEmpty(Contact2)) return Contact2;
                else return "";
            }
        }
    }
}