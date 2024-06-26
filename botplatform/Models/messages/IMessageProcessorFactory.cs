﻿using botplatform.Model.bot;
using botplatform.Models.pmprocessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace botplatform.Models.messages
{
    public interface IMessageProcessorFactory
    {
        MessageProcessorBase Get(BotType type, string geotag, string token, ITelegramBotClient bot);
        MessageProcessorBase Get(PostingType type, string geotag, string token, ITelegramBotClient bot);
    }
}
