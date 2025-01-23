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
using TL;

namespace botplatform.Models.pmprocessor
{
    public abstract class PMBase : ViewModelBase, IPM, IMessageObserver, IDiagnosticsResulter
    {
        #region vars
        PmModel model;
        protected string? direction = null;

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

        protected QuoteProcessor quoteProcessor;

        protected IAIserver ai;
        protected ITGBotFollowersStatApi server;

        DateTime startDate;
        Random random = new Random();

        protected errorCollector errorCollector = new();

        protected System.Timers.Timer businessUpdatesCheckTimer;
        uint businessUpdatesCounter = 0;
        uint businessUpdatesCounter_prev = 0;

        object lockObject = new object();
        List<long> aiProcessedUsers = new List<long>();
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

        bool _is_ai_enabled = true;
        public bool is_ai_enabled
        {
            get => _is_ai_enabled;
            set => this.RaiseAndSetIfChanged(ref _is_ai_enabled, value);
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

        public PMBase(PmModel model, IPMStorage pmStorage, IDBStorage dbStorage, ILogger logger, string? direction = null)
        {
            this.model = model;
            this.direction = direction;

            this.logger = logger;
            this.pmStorage = pmStorage;
            this.dbStorage = dbStorage;
            this.dbStorage = dbStorage;

            geotag = model.geotag;
            phone_number = model.phone_number;
            bot_token = model.bot_token;
            posting_type = model.posting_type;
            is_ai_enabled = model.is_ai_enabled;
            startDate = model.start_date;

            quoteProcessor = new QuoteProcessor(geotag);

            messageProcessorFactory = new MessageProcessorFactory(logger);

            var settings = Settings.getInstance();

            //ai = new AIServer("https://gpt.raceup.io");
            ai = new AIServer(settings.ai_server, settings.ai_token);

            //server = new TGBotFollowersStatApi("https://ru.flopasda.site");
            server = new TGBotFollowersStatApi(settings.stat_server, settings.stat_token);  

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

        async Task processOwner(Telegram.Bot.Types.Message message)
        {
            try
            {

                var splt = message.Text.Split(":");

                switch (splt[0])
                {
                    case "AI":
                        switch (splt[1])
                        {
                            case "STATUS":
                                var tg_user_id = long.Parse(splt[2]);
                                var status = splt[3].Equals("ON");
                                var code = "MANUAL";

                                if (!is_ai_enabled && status)
                                    return;

                                dbStorage.updateUserData(geotag, tg_user_id, ai_on: status, ai_off_code: code);
                                await processAIState(tg_user_id, status, code);
                                break;
                        }
                        break;
                }


                logger.inf(geotag, message.Text);

            } catch (Exception ex)
            {
                logger.err(geotag, ex.Message);
            }
        }

        protected async Task<bool> checkNeedProcess(long chat, string fn, string ln, string un)
        {
            bool needProcess = true;
            var userData = await server.GetFollowerSubscriprion(geotag, chat);

            if (userData.Count != 0)
            {
                
                //var sdate = userData[0].subscribe_date;                
                try
                {
                    var datedSubscribes = userData.Where(s => !string.IsNullOrEmpty(s.subscribe_date));
                    var maxDateSub = datedSubscribes.Max(s => DateTime.Parse(s.subscribe_date));

                    //var date = DateTime.Parse(sdate);
                    //needProcess = date > startDate;
                    needProcess = maxDateSub > startDate;

                    logger.inf(geotag, $"{chat} {fn} {ln} {un} needProcess={needProcess} {maxDateSub} | {startDate}");

                }
                catch (Exception ex)
                {
                    //var msg = $"checkNeedProcess: {chat} {ex.Message} (1)";
                    //await errorCollector.Add(msg);
                    //logger.err(geotag, msg);
                }

#if DEBUG_TG_SERV
                needProcess = true;
#endif                
            }


            return needProcess;
        }

        protected void handleSticker(long chat, string? fn, string? ln, string? un, Sticker? sticker)
        {
            if (sticker != null)
                handleTextMessage(chat, fn, ln, un, sticker.Emoji);
        }

        protected void handleTextMessage(long chat, string? fn, string? ln, string? un, string? text)
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
                        await ai.SendToAI(geotag, chat, fn, ln, un, message: text, direction: direction);


                        logger.inf(geotag, $"{fn} {ln} {un} {chat}>{text}");
                    }
                    catch (Exception ex)
                    {
                        await processAIState(chat, false, "ERROR");
                        logger.err(geotag, $"processBusiness: {ex.Message}");
                        return;
                    }

                    await Task.Delay(random.Next(20, 40) * 1000);
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

        protected async Task notifyAIEnabled(long tg_id)
        {
            bool found = false;
            lock (lockObject)
            {
                found = aiProcessedUsers.Any(u => u == tg_id);
                if (!found)
                    aiProcessedUsers.Add(tg_id);

                if (aiProcessedUsers.Count > 2048)
                    aiProcessedUsers.RemoveAt(0);
            }
            if (!found)
                await processAIState(tg_id, true);
        }

        public async virtual Task processBusiness(Telegram.Bot.Types.Update update)
        {

            string caption = update.BusinessMessage.Caption;
            string message = update.BusinessMessage.Text;

            logger.inf(geotag, $"caption: {caption}, message: {message}");

            var chat = update.BusinessMessage.From.Id;
            var bcId = update.BusinessMessage.BusinessConnectionId;

            var fn = update.BusinessMessage.From.FirstName;
            var ln = update.BusinessMessage.From.LastName;
            var un = update.BusinessMessage.From.Username;

            try
            {
                //check self
                var bc = await bot.GetBusinessConnection(bcId);

                if (chat == bc.User.Id)
                {
                    var userId = update.BusinessMessage.Chat.Id;

                    var found = dbStorage.getUser(geotag, userId);
                    if (found != null)
                    {
                        if (!found.is_first_msg_rep)
                        {
                            dbStorage.updateUserData(geotag, userId, is_reply: true);
                            try
                            {
                                await server.MarkFollowerWasReplied(geotag, userId);
                            } catch (Exception ex)
                            {
                                logger.err(geotag, $"{ex.Message}");
                            }
                        }
                    }

                    return;
                }

                db_storage.User user = null;
                bool isNew;

                (user, isNew) = dbStorage.createUserIfNeeded_AI(geotag, chat, bcId, fn, ln, un);

                if (isNew)
                {

                    dbStorage.updateUserData(geotag, chat, first_msg_id: update.BusinessMessage.MessageId);

                    try
                    {
                        await server.MarkFollowerMadeFeedback(geotag, chat, fn, ln, un, fb_event: isNew);
                        logger.inf(geotag, $"markFolloweMadeFeedBack: {chat} {fn} {ln} need_event={isNew}");
                    } catch (Exception ex)
                    {
                        logger.err(geotag, $"{ex.Message}");
                    }

                    try
                    {
                        user.ai_on = await checkNeedProcess(chat, fn, ln, un);
                        
                    } catch (Exception ex)
                    {
                        var msg = $"checkNeedProcess: {chat} {ex.Message} (2)";
                        await errorCollector.Add(msg);
                        logger.err(geotag, msg);
                    }

                    try
                    {
                        if (!user.ai_on)
                        {
                            dbStorage.updateUserData(geotag, chat, ai_on: false, ai_off_code: "DATE");
                            await processAIState(chat, false, code: "DATE");
                        } else
                        {
                            //await notifyAIstate(chat, true);                           
                        }

                    } catch (Exception ex)
                    {
                        var msg = $"updateUserData: {chat} {ex.Message}";
                        await errorCollector.Add(msg);
                        logger.err(geotag, msg);
                    }
                }

                if (!user.ai_on)
                    return;

                await notifyAIEnabled(chat);

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
                            await ai.SendToAI(geotag, chat, fn, ln, un, message: update.BusinessMessage.Caption, base64_image: base64_image, direction: direction);
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
                            else
                                if (chat == user.tg_id)
                                    await processOwner(update.Message);
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
        protected virtual async Task sendStatusMessage(long tg_user_id, string bcid, string response_code, string message)
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
        protected async Task processAIState(long tg_id, bool isActive, string? code = null)
        {
            try
            {
                var state = isActive ? "ON" : "OFF";

                if (!isActive) 
                    quoteProcessor.Remove(tg_id);

                var outCode = (!string.IsNullOrEmpty(code)) ? $":{code}" : "";
                var message = $"AI:STATUS:{tg_id}:{state}{outCode}";
                await bot.SendTextMessageAsync(user.tg_id, message);
                logger.warn(geotag, message);
            }
            catch (Exception ex)
            {
                logger.err(geotag, $"notifyAIstate: {tg_id} isActive={isActive} {ex.Message}");
                await errorCollector.Add($"Не удалось передать данные о состоянии ИИ {tg_id}");
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

                await user.Start();
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

            if (user == null || !user.ai_on)
                return;

            logger.dbg(geotag, $"Update: {source} {tg_user_id} {response_code} ismessage={!string.IsNullOrEmpty(message)}");

            try
            {
                //system codes
                switch (response_code)
                {
                    case "DIALOG_END":
                        dbStorage.updateUserData(geotag, tg_user_id, ai_on: false, ai_off_code: response_code);
                        try
                        {
                            await server.LeadDistributeRequest(tg_user_id, geotag, AssignmentTypes.RD);
                        } catch(Exception ex)
                        {
                        }

                        await processAIState(tg_user_id, false, code: "DIALOG_END");
                        break;

                    case "DIALOG_ERROR":
                        dbStorage.updateUserData(geotag, tg_user_id, ai_on: false, ai_off_code: response_code);
                        try
                        {
                            await server.LeadDistributeRequest(tg_user_id, geotag, AssignmentTypes.RD);
                        } catch (Exception ex)
                        {

                        }
                        await processAIState(tg_user_id, false, code: "DIALOG_ERROR");
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

                        var found = dbStorage.getUser(geotag, tg_user_id);
                        if (found != null)
                        {
                            if (!found.is_first_msg_rep)
                            {
                                dbStorage.updateUserData(geotag, tg_user_id, is_reply: true);
                                try
                                {
                                    await server.MarkFollowerWasReplied(geotag, tg_user_id);
                                }
                                catch (Exception ex)
                                {
                                    logger.err(geotag, $"{ex.Message}");
                                }
                            }
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
