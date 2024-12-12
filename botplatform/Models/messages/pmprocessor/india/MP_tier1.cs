using asknvl.messaging;
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
    public class MP_tier1 : MessageProcessorBase
    {
        public override ObservableCollection<messageControlVM> MessageTypes { get; }    

        public MP_tier1(string geotag, string token, ITelegramBotClient bot) : base(geotag, token, bot)
        {
            MessageTypes = new ObservableCollection<messageControlVM>()
            {
                new messageControlVM(this)
                {
                    Code = "LINK",
                    Description = "Сообщение с ссылкой"
                },                
                new messageControlVM(this)
                {
                    Code = $"PUSH_3H",
                    Description = $"Пуш 3 часа"
                },
                new messageControlVM(this)
                {
                    Code = $"PUSH_6H",
                    Description = $"Пуш 6 часов"
                },
                new messageControlVM(this)
                {
                    Code = $"PUSH_24H",
                    Description = $"Пуш 24 часа"
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

                if (code.Equals("LINK"))
                {
                    if (!string.IsNullOrEmpty(link))
                    {
                        List<AutoChange> autoChange = new List<AutoChange>()
                        {
                            new AutoChange() {
                                OldText = "https://partner.chng/",
                                NewText = $"{link}"
                            }
                        };

                        var _msg = msg.Clone();
                        _msg.MakeAutochange(autoChange);
                        _msg.Message.ReplyMarkup = markUp;
                        return _msg;
                    }
                }

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
