using asknvl.server;
using botplatform.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace botplatform.Models.messages.pmprocessor.india
{
    internal class MP_latam_v1 : MessageProcessorBase
    {
        public override ObservableCollection<messageControlVM> MessageTypes { get; }

        public MP_latam_v1(string geotag, string token, ITelegramBotClient bot) : base(geotag, token, bot)
        {
            MessageTypes = new ObservableCollection<messageControlVM>()
            {               
                new messageControlVM(this)
                {
                    Code = $"CONCERN_ABOUT_COST",
                    Description = $"Дорого, нет денег"
                },
                new messageControlVM(this)
                {
                    Code = $"CANNOT_FIND_MONEY",
                    Description = $"Негде найти деньги"
                },
                new messageControlVM(this)
                {
                    Code = $"AM_I_A_BOT",
                    Description = $"Ты-бот"
                },
                new messageControlVM(this)
                {
                    Code = $"HOW_TO_PLAY",
                    Description = $"Как начать работать"
                },
                new messageControlVM(this)
                {
                    Code = $"ACCUSED_OF_FRAUD",
                    Description = $"Мошенник"
                },
                new messageControlVM(this)
                {
                    Code = $"ITS_TRUE",
                    Description = $"Правда ли это?"
                },
                new messageControlVM(this)
                {
                    Code = $"DREAM_ANSWER",
                    Description = $"Мечты"
                }                
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
