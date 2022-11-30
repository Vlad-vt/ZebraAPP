using Newtonsoft.Json;
using System.Collections.Generic;

namespace ZebraAPP.Version
{
    public class VersionFile
    {
        [JsonProperty("AppName")]
        public string AppName { get; private set; }

        [JsonProperty("AppVersion")]
        public string AppVersion { get; private set; }

        [JsonProperty("Logs")]
        public List<string> Logs { get; private set; }
    }
}
