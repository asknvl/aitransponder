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
        public int? first_msg_id { get; set; }
        public DateTime first_msg_rcvd_date { get; set; }
        public bool is_first_msg_rep { get; set; }
        public DateTime first_msg_rep_date { get; set; }
        public bool is_chat_deleted { get; set; }
        public DateTime chat_delete_date { get; set; }
        public bool was_autoreply { get; set; }
        public DateTime autoreply_date { get; set; }
        public int message_counter { get; set; }        
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

            first_msg_id = null;
            is_first_msg_rep = false;
            is_chat_deleted = false;
            was_autoreply = false;
        }
    }    
}
