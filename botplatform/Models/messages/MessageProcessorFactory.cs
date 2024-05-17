﻿using asknvl.logger;
using botplatform.Models.messages.pmprocessor.india_hack;
using botplatform.Models.pmprocessor;
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

        public MessageProcessorBase Get(PMType type, string geotag, string token, ITelegramBotClient bot)
        {
            switch (type)
            {
                case PMType.qualification_pm:
                    return new MP_india_strategy(geotag, token, bot);                      
                default:
                    return null;
            }
        }
    }
}
