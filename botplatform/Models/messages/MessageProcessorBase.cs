using aksnvl.messaging;
using aksnvl.storage;
using asknvl.messaging;
using asknvl.storage;
using botplatform.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static asknvl.server.TGBotFollowersStatApi;

namespace botplatform.Models.messages
{
    public abstract class MessageProcessorBase : ViewModelBase, IMessageUpdater
    {
        #region vars
        protected Dictionary<string, StateMessage> messages = new();
        IStorage<Dictionary<string, StateMessage>> messageStorage;
        string geotag;
        string token;
        ITelegramBotClient bot;
        #endregion

        #region properties
        public abstract ObservableCollection<messageControlVM> MessageTypes { get; }
        #endregion

        public MessageProcessorBase(string geotag, string token, ITelegramBotClient bot)
        {
            this.geotag = geotag;
            this.bot = bot;          
            this.token = token;
        }

        #region private        
        #endregion

        #region public        
        public async void Add(string code, Message message, string pm)
        {
            if (MessageTypes == null)
                return;

            var found = MessageTypes.Any(t => t.Code.Equals(code));
            if (!found)
                return;

            var pattern = await StateMessage.Create(bot, message, geotag, token);
            AutoChange pm_autochange = new AutoChange()
            {
                OldText = "@booowos",
                NewText = pm
            };
            var autochanges = new List<AutoChange>() { pm_autochange };
            pattern.MakeAutochange(autochanges);
            pattern.Id = messages.Count();

            if (messages.ContainsKey(code))
                messages[code] = pattern;
            else
                messages.Add(code, pattern);

            messageStorage.save(messages);

            Debug.WriteLine($"{code}");
            MessageUpdatedEvent?.Invoke(code, true);
        }

        public void Init()
        {
            messageStorage = new Storage<Dictionary<string, StateMessage>>($"{geotag}.json", "messages", messages);
            messages = messageStorage.load();

            foreach (var item in messages)
            {
                MessageUpdatedEvent.Invoke(item.Key, true);
            }
        }

        public void Clear()
        {
            messages.Clear();
            messageStorage.save(messages);
        }

        public abstract StateMessage GetMessage(string status,
                                                string? link = null,
                                                string? pm = null,
                                                string? uuid = null,
                                                string? channel = null,
                                                bool? isnegative = false);
        public abstract StateMessage GetMessage(tgFollowerStatusResponse? resp,
                                                string? link = null,
                                                string? pm = null,
                                                string? channel = null,
                                                bool? isnegative = false);

        public abstract StateMessage GetChatJoinMessage();

        public abstract StateMessage GetPush(string? code,
                                             string? link = null,
                                             string? pm = null,
                                             string? uuid = null,
                                             string? channel = null,
                                             bool? isnegative = false);

        public abstract StateMessage GetPush(tgFollowerStatusResponse? resp,
                                            string? code,
                                            string? link = null,
                                            string? pm = null,                                            
                                            string? channel = null,
                                            bool? isnegative = false);

        public async Task UpdateMessageRequest(string code)
        {

            messages.Remove(code);
            messageStorage.save(messages);
            MessageUpdatedEvent?.Invoke(code, false);

            var found = MessageTypes.FirstOrDefault(m => m.Code.Equals(code));
            if (found != null)
            {
                await Task.Run(() =>
                {
                    UpdateMessageRequestEvent?.Invoke(found.Code, found.Description);
                });
            }
        }

        public async Task ShowMessageRequest(string code)
        {
            if (messages.ContainsKey(code))
            {
                await Task.Run(() =>
                {
                    ShowMessageRequestEvent?.Invoke(messages[code], code);
                });
            }            
        }
        #endregion

        #region callbacks
        public event Action<string, string> UpdateMessageRequestEvent;
        public event Action<StateMessage, string> ShowMessageRequestEvent;
        public event Action<string, bool> MessageUpdatedEvent;
        #endregion
    }
}
