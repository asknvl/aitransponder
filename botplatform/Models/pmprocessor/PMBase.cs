using asknvl;
using asknvl.logger;
using asknvl.server;
using Avalonia.Controls;
using botplatform.Model.bot;
using botplatform.Models.messages;
using botplatform.Models.pmprocessor.message_queue;
using botplatform.Models.pmprocessor.quote_rocessor;
using botplatform.Models.pmprocessor.userapi;
using botplatform.Models.server;
using botplatform.Models.settings;
using botplatform.Models.storage;
using botplatform.Operators;
using botplatform.ViewModels;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using TL;
using static System.Net.Mime.MediaTypeNames;

namespace botplatform.Models.pmprocessor
{
    public abstract class PMBase : ViewModelBase, IPM, IMessageObserver
    {
        #region vars
        protected ILogger logger;        
        protected IPMStorage pmStorage;
        protected IMessageProcessorFactory messageProcessorFactory;

        protected ITelegramBotClient bot;
        protected ITGUser user;
        protected IMarkRead marker;

        protected CancellationTokenSource cts;
        protected State state = State.free;

        List<int> ignoredMesageIds = new();

        object lockObj = new object();
        List<userInfo> activeUsers = new();

        System.Timers.Timer aggregateMessageTimer;

        MessageQueue pmMessages;

        MessageHistory history;

        PmModel tmpPmModel;     
        Dictionary<long, string> bcIds = new Dictionary<long, string>();
        Dictionary<long, bool> usersStatuses = new Dictionary<long, bool>();

        QuoteProcessor quoteProcessor = new QuoteProcessor();


        IAIserver ai;
        ITGBotFollowersStatApi server;
        #endregion

        #region properties
        string _geotag;
        public string geotag {
            get => _geotag;
            set => this.RaiseAndSetIfChanged(ref _geotag, value);   
        }

        string _phone_number;        
        public string phone_number { 
            get => _phone_number;
            set => this.RaiseAndSetIfChanged(ref _phone_number, value);
        }

        string _bot_token;        
        public string bot_token { 
            get => _bot_token;
            set => this.RaiseAndSetIfChanged(ref _bot_token, value);    
        }

        bool _need_verification;
        public bool need_verification
        {
            get => _need_verification;
            set => this.RaiseAndSetIfChanged(ref _need_verification, value);    
        }

        string _verify_code;
        public string verify_code
        {
            get => _verify_code;
            set => this.RaiseAndSetIfChanged(ref _verify_code, value);
        }

        public List<PostingType> posting_types { get; } = common.common_Available_Posting_Types;

        PostingType _posting_type;
        public PostingType posting_type
        {
            get => _posting_type;
            set => this.RaiseAndSetIfChanged(ref _posting_type, value);
        }

        string _bot_username;
        public string bot_username
        {
            get => _bot_username;
            set => this.RaiseAndSetIfChanged(ref _bot_username, value);
        }

        bool _is_editable;
        public bool is_editable
        {
            get => _is_editable;
            set => this.RaiseAndSetIfChanged(ref _is_editable, value);
        }

        bool _is_active = false;
        public bool is_active
        {
            get => _is_active;
            set
            {
                is_editable = false;
                this.RaiseAndSetIfChanged(ref _is_active, value);
            }
        }

        string awaitedMessageCode;
        public string AwaitedMessageCode
        {
            get => awaitedMessageCode;
            set => this.RaiseAndSetIfChanged(ref awaitedMessageCode, value);
        }

        MessageProcessorBase messageProcessor;
        public MessageProcessorBase MessageProcessor
        {
            get => messageProcessor;
            set => this.RaiseAndSetIfChanged(ref messageProcessor, value);
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> startCmd { get; }
        public ReactiveCommand<Unit, Unit> stopCmd { get; }
        public ReactiveCommand<Unit, Unit> editCmd { get; }
        public ReactiveCommand<Unit, Unit> cancelCmd { get; }
        public ReactiveCommand<Unit, Unit> saveCmd { get; }
        public ReactiveCommand<Unit, Unit> verifyCmd { get; }
        #endregion

        public PMBase(PmModel model, IPMStorage pmStorage, ILogger logger)
        {

            this.logger = logger;            
            this.pmStorage = pmStorage;

            geotag = model.geotag;
            phone_number = model.phone_number;
            bot_token = model.bot_token;
            posting_type = model.posting_type;

            messageProcessorFactory = new MessageProcessorFactory(logger);

            pmMessages = new MessageQueue();
            history = new MessageHistory();

            ai = new AIServer("https://gpt.raceup.io");
            server = new TGBotFollowersStatApi("https://ru.flopasda.site");

            aggregateMessageTimer = new System.Timers.Timer();
            aggregateMessageTimer.Interval = 20 * 1000;
            aggregateMessageTimer.AutoReset = true;
            aggregateMessageTimer.Elapsed += AggregateMessageTimer_Elapsed;

            user = new user_v0(model.api_id, model.api_hash, phone_number, "5555", logger);
            user.VerificationCodeRequestEvent += User_VerificationCodeRequestEvent;
            user.StatusChangedEvent += User_StatusChangedEvent;

            marker = (IMarkRead)user;

            #region commands
            startCmd = ReactiveCommand.CreateFromTask(async () => {
                await Start();
            });
            stopCmd = ReactiveCommand.Create(() => {
                Stop();
            });
            editCmd = ReactiveCommand.Create(() => {

                tmpPmModel = new PmModel()
                {
                    geotag = geotag,
                    phone_number = phone_number,
                    bot_token = bot_token,
                    posting_type = posting_type,

                };

                is_editable = true;            
            });    
            cancelCmd = ReactiveCommand.Create(() => {
                geotag = tmpPmModel.geotag;
                bot_token = tmpPmModel.bot_token;
                phone_number = tmpPmModel.phone_number;
                posting_type = tmpPmModel.posting_type;
                is_editable = false;
            });  
            saveCmd = ReactiveCommand.Create(() => {
                var model = new PmModel()
                {
                    geotag = geotag,
                    phone_number = phone_number,
                    bot_token = bot_token,
                    posting_type = posting_type
                };
                pmStorage.Update(tmpPmModel.geotag, model);
                is_editable = false;
            });
            verifyCmd = ReactiveCommand.Create(() => {
                user?.SetVerifyCode(verify_code);
            });
            #endregion

        }

        #region helpers
        void initMessageProcessor()
        {
            MessageProcessor = messageProcessorFactory.Get(posting_type, geotag, bot_token, bot);

            if (MessageProcessor != null)
            {
                MessageProcessor.UpdateMessageRequestEvent += async (code, description) =>
                {
                    AwaitedMessageCode = code;
                    state = State.waiting_new_message;
                    
                    var operator_tg = Settings.getInstance().operator_tg;

                    try
                    {
                        await bot.SendTextMessageAsync(operator_tg, $"Перешлите сообщение для: \n{description.ToLower()}");
                    }
                    catch (Exception ex)
                    {
                        logger.err("BOT", $"UpdateMessageRequestEvent: {ex.Message}");
                    }

                };

                MessageProcessor.ShowMessageRequestEvent += async (message, code) =>
                {                    
                    var operator_tg = Settings.getInstance().operator_tg;

                    try
                    {
                        int id = await message.Send(operator_tg, bot);
                    }
                    catch (Exception ex)
                    {
                        logger.err(geotag, $"ShowMessageRequestEvent: {ex.Message}");
                    }
                };

                MessageProcessor.Init();
            }
        }

        async Task processOperator(Telegram.Bot.Types.Message message)
        {
            try 
            { 
                if (state == State.waiting_new_message)
                {
                    MessageProcessor.Add(AwaitedMessageCode, message);
                    state = State.free;
                    return;
                }
            } catch (Exception ex)
            {
                logger.err(geotag, ex.Message);
            }
        }

        async Task processBusiness(Telegram.Bot.Types.Update update)
        {
            try
            {
                var chat = update.BusinessMessage.From.Id;
                var fn = update.BusinessMessage.From.FirstName;
                var ln = update.BusinessMessage.From.LastName;
                var un = update.BusinessMessage.From.Username;

                if (!usersStatuses.ContainsKey(chat))
                {

                    bool needProcess = true;
                    var userData = await server.GetFollowerSubscriprion(geotag, chat);

                    if (userData.Count != 0)
                    {
                        var sdate = userData[0].subscribe_date;
                        var date = DateTime.Parse(sdate);
                        var startDate = new DateTime(2024, 05, 1, 9, 42, 0);

                        needProcess = date > startDate;

#if DEBUG_TG_SERV
                        needProcess = true;
#endif

                        logger.inf(geotag, $"{chat} {fn} {ln} {un} {sdate} | {date} {needProcess}");
                    }

                    usersStatuses.Add(chat, needProcess);
                }


                if (!usersStatuses[chat])
                    return;                
                

                if (!activeUsers.Any(u => u.tg_user_id == chat))
                {

                    if (activeUsers.Count == 1024)
                    {
                        activeUsers.RemoveAt(0);
                    }

                    activeUsers.Add(new userInfo()
                    {
                        tg_user_id = chat,
                        fn = fn,
                        ln = ln,
                        un = un
                    });
                }


                logger.inf(geotag, $"{fn} {ln} {un} {chat}>{update.BusinessMessage.Text}");

                var bc = await bot.GetBusinessConnectionAsync(new GetBusinessConnectionRequest(update.BusinessMessage.BusinessConnectionId));
                
                
                if (chat != bc.User.Id)
                {
                    if (!bcIds.ContainsKey(chat))
                        bcIds.Add(chat, update.BusinessMessage.BusinessConnectionId);
                                        
                    var text = update.BusinessMessage.Text;

                    if (text != null)
                    {
                        pmMessages.Add(chat, text);
                        history.Add(MessageFrom.Lead, chat, text);
                    }


                    //logger.inf(geotag, $"{fn} {ln} {un} {chat}>{text}");
                }

            } catch (Exception ex)
            {
                logger.err(geotag, ex.Message);
            }
        }
        #endregion

        #region handlers
        private void User_VerificationCodeRequestEvent(ITGUser usr)
        {
            need_verification = true;
        }

        private void User_StatusChangedEvent(ITGUser usr, DropStatus status)
        {
            if (status == DropStatus.active)
            {
                need_verification = false;
            }
        }

        private async void AggregateMessageTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {

            if (!is_active)
                return;

            try
            {
                //var toSent = pmMessages.GetMessages();

                //foreach (var message in toSent)
                //{
                //    try
                //    {
                //        await ai.SendMessageToAI(geotag, message.Key, message.Value);                       

                //        logger.dbg(geotag, $"aggregate: {message.Key} {message.Value}");

                //    } catch (Exception ex)
                //    {
                //        logger.err(geotag, $"Aggregate: {message.Key} {message.Value}");
                //    }
                //}

                var toSent = history.Get();
                logger.dbg(geotag, $"toSent: count={toSent.Count}");
                foreach ( var item in toSent )
                {
                    try
                    {

                        string? fn = null;
                        string? ln = null;
                        string? un = null;

                        var user = activeUsers.FirstOrDefault(u => u.tg_user_id == item.Key);
                        if (user != null)
                        {
                            fn = user.fn;
                            ln = user.ln;
                            un = user.un;
                        }

                        logger.dbg(geotag, $"{item.Key} {fn} {ln} {un}");

                        string s = "";
                        var histItems = item.Value.Get();

                        foreach (var h in histItems)  {
                            s += $"{h.role} > {h.content}\n";
                        }
                        logger.dbg(geotag, $"{s}");


                        try
                        {
                            await ai.SendHistoryToAI(geotag, item.Key, fn, ln, un, histItems);
                        } catch (Exception ex)
                        {
                            logger.err(geotag, $"AggregateMessageTimer: {ex.Message}");
                        }

                        await marker?.MarkAsRead(item.Key);
                    } catch (Exception ex)
                    {

                    }
                }


            } catch (Exception ex)
            {

            }
        }

        async Task HandleUpdateAsync(ITelegramBotClient bot, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
        {
            if (update == null)
                return;

            if (ignoredMesageIds.Contains(update.Id))
                return;

            long chat;

            try
            {
                switch (update.Type)
                {
                    case UpdateType.BusinessMessage:
                        if (update.BusinessMessage != null)
                        {
                            await processBusiness(update);  
                        }
                        break;

                    case UpdateType.Message:                         
                        if (update.Message != null)
                        {
                            chat = update.Message.From.Id;
                            if (chat == Settings.getInstance().operator_tg)
                                await processOperator(update.Message);
                        }
                        break;
                }

            } catch (Exception ex)
            {
                logger.err(geotag, ex.Message);
            }
        }

        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"{geotag} Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            logger.err(geotag, ErrorMessage);
            return Task.CompletedTask;
        }
        #endregion

        #region helpers
        async Task sendTextMessage(long tg_user_id, string message)
        {
            int delay = (int)(message.Length * 0.1 * 1000);
            int typings = delay / 5000;

            if (typings == 0)
                typings = 1;

            var bcid = bcIds[tg_user_id];
            for (int i = 0; i < typings; i++)
            {
                await bot.SendChatActionAsync(tg_user_id, ChatAction.Typing, businessConnectionId: bcid);
                await Task.Delay(5000);
            }
            await bot.SendTextMessageAsync(tg_user_id, message, businessConnectionId: bcid);

            var msg_to_ai = $"{message}";
            history.Add(MessageFrom.PM, tg_user_id, msg_to_ai);
            logger.inf_urgent(geotag, $"{tg_user_id}>{message}");
        }

        async Task sendStatusMessage(long tg_user_id, string response_code, string message)
        {
            try
            {
                var m = MessageProcessor.GetMessage(response_code);
                if (m != null)
                {
                    var bcid = bcIds[tg_user_id];

                    var exists_id = quoteProcessor.Get(tg_user_id, response_code);
                    if (exists_id != -1)
                    {
                        await m.Send(tg_user_id, bot, bcid: bcid, reply_message_id: exists_id);

                    } else
                    {
                        int id = await m.Send(tg_user_id, bot, bcid: bcid);
                        quoteProcessor.Add(tg_user_id, response_code, id);
                    }                    
                } 
            } catch (Exception ex)
            {
                logger.err(geotag, $"sendStatusMessage: {ex.Message}");
            }
        }
        #endregion

        #region public
        public virtual async Task Start()
        {
            if (is_active)
            {
                logger.err(geotag, $"PM already running");
                return;
            }

#if DEBUG
            //bot = new TelegramBotClient(new TelegramBotClientOptions(bot_token, "http://localhost:8081/bot/"));  
            bot = new TelegramBotClient(bot_token);
#elif DEBUG_TG_SERV
            bot = new TelegramBotClient(bot_token);
#else
            //bot = new TelegramBotClient(new TelegramBotClientOptions(bot_token, "http://localhost:8081/bot/"));
            bot = new TelegramBotClient(bot_token);
#endif

            var u = await bot.GetMeAsync();
            bot_username = u.Username;

            var updates = await bot.GetUpdatesAsync();
            ignoredMesageIds = updates.Select(u => u.Id).ToList();


            cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[] { UpdateType.Message, UpdateType.BusinessConnection, UpdateType.BusinessMessage }
            };

            initMessageProcessor();

            aggregateMessageTimer.Start();

            bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cts.Token);

            await user.Start();

            is_active = true;

            logger.inf(geotag, $"Starting PM, posting={posting_type}");
            logger.inf(geotag, $"PM started");
        }

        public void Stop()
        {
            cts?.Cancel();
            aggregateMessageTimer.Stop();
            is_active = false;
            logger.inf(geotag, "PM stopped");
        }

        public string GetGeotag()
        {
            return geotag;
        }

        public async Task SendMessage(string source, long tg_user_id, string response_code, string message)
        {
            try
            {

                if (!string.IsNullOrEmpty(message) || !string.IsNullOrEmpty(response_code))
                {

                    var _ = Task.Run(async () => {


                        if (!response_code.Equals("UNKNOWN") && geotag.Equals("INDAH17"))
                        {
                            var m = MessageProcessor.GetMessage(response_code);
                            if (m != null)
                            {
                                await sendStatusMessage(tg_user_id, response_code, message);
                            } else
                            {
                                await sendTextMessage(tg_user_id, message);
                            }

                        } else
                        {
                            await sendTextMessage(tg_user_id, message);
                        }
                    });

                    

                    //var msg_to_ai = $"Response Code: [{response_code}] Response:{message}";
                  
                }

            } catch (Exception ex)
            {
                logger.err(geotag, $"SendMessage: {ex.Message}");
            }
        }
        #endregion
    }
    public class userInfo
    {
        public long tg_user_id { get; set; }
        public string? fn { get; set;}
        public string? ln { get; set; }
        public string? un { get; set; }
    }
}
