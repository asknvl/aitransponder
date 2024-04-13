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
        public static List<PostingType> common_Available_Posting_Types { get; } = new List<PostingType>() { 
        
            PostingType.india_hack,
            PostingType.india_strategy,
            PostingType.latam_x,
            PostingType.latam_t
        
        };
    }
}
