using asknvl.logger;
using botplatform.Model.bot;
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
    }
}
