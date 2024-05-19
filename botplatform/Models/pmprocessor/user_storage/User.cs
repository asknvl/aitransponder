using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.pmprocessor.db_storage
{
    public class User
    {
        [Key]
        public int id { get; set; }
        public string geotag { get; set; }
        public long tg_id { get; set; }
        public string? bcId { get; set; }
        public string? fn { get; set; }
        public string? ln { get; set; }
        public string? un { get; set; }        
        public DateTime created_date { get; set; }
        public bool ai_on { get; set; }
        public DateTime ai_off_time { get; set; }
        public string? ai_off_code { get; set; }        
        public User(string geotag, long tg_id, string bcId, string? fn = null, string? ln = null, string? un = null)
        {
            this.geotag = geotag;
            this.tg_id = tg_id;
            this.bcId = bcId;
            this.fn = fn;
            this.ln = ln;
            this.un = un;
            
            created_date = DateTime.UtcNow;
            ai_on = true;
            this.bcId = bcId;
        }
    }    
}
