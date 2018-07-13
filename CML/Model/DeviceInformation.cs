using System;

using Newtonsoft.Json;

namespace CML.Model
{
    public class DeviceInformation
    {
        [JsonProperty(PropertyName = "end_trip")]
        public bool EndTrip { get; set; }

        [JsonProperty (PropertyName ="longitude")]
        public double Longitude { get; set; }

        [JsonProperty(PropertyName = "latitude")]
        public double Latitude { get; set; }

        [JsonProperty(PropertyName = "speed")]
        public double Speed { get; set; }

        [JsonProperty(PropertyName = "battery")]
        public int Battery { get; set; }

        [JsonProperty(PropertyName = "device_id")]
        public string DeviceId { get; set; }

        [JsonProperty(PropertyName = "aquisition_date")]
        public DateTime AquisitionDate { get; set; }

        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }
    }
}
