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
        public string api_id { get; set; } = "23400467";
        public string api_hash { get; set; } = "af8d86630f308931e5bcbdf045724f7c";
        public bool is_ai_enabled { get; set; }
    }

    public enum PMType
    {
        qualification_pm,        
        support_pm,
        latam_pm,
        tier_1,
        latam_pm_v1,
        tier_1_events,
        inda_nonstop
    }

}
