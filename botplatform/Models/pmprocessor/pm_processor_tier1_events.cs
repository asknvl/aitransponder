using asknvl.logger;
using asknvl.server;
using botplatform.Models.pmprocessor.db_storage;
using botplatform.Models.storage;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;


namespace botplatform.Models.pmprocessor
{
    public class pm_processor_tier1_events : PMBase
    {
        public pm_processor_tier1_events(PmModel model, IPMStorage pmStorage, IDBStorage dbStorage, ILogger logger) : base(model, pmStorage, dbStorage, logger)
        {
        }

        public override Task Start()
        {
            return base.Start().ContinueWith(t =>
            {
                businessUpdatesCheckTimer?.Start();
            });
        }

        public override async Task processBusiness(Update update)
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

                (user, isNew) = dbStorage.createUserIfNeeded_TRCK(geotag, chat, bcId, fn, ln, un);

                if (isNew)
                {

                    dbStorage.updateUserData(geotag, chat, first_msg_id: update.BusinessMessage.MessageId);

                    var needEvent = await checkNeedProcess(chat, fn, ln, un);

                    try
                    {
                        await server.MarkFollowerMadeFeedback(geotag, chat, fn, ln, un, fb_event: needEvent);
                        logger.inf(geotag, $"markFolloweMadeFeedBack: {chat} {fn} {ln} need_event={isNew}");
                    }
                    catch (Exception ex)
                    {
                        logger.err(geotag, $"{ex.Message}");
                    }                   
                }               
            }
            catch (Exception ex)
            {
                logger.err(geotag, ex.Message);
            }
        }

        public override async Task Update(string source, long tg_user_id, string response_code, string message)
        {
            await Task.CompletedTask;
        }
    }
}
