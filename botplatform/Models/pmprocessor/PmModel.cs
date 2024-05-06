using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.pmprocessor
{
    public class PmModel
    {
        public string geotag { get; set; }
        public string phone_number { get; set; }
        public string bot_token { get; set; }
        public string api_id { get; set; }
        public string api_hash { get; set; }
        public PostingType posting_type { get; set; }
    }

    public enum PostingType
    {
        india_hack,
        india_strategy,
        latam_x,
        latam_t
    }
}
