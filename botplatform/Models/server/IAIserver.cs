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
    }
}
