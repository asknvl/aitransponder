using asknvl.logger;
using botplatform.Models.pmprocessor.db_storage;
using botplatform.Models.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace botplatform.Models.pmprocessor
{
    public class pm_processor_latam_v1 : PMBase, IAutoReplyObserver
    {
        public pm_processor_latam_v1(PmModel model, IPMStorage pmStorage, IDBStorage dbStorage, ILogger logger) : base(model, pmStorage, dbStorage, logger)
        {
        }

        public override async Task processBusiness(Update update)
        {
            try
            {

            } catch (Exception ex)
            {

            }
        }

        public Task AutoReply(string channel_tag, long user_tg_id, string status_code, string? message)
        {
            throw new NotImplementedException();
        }

        public string GetChannelTag()
        {
            return geotag;
        }
    }
}
