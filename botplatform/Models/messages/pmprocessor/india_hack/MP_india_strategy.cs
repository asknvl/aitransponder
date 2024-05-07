using asknvl.server;
using botplatform.ViewModels;
using DynamicData;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace botplatform.Models.messages.pmprocessor.india_hack
{
    public class MP_india_strategy : MessageProcessorBase
    {
        public override ObservableCollection<messageControlVM> MessageTypes { get; }

        public MP_india_strategy(string geotag, string token, ITelegramBotClient bot) : base(geotag, token, bot)
        {
            MessageTypes = new ObservableCollection<messageControlVM>()
            {
                new messageControlVM(this)
                {
                    Code = "FIND_ACCOUNT_ID",
                    Description = "ID аккаунта"
                },
                new messageControlVM(this)
                {
                    Code = "LOG_OUT",
                    Description = "Выйти из аккаунта"
                },
                new messageControlVM(this)
                {
                    Code = "AM_I_A_BOT",
                    Description = "Я бот?"
                },
                new messageControlVM(this)
                {
                    Code = "ACCUSED_OF_FRAUD",
                    Description = "Ты мошенник"
                },
                new messageControlVM(this)
                {
                    Code = "CONCERN_ABOUT_COST",
                    Description = "1000 рупий-дорого"
                },
                new messageControlVM(this)
                {
                    Code = "CANNOT_FIND_MONEY",
                    Description = "Негде найти деньги"
                },
            };
        }

        public override StateMessage GetChatJoinMessage()
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetMessage(string status, string? link = null, string? pm = null, string? uuid = null, string? channel = null, bool? isnegative = false)
        {
            StateMessage msg = null;

            string code = status;
            InlineKeyboardMarkup markUp = null;

            if (messages.ContainsKey(code))
            {
                msg = messages[code];//.Clone();
                msg.Message.ReplyMarkup = markUp;
            }
            else
            {
                var found = MessageTypes.FirstOrDefault(m => m.Code.Equals(code));
                if (found != null)
                    found.IsSet = false;

            }

            return msg;
        }

        public override StateMessage GetMessage(TGBotFollowersStatApi.tgFollowerStatusResponse? resp, string? link = null, string? pm = null, string? channel = null, bool? isnegative = false)
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetPush(string? code, string? link = null, string? pm = null, string? uuid = null, string? channel = null, bool? isnegative = false)
        {
            throw new NotImplementedException();
        }

        public override StateMessage GetPush(TGBotFollowersStatApi.tgFollowerStatusResponse? resp, string? code, string? link = null, string? pm = null, string? channel = null, bool? isnegative = false)
        {
            throw new NotImplementedException();
        }
    }
}
