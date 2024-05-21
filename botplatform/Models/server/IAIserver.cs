using botplatform.Models.pmprocessor.message_queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace botplatform.Models.server
{
    public interface IAIserver
    {
        Task SendMessageToAI(string geotag, long tg_user_id, string text);
        Task SendHistoryToAI(string geotag, long tg_user_id, string fn, string ln, string un, List<HistoryItem> messages);
        Task SendToAI(string geotag, long tg_user_id, string fn, string ln, string un, string? message = null, string? base64_image = null);
    }

    public class universalMessageDto
    {
                
        public List<messageDto> messages = new List<messageDto>();
        public long tg_user_id { get; set; }
        public string source { get; set; }
        public string? firstname { get; set; }
        public string? lastname { get; set; }
        public string? username { get; set; }
        public bool is_test { get; set; }

        public universalMessageDto(string source, long tg_user_id,  string? fn, string? ln, string? un, string? text = null, string? base64_image = null, bool? is_test = null) {

            this.source = source;
            this.tg_user_id = tg_user_id;            

            firstname = fn;
            lastname = ln;
            username = un;

            if (is_test != null)
                this.is_test = (bool)is_test;

            messages.Add(new messageDto(text, base64_image));
        }
    }

    public class messageDto
    {
        public string role { get; set; } = "user";
        public List<Object> content { get; set; } = new();

        public messageDto(string? text, string? base64_image)
        {
            if (text != null)
            {
                content.Add(new textDto(text));
            }

            if (base64_image != null)
            {
                content.Add(new imageDto(base64_image));
            }
        }
    }

    public class textDto
    {
        public string type { get; set; } = "text";
        public string text { get; set; }    
        public textDto(string text)
        {
            this.text = text;   
        }
    }

    public class imageDto
    {
        public string type { get; set; } = "image_url";
        public urlDto image_url { get; set; }
        public imageDto(string base64_image)
        {
            image_url = new urlDto(base64_image);
        }
    }

    public class urlDto
    {
        public string url { get; set; }
        public urlDto(string base64_image)
        {
            url = $"data:image/jpeg;base64,{base64_image}";
        }
    }
}
