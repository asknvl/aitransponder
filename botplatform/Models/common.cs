using botplatform.Models.pmprocessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models
{
    public static class common
    {
        public static List<PMType> common_Available_Posting_Types { get; } = new List<PMType>() { 
        
            PMType.qualification_pm, //0
            PMType.support_pm,
            PMType.latam_pm,
            PMType.tier_1,
            PMType.latam_pm_v1,
            PMType.tier_1_events,
            PMType.inda_nonstop //6
       };
    }
}
