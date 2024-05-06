using asknvl;
using asknvl.logger;
using botplatform.Model.bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TL;

namespace botplatform.Models.pmprocessor.userapi
{
    public class user_v0 : TGUserBase, IMarkRead
    {
        public user_v0(string api_id, string api_hash, string phone_number, string _2fa_password, ILogger logger) : base(api_id, api_hash, phone_number, _2fa_password, logger)
        {

        }
        
        public async Task MarkAsRead(long id)
        {
            if (status == DropStatus.active)
            {
                try
                {
                    TL.User foundUser = null;
                    var found = users.TryGetValue(id, out foundUser);
                    var peer = new InputPeerUser(foundUser.ID, foundUser.access_hash);
                    //var histoty = await user.Messages_GetHistory(peer);
                    await user.ReadHistory(peer);
                    logger.err(phone_number, $"MarkeAsRead {id} OK");

                }
                catch (Exception ex)
                {
                    logger.err(phone_number, $"MarkAsRead {id}: {ex.Message}");
                }
            }
        }

        protected override Task processUpdate(Update update)
        {
            return Task.CompletedTask;
        }
    }
}
