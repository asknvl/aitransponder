using System;

namespace botplatform.Models.pmprocessor
{
    public class PmModel
    {
        public PMType posting_type { get; set; }
        public DateTime start_date { get; set; } = new DateTime(2024, 05, 1, 9, 42, 0);
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
