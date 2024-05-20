using botplatform.Models.pmprocessor.message_queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.server
{
    public interface IAIserver
    {
        Task SendMessageToAI(string geotag, long tg_user_id, string text);
        Task SendHistoryToAI(string geotag, long tg_user_id, string fn, string ln, string un, List<HistoryItem> messages);

        Task SendToAI(string geotag, long tg_user_id, string fn, string ln, string un);
    }

    public class messageDto
    {
        public string role { get; set; } = "user";
        public List<Object> content { get; set; } = new();
        public long tg_user_id { get; set; }
        public string sorce { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string username { get; set; }
        public bool is_test { get; set; }

        public messageDto(string? text = null, string? base64_image = null) { 
        }
    }

    public class textDto
    {
        public string type { get; set; } = "text";
        public string text { get; set; }    
    }

    public class imageDto
    {
        public string type { get; set; } = "image_url";
        public urlDto url { get; set; }
    }

    public class urlDto
    {
        public string url { get; set; }
    }

    


}
