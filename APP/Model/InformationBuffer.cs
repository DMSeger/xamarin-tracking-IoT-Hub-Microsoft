using System.Collections.Generic;

using Newtonsoft.Json;

namespace CML.Model
{
    public class InformationBuffer
    {
        [JsonProperty(PropertyName ="buffer")]
        public List<DeviceInformation> Buffer { get; set; }

        public InformationBuffer()
        {
            this.Buffer = new List<DeviceInformation>();
        }
    }
}
