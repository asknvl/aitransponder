using asknvl.logger;
using botplatform.Models.messages.pmprocessor.india;
using botplatform.Models.pmprocessor;
using System;
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
                    return new MP_india_qual(geotag, token, bot);

                case PMType.support_pm:
                    return new MP_india_supp(geotag, token, bot);

                case PMType.latam_pm:
                    return new MP_latam(geotag, token, bot);

                case PMType.tier_1:
                    return new MP_tier1(geotag, token, bot);

                case PMType.latam_pm_v1:
                    return new MP_latam_v1(geotag, token, bot);

                default:
                    return null;
            }
        }
    }
}
