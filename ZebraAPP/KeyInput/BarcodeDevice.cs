using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace ZebraAPP.KeyInput
{
    class BarcodeDevice
    {
        [JsonIgnore]
        public string DeviceName { get; }
        [JsonProperty("barcode")]
        public string Barcode { get; set; }

        public BarcodeDevice(string DeviceName)
        {
            this.DeviceName = DeviceName;
            this.Barcode = "";
        }

        public void SendToNodeRed()
        {
            try
            {
                var httpRequest = (HttpWebRequest)WebRequest.Create("http://localhost:1880/sim/barcode");
                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                this.Barcode = "";
                httpRequest.Method = "POST";
                httpRequest.ContentType = "application/json";
                using (var requestStream = httpRequest.GetRequestStream())
                using (var writer = new StreamWriter(requestStream))
                {
                    writer.Write(json);
                }
                using (var httpResponse = httpRequest.GetResponse())
                using (var responseStream = httpResponse.GetResponseStream())
                using (var reader = new StreamReader(responseStream))
                {
                    string response = reader.ReadToEnd();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Node Red not connected");
            }
        }
    }
}
