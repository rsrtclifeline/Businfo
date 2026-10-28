using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Businfo.Helper
{
    
    public class IndianData
    {
        public static List<StateData> stateDatas;
        public static readonly string[] States = new string[]
        {
            // 28 States
            "Andhra Pradesh",
            "Arunachal Pradesh",
            "Assam",
            "Bihar",
            "Chhattisgarh",
            "Goa",
            "Gujarat",
            "Haryana",
            "Himachal Pradesh",
            "Jharkhand",
            "Karnataka",
            "Kerala",
            "Madhya Pradesh",
            "Maharashtra",
            "Manipur",
            "Meghalaya",
            "Mizoram",
            "Nagaland",
            "Odisha",
            "Punjab",
            "Rajasthan",
            "Sikkim",
            "Tamil Nadu",
            "Telangana",
            "Tripura",
            "Uttar Pradesh",
            "Uttarakhand",
            "West Bengal"
        };

        public static readonly string[] UnionTerritories = new string[]
        {
            // 8 UTs
            "Andaman and Nicobar Islands",
            "Chandigarh",
            "Dadra and Nagar Haveli and Daman and Diu",
            "Delhi",
            "Jammu and Kashmir",
            "Ladakh",
            "Lakshadweep",
            "Puducherry"
        };

        
        // Optional: Combined list for easy use
        public static readonly string[] AllRegions;

        static IndianData()
        {
            var all = new List<string>();
            all.AddRange(States);
            all.AddRange(UnionTerritories);
            AllRegions = all.ToArray();
            var a = "Helper/sAndDistrict.json";
            var b = File.ReadAllText(a);
            stateDatas = JsonConvert.DeserializeObject<StateRoot>(b).states;
        }
    }


    public class  District
    {
       static string name;
        static int zipcode;

    }
    public class StateRoot
    {
        public List<StateData> states { get; set; }
    }

    public class StateData
    {
        public string state { get; set; }
        public List<string> districts { get; set; }
    }
}
