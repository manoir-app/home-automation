using Newtonsoft.Json;

namespace Home.Agents.Sarah.Devices.Shelly
{

    public class ShellyInfoDataObject
    {
        protected ShellyInfoDataObject()
        {

        }

        private class TempClass
        {
            public int? gen { get; set; }

        }

        public static ShellyInfoDataObject FromJson(string json)
        {
            return FromJson(json, out int version);
        }

        public static ShellyInfoDataObject FromJson(string json, out int version)
        {
            version = 1;
            var tmp = JsonConvert.DeserializeObject<TempClass>(json);
            if (tmp != null && tmp.gen.HasValue)
            {
                switch (tmp.gen.Value)
                {
                    default:
                        version = tmp.gen.Value;
                        return JsonConvert.DeserializeObject<ShellyInfoDataObjectGen2>(json);
                }
            }
            else
            {
                version = 1;
                return JsonConvert.DeserializeObject<ShellyInfoDataObjectGen1>(json);
            }
        }

    }


    public class ShellyInfoDataObjectGen1 : ShellyInfoDataObject
    {
        public string type { get; set; }
        public string mac { get; set; }
        public bool auth { get; set; }
        public string fw { get; set; }
        public int longid { get; set; }
        public bool sleep_mode { get; set; }
    }


    public class ShellyInfoDataObjectGen2 : ShellyInfoDataObject
    {
        public string name { get; set; }
        public string id { get; set; }
        public string mac { get; set; }
        public string model { get; set; }
        public int gen { get; set; }
        public string fw_id { get; set; }
        public string ver { get; set; }
        public string app { get; set; }
        public bool auth_en { get; set; }
        public object auth_domain { get; set; }
    }


}
