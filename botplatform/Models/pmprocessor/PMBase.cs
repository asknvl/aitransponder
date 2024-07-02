using asknvl;
using asknvl.logger;
using asknvl.server;
using botplatform.Models.messages;
using botplatform.Models.pmprocessor.message_queue;
using botplatform.Models.pmprocessor.quote_rocessor;
using botplatform.Models.pmprocessor.db_storage;
using botplatform.Models.pmprocessor.userapi;
using botplatform.Models.server;
using botplatform.Models.settings;
using botplatform.Models.storage;
using botplatform.ViewModels;
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
using System.IO;
using SkiaSharp;
using botplatform.Models.diagnostics;

namespace botplatform.Models.pmprocessor
{
    public abstract class PMBase : ViewModelBase, IPM, IMessageObserver, IDiagnosticsResulter
    {
        #region vars
        PmModel model;

        protected ILogger logger;

        protected IPMStorage pmStorage;
        protected IDBStorage dbStorage;

        protected IMessageProcessorFactory messageProcessorFactory;

        protected ITelegramBotClient bot;
        protected ITGUser? user = null;
        protected IMarkRead marker;

        State state;

        protected CancellationTokenSource cts;

        List<int> ignoredMesageIds = new();
        
        PmModel tmpPmModel;

        QuoteProcessor quoteProcessor = new QuoteProcessor();

        IAIserver ai;
        protected ITGBotFollowersStatApi server;

        DateTime startDate;
        Random random = new Random();

        protected errorCollector errorCollector = new();

        protected System.Timers.Timer businessUpdatesCheckTimer;
        uint businessUpdatesCounter = 0;
        uint businessUpdatesCounter_prev = 0;
        #endregion

        #region properties
        string _geotag;
        public string geotag
        {
            get => _geotag;
            set => this.RaiseAndSetIfChanged(ref _geotag, value);
        }

        string _phone_number;
        public string phone_number
        {
            get => _phone_number;
            set => this.RaiseAndSetIfChanged(ref _phone_number, value);
        }

        string _bot_token;
        public string bot_token
        {
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

        public List<PMType> posting_types { get; } = common.common_Available_Posting_Types;

        PMType _posting_type;
        public PMType posting_type
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

        public PMBase(PmModel model, IPMStorage pmStorage, IDBStorage dbStorage, ILogger logger)
        {
            this.model = model;

            this.logger = logger;
            this.pmStorage = pmStorage;
            this.dbStorage = dbStorage;
            this.dbStorage = dbStorage;

            geotag = model.geotag;
            phone_number = model.phone_number;
            bot_token = model.bot_token;
            posting_type = model.posting_type;

            startDate = model.start_date;

            messageProcessorFactory = new MessageProcessorFactory(logger);

            ai = new AIServer("https://gpt.raceup.io");
            server = new TGBotFollowersStatApi("https://ru.flopasda.site");

            businessUpdatesCheckTimer = new System.Timers.Timer();
            businessUpdatesCheckTimer.Interval = 1 * 60 * 60 * 1000; // в часах
            businessUpdatesCheckTimer.AutoReset = true;
            businessUpdatesCheckTimer.Elapsed += ActivityTimer_Elapsed;

            #region commands
            startCmd = ReactiveCommand.CreateFromTask(async () =>
            {
                await Start();
            });
            stopCmd = ReactiveCommand.Create(() =>
            {
                Stop();
            });
            editCmd = ReactiveCommand.Create(() =>
            {

                tmpPmModel = new PmModel()
                {
                    geotag = geotag,
                    phone_number = phone_number,
                    bot_token = bot_token,
                    posting_type = posting_type,

                };

                is_editable = true;
            });
            cancelCmd = ReactiveCommand.Create(() =>
            {
                geotag = tmpPmModel.geotag;
                bot_token = tmpPmModel.bot_token;
                phone_number = tmpPmModel.phone_number;
                posting_type = tmpPmModel.posting_type;
                is_editable = false;
            });
            saveCmd = ReactiveCommand.Create(() =>
            {
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
            verifyCmd = ReactiveCommand.Create(() =>
            {
                user?.SetVerifyCode(verify_code);
            });
            #endregion

        }

        private async void ActivityTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (businessUpdatesCounter == businessUpdatesCounter_prev)
            {
                await errorCollector.Add($"ИИ не был активен в течение часа");
            }

            businessUpdatesCounter_prev = businessUpdatesCounter;   
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
            }
            catch (Exception ex)
            {
                logger.err(geotag, ex.Message);
            }
        }

        async Task<bool> checkNeedProcess(long chat, string fn, string ln, string un)
        {
            bool needProcess = true;
            var userData = await server.GetFollowerSubscriprion(geotag, chat);

            if (userData.Count != 0)
            {
                var datedSubscribes = userData.Where(s => !string.IsNullOrEmpty(s.subscribe_date));
                var maxDateSub = datedSubscribes.Max(s => DateTime.Parse(s.subscribe_date));
                //var sdate = userData[0].subscribe_date;                
                try
                {
                    //var date = DateTime.Parse(sdate);
                    //needProcess = date > startDate;
                    needProcess = maxDateSub > startDate;

                    logger.inf(geotag, $"{chat} {fn} {ln} {un} needProcess={needProcess} {maxDateSub} | {startDate}");

                }
                catch (Exception ex)
                {
                    throw ex;
                }

#if DEBUG_TG_SERV
                needProcess = true;
#endif                
            }


            return needProcess;
        }

        void handleSticker(long chat, string? fn, string? ln, string? un, Sticker? sticker)
        {
            if (sticker != null)
                handleTextMessage(chat, fn, ln, un, sticker.Emoji);
        }

        void handleTextMessage(long chat, string? fn, string? ln, string? un, string? text)
        {
            if (text != null)
            {
                var _ = Task.Run(async () =>
                {

                    var hitem = new List<HistoryItem>()
                        {
                                new HistoryItem(MessageFrom.Lead, text)
                        };

                    try
                    {
                        //await ai.SendHistoryToAI(geotag, chat, fn, ln, un, hitem);
                        await ai.SendToAI(geotag, chat, fn, ln, un, message: text);

                        logger.inf(geotag, $"{fn} {ln} {un} {chat}>{text}");
                    }
                    catch (Exception ex)
                    {
                        logger.err(geotag, $"processBusiness: {ex.Message}");
                    }

                    await Task.Delay(random.Next(5, 21) * 1000);
                    try
                    {
                        await marker?.MarkAsRead(chat);
                    }
                    catch (Exception ex)
                    {
                    }
                });
            }
        }

        async Task handlePhotoMessage()
        {
            await Task.CompletedTask;
        }


        public async virtual Task processBusiness(Telegram.Bot.Types.Update update)
        {

            string caption = update.BusinessMessage.Caption;
            string message = update.BusinessMessage.Text;

            logger.inf(geotag, $"caption: {caption}, message: {message}");

            try
            {
                var chat = update.BusinessMessage.From.Id;
                var bcId = update.BusinessMessage.BusinessConnectionId;

                var fn = update.BusinessMessage.From.FirstName;
                var ln = update.BusinessMessage.From.LastName;
                var un = update.BusinessMessage.From.Username;

                //check self
                var bc = await bot.GetBusinessConnectionAsync(new GetBusinessConnectionRequest(bcId));
                if (chat == bc.User.Id)
                    return;

                db_storage.User user = null;
                bool isNew;

                (user, isNew) = dbStorage.createUserIfNeeded_AI(geotag, chat, bcId, fn, ln, un);

                if (isNew)
                {
                    user.ai_on = await checkNeedProcess(chat, fn, ln, un);
                    if (!user.ai_on)
                        dbStorage.updateUserData(geotag, chat, ai_on: false, ai_off_code: "DATE");
                }

                if (!user.ai_on)
                    return;


                switch (update.BusinessMessage.Type)
                {
                    case Telegram.Bot.Types.Enums.MessageType.Sticker:
                        handleSticker(chat, fn, ln, un, update.BusinessMessage.Sticker);
                        break;

                    case Telegram.Bot.Types.Enums.MessageType.Text:


                        var _ = Task.Run(async () =>
                        {
                            await Task.Delay(20000);
                            await bot.SendChatActionAsync(chat, ChatAction.Typing, businessConnectionId: user.bcId);
                            await Task.Delay(5000);
                            await bot.SendChatActionAsync(chat, ChatAction.Typing, businessConnectionId: user.bcId);
                        });

                        handleTextMessage(chat, fn, ln, un, update.BusinessMessage.Text);

                        break;

                    case Telegram.Bot.Types.Enums.MessageType.Photo:

                        MemoryStream memoryStream = new MemoryStream();

                        var fileId = update.BusinessMessage.Photo.Last().FileId;
                        var fileInfo = await bot.GetFileAsync(fileId);
                        var filePath = fileInfo.FilePath;

                        await bot.DownloadFileAsync(
                                filePath: filePath,
                                destination: memoryStream
                            );

                        string base64_image = null;
                        MemoryStream compressedStream = new MemoryStream();
                        memoryStream.Position = 0;
                        byte[] inputImage = memoryStream.ToArray();
                        memoryStream.Close();

                        using (var originalBitmap = SKBitmap.Decode(inputImage))
                        {
                            var quality = 20; // Quality set to 50
                            using (var image = SKImage.FromBitmap(originalBitmap))
                            using (var data = image.Encode(SKEncodedImageFormat.Jpeg, quality))
                            {
                                data.SaveTo(compressedStream);
                                base64_image = Convert.ToBase64String(compressedStream.ToArray());
                            }

                        }

                        logger.inf(geotag, $"compressed: {inputImage.Length} to {compressedStream.Length}");

                        //var base64_image = Convert.ToBase64String(memoryStream.ToArray());
                        if (base64_image != null)
                        {
                            await ai.SendToAI(geotag, chat, fn, ln, un, message: update.BusinessMessage.Caption, base64_image: base64_image);
                        }

                        memoryStream.Dispose();
                        compressedStream.Dispose();

                        break;

                }
            }
            catch (Exception ex)
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
                            businessUpdatesCounter++;
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

            }
            catch (Exception ex)
            {
                logger.err(geotag, ex.Message);
            }
        }

        async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            await errorCollector.Add(ErrorMessage);
            logger.err(geotag, ErrorMessage);        
        }

        private void User_BusinessBotToggleEvent(long tg_user_id, bool state)
        {
            logger.warn(geotag, $"BusinessBot: {tg_user_id} {state}");
        }
        #endregion

        #region helpers
        protected async Task sendTextMessage(long tg_user_id, string bcid, string message)
        {
            //int delay = (int)(message.Length * 0.1 * 1000);
            //int typings = delay / 5000;

            //if (typings == 0)
            //    typings = 1;

            //for (int i = 0; i < typings; i++)
            //{
            //    await bot.SendChatActionAsync(tg_user_id, ChatAction.Typing, businessConnectionId: bcid);
            //    await Task.Delay(5000);
            //}
            await bot.SendTextMessageAsync(tg_user_id, message, businessConnectionId: bcid);

            //var msg_to_ai = $"{message}";
            //history.Add(MessageFrom.PM, tg_user_id, msg_to_ai);
            logger.inf_urgent(geotag, $"{tg_user_id}>{message}");
        }

        protected async Task sendStatusMessage(long tg_user_id, string bcid, string response_code, string message)
        {
            try
            {        
                int exists_id = 0;
                bool is_used = false;

                (exists_id, is_used) = quoteProcessor.Get(tg_user_id, response_code);

                if (exists_id == -1)
                {
                    var m = MessageProcessor.GetMessage(response_code);
                    int id = await m.Send(tg_user_id, bot, bcid: bcid);
                    quoteProcessor.Add(tg_user_id, response_code, id);
                } 

                if (exists_id != -1 && is_used == false)
                {
                    var m = MessageProcessor.GetMessage("ALREADY_SENT");
                    if (m != null)
                    {
                        await m.Send(tg_user_id, bot, bcid: bcid, reply_message_id: exists_id);
                    }
                }

                if (exists_id != -1 && is_used == true)
                {
                    await bot.SendTextMessageAsync(tg_user_id, message, businessConnectionId: bcid);
                }

            }
            catch (Exception ex)
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
                AllowedUpdates = new UpdateType[] { UpdateType.Message, UpdateType.BusinessConnection, UpdateType.BusinessMessage, UpdateType.EditedBusinessMessage }
            };

            initMessageProcessor();

            //aggregateMessageTimer.Start();

            bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cts.Token);

            //if (user != null)
            //    await user.Start();


            if (!string.IsNullOrEmpty(phone_number))
            {
                user = new user_v0(model.api_id, model.api_hash, phone_number, "5555", logger);
                user.BusinessBotToggleEvent += User_BusinessBotToggleEvent;

                user.VerificationCodeRequestEvent += User_VerificationCodeRequestEvent;
                user.StatusChangedEvent += User_StatusChangedEvent;

                marker = (IMarkRead)user;

                user.Start();
                
            }

            is_active = true;

            logger.inf(geotag, $"Starting PM, posting={posting_type}");
            logger.inf(geotag, $"PM started");
        }

        public void Stop()
        {
            businessUpdatesCheckTimer?.Stop();
            cts?.Cancel();
            logger.inf(geotag, "PM stopped");
            user?.Stop();
            is_active = false;            
        }

        public string GetGeotag()
        {
            return geotag;
        }

        public virtual async Task Update(string source, long tg_user_id, string response_code, string message)
        {

            if (!source.Equals(geotag))
            {
                logger.err(geotag, $"Update: source {source} not equals {geotag}");
                return;
            }


            db_storage.User user = null;
            try
            {
                user = dbStorage.getUser(source, tg_user_id);
            }
            catch (Exception ex)
            {
                logger.err(geotag, $"Update: {source} {tg_user_id} user not found");
            }

            if (user == null)
                return;

            logger.dbg(geotag, $"Update: {source} {tg_user_id} {response_code} ismessage={!string.IsNullOrEmpty(message)}");

            try
            {
                //system codes
                switch (response_code)
                {
                    case "DIALOG_END":
                        dbStorage.updateUserData(geotag, tg_user_id, ai_on: false, ai_off_code: response_code);
                        await server.LeadDistributeRequest(tg_user_id, geotag, AssignmentTypes.RD);
                        break;

                    case "DIALOG_ERROR":
                        dbStorage.updateUserData(geotag, tg_user_id, ai_on: false, ai_off_code: response_code);
                        await server.LeadDistributeRequest(tg_user_id, geotag, AssignmentTypes.RD);
                        return;

                    default:
                        break;
                }


                if (!string.IsNullOrEmpty(message) || !string.IsNullOrEmpty(response_code))
                {
                    var _ = Task.Run(async () =>
                    {

                        if (!response_code.Equals("UNKNOWN"))
                        {
                            var m = MessageProcessor.GetMessage(response_code);
                            if (m != null)
                            {
                                await sendStatusMessage(tg_user_id, user.bcId, response_code, message);
                            }
                            else
                            {
                                await sendTextMessage(tg_user_id, user.bcId, message);
                            }

                        }
                        else
                        {
                            await sendTextMessage(tg_user_id, user.bcId, message);
                        }
                    });
                }

            }
            catch (Exception ex)
            {
                logger.err(geotag, $"Update: {ex.Message}");
            }
        }

        public async Task<DiagnosticsResult> GetDiagnosticsResult()
        {
            DiagnosticsResult result = new DiagnosticsResult();

            result.Geotag = geotag;

            if (user.status != DropStatus.active)
            {
                result.isOk = false;
                result.errorsList.Add($"Номер {user.phone_number} не активeн");
            }

            if (!is_active)
            {
                result.isOk = false;
                result.errorsList.Add($"Бот {bot_username} не активен");
            }

            var errors = await errorCollector.Get();
            if (errors.Length > 0)
            {                
                result.isOk = false;
                foreach (var error in errors)
                {
                    result.errorsList.Add(error);
                }
            }

            await Task.CompletedTask;
            return result;
        }
        #endregion
    }

    public class userInfo
    {
        public long tg_user_id { get; set; }
        public string? fn { get; set; }
        public string? ln { get; set; }
        public string? un { get; set; }
    }

    public enum State
    {
        free,
        waiting_new_message
    }
}
