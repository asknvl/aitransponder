using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.pmprocessor
{
    public interface IPM
    {
        public string geotag { get; set; }
        public string phone_number { get; set; }
        public string bot_token { get; set; }

        //public Task start() { }
    }
}
