using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.pmprocessor
{
    public interface IAutoReplyObserver
    {
        string GetChannelTag();
        Task AutoReply(string channel_tag, long user_tg_id, string status_code, string? message);
    }
}
