using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.pmprocessor
{
    public interface IMessageObserver
    {
        string GetGeotag();
        Task SendMessage(string source, long tg_user_id, string response_code, string message);
    }
}
