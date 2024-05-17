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
        
            PMType.qualification_pm,
            PMType.support_pm
        };
    }
}
