using botplatform.Models.pmprocessor;
using Telegram.Bot;

namespace botplatform.Models.messages
{
    public interface IMessageProcessorFactory
    {        
        MessageProcessorBase Get(PMType type, string geotag, string token, ITelegramBotClient bot);
    }
}
