using asknvl.logger;
using asknvl.server;
using botplatform.Models.pmprocessor.db_storage;
using botplatform.Models.pmprocessor.quote_rocessor;
using botplatform.Models.storage;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace botplatform.Models.pmprocessor
{
    public class pm_processor_latam_v1 : PMBase, IAutoReplyObserver
    {
        public pm_processor_latam_v1(PmModel model, IPMStorage pmStorage, IDBStorage dbStorage, ILogger logger) : base(model, pmStorage, dbStorage, logger, direction: "latam")
        {
        }

        public override async Task processBusiness(Update update)
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
                            }
                            catch (Exception ex)
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
                        await server.MarkFollowerMadeFeedback(geotag, chat, fn, ln, un);
                    }
                    catch (Exception ex)
                    {
                        logger.err(geotag, $"{ex.Message}");
                    }

                    try
                    {
                        user.ai_on = await checkNeedProcess(chat, fn, ln, un);
                    }
                    catch (Exception ex)
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
                            await processAIState(chat, false);
                        }
                        else
                        {
                            //await notifyAIstate(chat, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        var msg = $"updateUserData: {chat} {ex.Message}";
                        await errorCollector.Add(msg);
                        logger.err(geotag, msg);
                    }
                }

                if (!user.ai_on)
                    return;

                await notifyAIEnabled(chat);

                var counter = user.message_counter;
                dbStorage.updateUserData(geotag, chat, message_counter: ++counter);
                logger.warn(geotag, $"updateCounter: {chat} {fn} {ln} counter={counter}");
                
                switch (update.BusinessMessage.Type)
                {
                    case MessageType.Sticker:
                        handleSticker(chat, fn, ln, un, update.BusinessMessage.Sticker);
                        break;

                    case MessageType.Text:


                        var _ = Task.Run(async () =>
                        {
                            await Task.Delay(20000);
                            await bot.SendChatActionAsync(chat, ChatAction.Typing, businessConnectionId: user.bcId);
                            await Task.Delay(5000);
                            await bot.SendChatActionAsync(chat, ChatAction.Typing, businessConnectionId: user.bcId);
                        });

                        handleTextMessage(chat, fn, ln, un, update.BusinessMessage.Text);
                        break;
                }

            } catch (Exception ex)
            {
                logger.err(geotag, $"processBusiness: {chat} {fn} {ln} {ex.Message}");
            }
        }

        public async Task AutoReply(string channel_tag, long user_tg_id, string status_code, string? message)
        {
            try
            {

                logger.dbg(geotag, $"autoreply request {channel_tag} {user_tg_id} {status_code}");
                var user = dbStorage.getUser(channel_tag, user_tg_id);

                if (user.was_autoreply)
                    return;

                dbStorage.updateUserData(user.geotag, user_tg_id, was_autoreply: true);

                var m = MessageProcessor.GetMessage(status_code);
                if (m == null)
                    await errorCollector.Add($"Не установлен автоответ {status_code}");

                try
                {
                    await m.Send(user.tg_id, bot, bcid: user.bcId);
                }
                catch (Exception ex)
                {
                    logger.err(geotag, $"AutoReply: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                logger.err(geotag, $"AutoReply: {ex.Message}");
            }
        }

        public string GetChannelTag()
        {
            return geotag;
        }

        override protected async Task sendStatusMessage(long tg_user_id, string bcid, string response_code, string message)
        {
            try
            {
                int exists_id = 0;
                bool is_used = false;

                (exists_id, is_used) = quoteProcessor.Get(tg_user_id, response_code);

                if (exists_id == -1)
                {
                    var m = MessageProcessor.GetMessage(response_code);
                    if (m != null)
                    {
                        try
                        {
                            int id = await m.Send(tg_user_id, bot, bcid: bcid);
                            quoteProcessor.Add(tg_user_id, response_code, id);
                            await Task.Delay(3000);
                        } catch (Exception ex) { }
                    }
                    await bot.SendTextMessageAsync(tg_user_id, message, businessConnectionId: bcid);
                   
                } else
                {
                    await bot.SendTextMessageAsync(tg_user_id, message, businessConnectionId: bcid);
                }
            }
            catch (Exception ex)
            {
                logger.err(geotag, $"sendStatusMessage: {ex.Message}");
            }

        }

        public override async Task Update(string source, long tg_user_id, string response_code, string message)
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

            if (user == null /*|| !user.ai_on*/)
                return;

            logger.dbg(geotag, $"Update: {source} {tg_user_id} {response_code} message={message}");

            try
            {
                //system codes
                switch (response_code)
                {
                    case "DIALOG_END":
                        dbStorage.updateUserData(geotag, tg_user_id, ai_on: false, ai_off_code: response_code);
                        await processAIState(tg_user_id, false, code: "DIALOG_END");
                        break;

                    case "DIALOG_ERROR":
                        dbStorage.updateUserData(geotag, tg_user_id, ai_on: false, ai_off_code: response_code);                        
                        await processAIState(tg_user_id, false, code: "DIALOG_ERROR");
                        return;

                    default:
                        break;
                }


                var _ = Task.Run(async () => {

                    try
                    {
                        if (response_code.Contains("PUSH"))
                        {
                            var m = MessageProcessor.GetMessage(response_code);
                            if (m != null)
                            {
                                try
                                {
                                    int id = await m.Send(tg_user_id, bot, bcid: user.bcId);
                                    logger.inf(geotag, $"Push: {tg_user_id} {response_code}");
                                }
                                catch (Exception ex) {

                                    logger.err(geotag, $"Update: {ex.Message}");
                                }
                            }

                            return;
                        }

                        if (!response_code.Equals("UNKNOWN"))
                        {
                            await sendStatusMessage(tg_user_id, user.bcId, response_code, message);                            
                        } else
                        {
                            if (!string.IsNullOrEmpty(message))
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
                    }
                    catch (Exception ex)
                    {
                        logger.err(geotag, $"Update send: {ex.Message}");
                    }

                });
            }
            catch (Exception ex)
            {
                logger.err(geotag, $"Update: {ex.Message}");
            }
        }
    }
}
