using System;

namespace botplatform.Models.pmprocessor
{
    public class PmModel
    {
        public PMType posting_type { get; set; }
        public DateTime start_date { get; set; } 
        public string geotag { get; set; }
        public string phone_number { get; set; }
        public string bot_token { get; set; }
        public string api_id { get; set; }
        public string api_hash { get; set; }        
    }

    public enum PMType
    {
        qualification_pm,        
        support_pm
    }

}
