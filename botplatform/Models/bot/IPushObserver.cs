using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.bot
{
    public interface IPushObserver
    {
        string GetGeotag();
        Task<bool> Push(long tg_id, string code, int notification_id);
    }
}
