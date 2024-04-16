using asknvl.logger;
using Avalonia.Controls;
using botplatform.Model.bot;
using botplatform.Models.messages;
using botplatform.Models.pmprocessor.message_queue;
using botplatform.Models.server;
using botplatform.Models.settings;
using botplatform.Models.storage;
using botplatform.Operators;
using botplatform.ViewModels;
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

namespace botplatform.Models.pmprocessor
{
    public abstract class PMBase : ViewModelBase, IPM, IMessageObserver
    {
        #region vars
        protected ILogger logger;        
        protected IPMStorage pmStorage;
        protected IMessageProcessorFactory messageProcessorFactory;
        protected ITelegramBotClient bot;
        protected CancellationTokenSource cts;
        protected State state = State.free;
        List<int> ignoredMesageIds = new();
        System.Timers.Timer aggregateMessageTimer;
        MessageQueue pmMessages;
        PmModel tmpPmModel;     
        Dictionary<long, string> bcIds = new Dictionary<long, string>();
        IAIserver ai;
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

            ai = new AIServer("https://gpt.raceup.io");

            aggregateMessageTimer = new System.Timers.Timer();
            aggregateMessageTimer.Interval = 20 * 1000;
            aggregateMessageTimer.AutoReset = true;
            aggregateMessageTimer.Elapsed += AggregateMessageTimer_Elapsed;


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

        async Task processOperator(Message message)
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

        async Task processBusiness(Update update)
        {
            try
            {
                var chat = update.BusinessMessage.From.Id;
                var fn = update.BusinessMessage.From.FirstName;
                var ln = update.BusinessMessage.From.LastName;
                var un = update.BusinessMessage.From.Username;

                var bc = await bot.GetBusinessConnectionAsync(new GetBusinessConnectionRequest(update.BusinessMessage.BusinessConnectionId));

                
                if (chat != bc.User.Id)
                {
                    if (!bcIds.ContainsKey(chat))
                        bcIds.Add(chat, update.BusinessMessage.BusinessConnectionId);

                    var text = update.BusinessMessage.Text;
                    pmMessages.Add(chat, text);

                    logger.inf(geotag, $"{fn} {ln} {un} {chat}>{text}");
                }

            } catch (Exception ex)
            {
                logger.err(geotag, ex.Message);
            }
        }
        #endregion

        #region handlers
        private async void AggregateMessageTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                var toSent = pmMessages.GetMessages();

                foreach (var message in toSent)
                {
                    try
                    {
                        await ai.SendToAI(geotag, message.Key, message.Value);
                        logger.dbg(geotag, $"aggregate: {message.Key} {message.Value}");

                    } catch (Exception ex)
                    {
                        logger.err(geotag, $"Aggregate: {message.Key} {message.Value}");
                    }
                }

            } catch (Exception ex)
            {

            }
        }

        async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
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

        #region public
        public virtual async Task Start()
        {
            if (is_active)
            {
                logger.err(geotag, $"PM already running");
                return;
            }

#if DEBUG
            bot = new TelegramBotClient(new TelegramBotClientOptions(bot_token, "http://localhost:8081/bot/"));  
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

                if (!string.IsNullOrEmpty(message))
                {
                    var bcid = bcIds[tg_user_id];
                    await bot.SendTextMessageAsync(tg_user_id, message, businessConnectionId: bcid);
                    logger.inf_urgent(geotag, $"{tg_user_id}>{message}");
                }

            } catch (Exception ex)
            {
                logger.err(geotag, $"SendMessage: {ex.Message}");
            }
        }
        #endregion
    }
}
