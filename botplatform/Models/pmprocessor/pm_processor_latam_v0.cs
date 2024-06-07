using asknvl.logger;
using botplatform.Models.pmprocessor.db_storage;
using botplatform.Models.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;

namespace botplatform.Models.pmprocessor
{
    public class pm_processor_latam_v0 : PMBase, IAutoReplyObserver
    {


        public pm_processor_latam_v0(PmModel model, IPMStorage pmStorage, IDBStorage dbStorage, ILogger logger) : base(model, pmStorage, dbStorage, logger)
        {
        }

        public string GetChannelTag()
        {
            return geotag;
        }

        #region override
        public override async Task processBusiness(Update update)
        {
            try
            {

                var chat = update.BusinessMessage.From.Id;
                var fn = update.BusinessMessage.From.FirstName;
                var ln = update.BusinessMessage.From.LastName;
                var un = update.BusinessMessage.From.Username;

                var messageId = update.BusinessMessage.MessageId;

                var bcId = update.BusinessMessage.BusinessConnectionId;

                var bc = await bot.GetBusinessConnectionAsync(new GetBusinessConnectionRequest(bcId));
                var pmId = bc.User.Id;

                var userId = (update.BusinessMessage.From.Id != pmId) ? update.BusinessMessage.From.Id : update.BusinessMessage.Chat.Id;

                db_storage.User user = null;
                bool needProcess = false;

                (user, needProcess) = dbStorage.createUserIfNeeded(geotag, userId, bcId, fn, ln, un, ai_on: false);

                if (needProcess)
                {
                    if (chat != pmId)
                    {
                        //in
                        logger.dbg(geotag, $"{pmId} < {userId} message");
                        dbStorage.updateUserData(geotag, userId, first_msg_id: messageId);
                        await server.MarkFollowerMadeFeedback(geotag, userId, fn, ln, un);
                    }
                    else
                    {
                        //out
                        logger.dbg(geotag, $"{pmId} > {userId} message");
                        dbStorage.updateUserData(geotag, userId, is_reply: true);
                        await server.MarkFollowerWasReplied(geotag, userId);
                    }
                }

            }
            catch (Exception ex)
            {
                logger.err(geotag, $"processBusinessMessage: {ex.Message}");
            }
        }
        #endregion

        #region public
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

                try
                {
                    await m.Send(user.tg_id, bot, bcid: user.bcId);
                }
                catch (Exception ex)
                {
                    logger.err(geotag, $"AutoReply: {ex.Message}");
                }
                //} catch (Exception ex)
                //{

                //} 
            }
            catch (Exception ex)
            {
                logger.err(geotag, $"AutoReply: {ex.Message}");
            }
        }
        #endregion
    }
}
