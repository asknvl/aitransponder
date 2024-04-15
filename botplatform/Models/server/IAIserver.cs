using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.server
{
    public interface IAIserver
    {
        Task SendToAI(string geotag, long tg_user_id, string text);
    }
}
