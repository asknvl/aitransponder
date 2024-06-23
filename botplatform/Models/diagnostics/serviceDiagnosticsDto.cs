using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.diagnostics
{
    public class serviceDiagnosticsDto
    {        
        [JsonProperty]
        public string service_name { get; set; }
        [JsonProperty]
        public bool cheсk_result { get; set; } = true;
        [JsonProperty]
        public List<errorDto> errors { get; set; } = new();

        public void addError(string entity, string description)
        {
            errors.Add(new errorDto { entity = entity, description = description });
        }
    }

    public class errorDto
    {
        [JsonProperty]
        public string entity { get; set; }
        [JsonProperty]
        public string description { get; set; }
    }    
}
