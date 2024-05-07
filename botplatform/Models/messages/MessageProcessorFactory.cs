using asknvl.logger;
using botplatform.Model.bot;
using botplatform.Models.messages.pmprocessor.india_hack;
using botplatform.Models.pmprocessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace botplatform.Models.messages
{
    public class MessageProcessorFactory : IMessageProcessorFactory
    {
        #region vars
        ILogger logger;
        #endregion

        public MessageProcessorFactory(ILogger logger) { 
            this.logger = logger;
        }

        public MessageProcessorBase Get(BotType type, string geotag, string token, ITelegramBotClient bot)
        {
            switch (type)
            {
                default:
                    return null;
            }
        }

        public MessageProcessorBase Get(PostingType type, string geotag, string token, ITelegramBotClient bot)
        {
            switch (type)
            {
                case PostingType.india_hack:
                    return new MP_india_strategy(geotag, token, bot);                      
                default:
                    return null;
            }
        }
    }
}
