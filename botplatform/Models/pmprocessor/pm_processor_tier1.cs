using asknvl.logger;
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
    public class pm_processor_tier1 : PMBase
    {
        public pm_processor_tier1(PmModel model, IPMStorage pmStorage, IDBStorage dbStorage, ILogger logger) : base(model, pmStorage, dbStorage, logger)
        {
        }

        public override Task Start()
        {
            return base.Start().ContinueWith(t => {
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
                            await notifyAIstate(chat, false);
                        } else
                        {
                            await notifyAIstate(chat, true);
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

                var counter = user.message_counter;
                dbStorage.updateUserData(geotag, chat, message_counter: ++counter);
                logger.warn(geotag, $"updateCounter: {chat} {fn} {ln} counter={counter}");

                if (/*counter == 2*/true)
                {
                    logger.warn(geotag, $"linkMessage: {chat} {fn} {ln} counter={counter}");

                    try
                    {
                        //https://raceup-top1.space?uuid=o497t4gjzt

                        var linkData = await ai.GetLink(geotag, chat);
                        var link = $"{linkData.link}?uuid={linkData.uuid}";

                        var _ = Task.Run(async() => {
                            try
                            {
                                var m = MessageProcessor.GetMessage("LINK", link: link);
                                await Task.Delay(/*30000*/2000);
                                var id = await m.Send(chat, bot, bcid: bcId);
                                await bot.PinChatMessageAsync(chat, id, businessConnectionId: bcId);
                                
                            } catch (Exception ex)
                            {
                                logger.err(geotag, $"sendLinkMesage: {ex.Message}");
                            }
                        });

                    } catch (Exception ex)
                    {
                        logger.err(geotag, $"linkMessage: {chat} {fn} {ln} {ex.Message}");
                    }

                }

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
    }
}
