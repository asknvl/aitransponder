using aksnvl.messaging;
using asknvl.logger;
using asknvl.server;
using Avalonia.X11;
using botplatform.Models.bot;
using botplatform.Models.messages;
using botplatform.Models.storage;
using botplatform.Operators;
using botplatform.rest;
using botplatform.ViewModels;
using HarfBuzzSharp;
using motivebot.Model.storage;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
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
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.Mime.MediaTypeNames;

namespace botplatform.Model.bot
{
    public abstract class BotBase : ViewModelBase, IPushObserver, IStatusObserver, INotifyObserver
    {

        #region vars        
        protected IOperatorStorage operatorStorage;
        protected IBotStorage botStorage;
        protected ILogger logger;
        protected ITelegramBotClient bot;
        protected CancellationTokenSource cts;
        protected State state = State.free;
        protected ITGBotFollowersStatApi server;
        protected long ID;
        IMessageProcessorFactory messageProcessorFactory;
        BotModel tmpBotModel;

        #endregion

        #region properties        
        public abstract BotType Type { get; }

        string geotag;
        public string Geotag
        {
            get => geotag;
            set => this.RaiseAndSetIfChanged(ref geotag, value);
        }

        string? name;
        public string? Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        string token;
        public string Token
        {
            get => token;
            set => this.RaiseAndSetIfChanged(ref token, value);
        }

        string? link;
        public string? Link
        {
            get => link;
            set => this.RaiseAndSetIfChanged(ref link, value);
        }

        string? pm;
        public string? PM
        {
            get => pm;
            set => this.RaiseAndSetIfChanged(ref pm, value);
        }

        string? channel;
        public string? Channel
        {
            get => channel;
            set => this.RaiseAndSetIfChanged(ref channel, value);
        }

        bool? postbacks;
        public bool? Postbacks
        {
            get => postbacks;
            set
            {
                if (value == null)
                    value = false;
                this.RaiseAndSetIfChanged(ref postbacks, value);
            }
        }

        bool isActive = false;
        public bool IsActive
        {
            get => isActive;
            set
            {
                IsEditable = false;
                this.RaiseAndSetIfChanged(ref isActive, value);
            }
        }

        bool isEditable;
        public bool IsEditable
        {
            get => isEditable;
            set => this.RaiseAndSetIfChanged(ref isEditable, value);
        }

        MessageProcessorBase messageProcessor;
        public MessageProcessorBase MessageProcessor
        {
            get => messageProcessor;
            set => this.RaiseAndSetIfChanged(ref messageProcessor, value);
        }

        string awaitedMessageCode;
        public string AwaitedMessageCode
        {
            get => awaitedMessageCode;
            set => this.RaiseAndSetIfChanged(ref awaitedMessageCode, value);
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> startCmd { get; }
        public ReactiveCommand<Unit, Unit> stopCmd { get; }
        public ReactiveCommand<Unit, Unit> editCmd { get; }
        public ReactiveCommand<Unit, Unit> cancelCmd { get; }
        public ReactiveCommand<Unit, Unit> saveCmd { get; }
        #endregion

        public BotBase(IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger)
        {
            this.logger = logger;            
            this.operatorStorage = operatorStorage;
            this.botStorage = botStorage;

            messageProcessorFactory = new MessageProcessorFactory(logger);

            #region commands
            startCmd = ReactiveCommand.CreateFromTask(async () =>
            {
                await Start();
            });

            stopCmd = ReactiveCommand.Create(() =>
            {
                Stop();
            });

            editCmd = ReactiveCommand.Create(() => {

                tmpBotModel = new BotModel()
                {
                    type = Type,
                    geotag = Geotag,
                    token = Token,
                    link = Link,
                    pm = PM,
                    channel = Channel,
                    postbacks = Postbacks
                };

                IsEditable = true;
            });

            cancelCmd = ReactiveCommand.Create(() => {

                Geotag = tmpBotModel.geotag;
                Token = tmpBotModel.token;
                Link = tmpBotModel.link;
                PM = tmpBotModel.pm;
                Channel = tmpBotModel.channel;
                Postbacks = tmpBotModel.postbacks;

                IsEditable = false;

            });

            saveCmd = ReactiveCommand.Create(() => {


                var updateModel = new BotModel()
                {
                    type = Type,
                    geotag = Geotag,
                    token = Token,
                    link = Link,
                    pm = PM,
                    channel = Channel,
                    postbacks = Postbacks
                };

                botStorage.Update(updateModel);

                IsEditable = false;

            });
            #endregion
        }

        #region private
        public virtual async Task processFollower(Message message)
        {
            await Task.CompletedTask;
        }

        protected virtual async Task processCallbackQuery(CallbackQuery query)
        {
            await Task.CompletedTask;
        }

        protected virtual async Task sendOperatorTextMessage(Operator op, long chat, string text)
        {
            await Task.CompletedTask;
        }

        protected virtual async Task processOperator(Message message, Operator op)
        {

            var chat = message.From.Id;

            try
            {
                if (state == State.waiting_new_message)
                {
                    MessageProcessor.Add(AwaitedMessageCode, message, PM);
                    state = State.free;
                    return;
                }           
            }
            catch (Exception ex)
            {
                logger.err(Geotag, ex.Message);
            }
        }

        async Task processSubscribe(Update update)
        {

            await Task.CompletedTask;
        }

        async Task processMessage(Message message)
        {
            long chat = message.Chat.Id;

            var op = operatorStorage.GetOperator(geotag, chat);
            if (op != null)
            {
                await processOperator(message, op);
            }
            else
            {
                await processFollower(message);
            }
        }

        async Task processChatJoinRequest(ChatJoinRequest chatJoinRequest, CancellationToken cancellationToken)
        {
            //try
            //{
            //    var message = MessageProcessor.GetChatJoinMessage();
            //    if (message != null)
            //    {
            //        await message.Send(chatJoinRequest.From.Id, bot);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    logger.err(Geotag, $"processChatJoinRequest: {ex.Message}");
            //}
        }

        async Task processChatMember(ChatMemberUpdated chatMember, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }


        async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {

            if (update == null)
                return;

            if (update.BusinessMessage == null)
                return;

            try
            {

                switch (update.Type)
                {
                    case UpdateType.BusinessMessage:
                        var text = update.BusinessMessage.Text;
                        //await bot.SendMessageAsync(new SendMessageRequest(update.BusinessMessage.Chat.Id, "Вы кто такие? Я вас не звал! Идите на хуй!"));

                        var bc = await bot.GetBusinessConnectionAsync(new GetBusinessConnectionRequest(update.BusinessMessage.BusinessConnectionId));
                        
                        await Task.Delay(2000);
                        await bot.SendChatActionAsync(update.BusinessMessage.From.Id, ChatAction.Typing, businessConnectionId: update.BusinessMessage.BusinessConnectionId);
                        await Task.Delay(5000);

                        if (update.BusinessMessage.From.Id != bc.User.Id) {

                            var f = InputFile.FromUri("https://telegra.ph/file/ec3ab13268efb7d126de4.jpg");
                            await bot.SendPhotoAsync(update.BusinessMessage.Chat.Id, f, caption: "Добрый день. Я отвечу Вам позже.", businessConnectionId: update.BusinessMessage.BusinessConnectionId);
                        }

                            //await bot.SendTextMessageAsync(update.BusinessMessage.Chat.Id, "Вы кто такие? Я вас не звал! Идите на хуй!", businessConnectionId: update.BusinessMessage.BusinessConnectionId);

                        logger.inf("BSN", $"{text} {bc.Id}");
                        break;

                    default:
                        break;
                }

            } catch (Exception ex)
            {
            } 
        }

        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"{Geotag} Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            logger.err(Geotag, ErrorMessage);
            return Task.CompletedTask;
        }
        #endregion

        #region public
        public virtual async Task Start()
        {
            logger.inf(Geotag, $"Starting {Type} bot...");

            if (IsActive)
            {
                logger.err(Geotag, "Bot already started");
                return;
            }


#if DEBUG
            //server = new TGBotFollowersStatApi("http://185.46.9.229:4000");
            //bot = new TelegramBotClient(new TelegramBotClientOptions(Token, "http://localhost:8081/bot/"));
            server = new TGBotFollowersStatApi("http://136.243.74.153:4000");
            bot = new TelegramBotClient(new TelegramBotClientOptions(Token, "http://localhost:8081/bot/"));
#elif DEBUG_TG_SERV

            //server = new TGBotFollowersStatApi("http://185.46.9.229:4000");            
            server = new TGBotFollowersStatApi("http://136.243.74.153:4000");
            bot = new TelegramBotClient(Token);
#else
            server = new TGBotFollowersStatApi("http://136.243.74.153:4000");
            bot = new TelegramBotClient(new TelegramBotClientOptions(Token, "http://localhost:8081/bot/"));
#endif

            var u = await bot.GetMeAsync();
            Name = u.Username;
            ID = u.Id;
            
            

            //var bc = await bot.GetBusinessConnectionAsync(new GetBusinessConnectionRequest(bcId));
            
            cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[] { 
                    UpdateType.Message,
                    UpdateType.BusinessConnection,
                    UpdateType.BusinessMessage
                }
            };

            //MessageProcessor = new MessageProcessor_v0(geotag, bot);
            MessageProcessor = messageProcessorFactory.Get(Type, Geotag, Token, bot);

            if (MessageProcessor != null)
            {
                MessageProcessor.UpdateMessageRequestEvent += async (code, description) =>
                {
                    AwaitedMessageCode = code;
                    state = State.waiting_new_message;

                    //var operators = operatorsProcessor.GetAll(geotag).Where(o => o.permissions.Any(p => p.type.Equals(OperatorPermissionType.all)));
                    var operators = operatorStorage.GetAll(geotag).Where(o => o.permissions.Any(p => p.type.Equals(OperatorPermissionType.all)));

                    foreach (var op in operators)
                    {
                        try
                        {
                            await bot.SendTextMessageAsync(op.tg_id, $"Перешлите сообщение для: \n{description.ToLower()}");
                        }
                        catch (Exception ex)
                        {
                            logger.err("BOT", $"UpdateMessageRequestEvent: {ex.Message}");
                        }
                    }
                };

                MessageProcessor.ShowMessageRequestEvent += async (message, code) =>
                {
                    //var operators = operatorsProcessor.GetAll(geotag).Where(o => o.permissions.Any(p => p.type.Equals(OperatorPermissionType.all)));                
                    var operators = operatorStorage.GetAll(geotag).Where(o => o.permissions.Any(p => p.type.Equals(OperatorPermissionType.all)));

                    foreach (var op in operators)
                    {
                        try
                        {
                            int id = await message.Send(op.tg_id, bot);
                        }
                        catch (Exception ex)
                        {
                            logger.err(Geotag, $"ShowMessageRequestEvent: {ex.Message}");
                        }
                    }
                };
                MessageProcessor.Init();
            }

            bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cts.Token);

            try
            {
                await Task.Run(() => { });
                IsActive = true;
                logger.inf(Geotag, "Bot started");

            }
            catch (Exception ex)
            {
            }
        }

        public virtual async void Stop()
        {
            cts.Cancel();
            IsActive = false;
            logger.inf(Geotag, "Bot stopped");
        }

        public string GetGeotag()
        {
            return Geotag;
        }

        public virtual async Task<bool> Push(long id, string code, int notification_id)
        {
            await Task.CompletedTask;
            return false;
        }

        public abstract Task UpdateStatus(StatusUpdateDataDto updateData);

        public abstract Task Notify(object notifyObject);
        #endregion
    }
}
