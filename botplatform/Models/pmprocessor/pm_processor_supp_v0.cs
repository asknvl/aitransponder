﻿using asknvl.logger;
using asknvl.server;
using botplatform.Models.pmprocessor.db_storage;
using botplatform.Models.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.pmprocessor
{
    public class pm_processor_supp_v0 : PMBase
    {
        public pm_processor_supp_v0(PmModel model, IPMStorage pmStorage, IDBStorage dbStorage, ILogger logger) : base(model, pmStorage, dbStorage, logger)
        {
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

            if (user == null)
                return;

            logger.dbg(geotag, $"Update: {source} {tg_user_id} {response_code} ismessage={!string.IsNullOrEmpty(message)}");

            try
            {

                switch (response_code)
                {
                    case "DIALOG_END":
                        dbStorage.updateUserData(geotag, tg_user_id, ai_on: false, ai_off_code: response_code);                        
                        break;

                    case "DIALOG_LIMIT_END":
                        dbStorage.updateUserData(geotag, tg_user_id, ai_on: false, ai_off_code: response_code);
                        break;

                    case "DIALOG_ERROR":
                        dbStorage.updateUserData(geotag, tg_user_id, ai_on: false, ai_off_code: response_code);                        
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
    }
}