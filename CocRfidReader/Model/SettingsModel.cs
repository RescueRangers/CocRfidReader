using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CocRfidReader.Models
{
    public class SettingsModel
    {
        [JsonPropertyName("readerIP")]
        public string? ReaderIP { get; set; }
        [JsonPropertyName("readerTimeOut")]
        public int? ReaderTimeOut { get; set; }
        [JsonPropertyName("readerReadTime")]
        public int? ReaderReadTime { get; set; }
        [JsonPropertyName("enabledAntennas")]
        public int? EnabledAntennas { get; set; }
        [JsonPropertyName("antena1Power")]
        public double? Antena1Power { get; set; }
        [JsonPropertyName("antena2Power")]
        public double? Antena2Power { get; set; }

        [JsonPropertyName("antena3Power")]
        public double? Antena3Power { get; set; }

        [JsonPropertyName("antena4Power")]
        public double? Antena4Power { get; set; }

        [JsonPropertyName("connectionString")]
        public string? ConnectionString { get; set; }

        [JsonPropertyName("rfMode")]
        public int? RfMode { get; set; }

        [JsonPropertyName("session")]
        public int? Session { get; set; }

        [JsonPropertyName("sendGridAPI")]
        public string? SendGridAPI { get; set; }

        [JsonPropertyName("notifyAddresses")]
        public List<string>? NotifyAddresses { get; set; }

        [JsonPropertyName("fromAddress")]
        public string? FromAddress { get; set; }
        
        [JsonPropertyName("rampNumber")]
        public int? RampNumber { get; set; }

        public double GetAntennaPower(int antena)
        {
            var prop = GetType().GetProperty($"Antena{antena}Power");
            if (prop == null) throw new PropertyNotFoundException($"Antena{antena}Power not found");
            return (double)prop.GetValue(this);
        }
    }
}
