using asknvl.logger;
using botplatform.Model.bot;
using botplatform.Models.storage;
using botplatform.rest;
using motivebot.Model.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.bot
{
    public class transponder_bot : BotBase
    {

        public override BotType Type => BotType.transponder_v1;

        public transponder_bot(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(operatorStorage, botStorage, logger)
        {
            Geotag = model.geotag;
            Token = model.token;
        }

        public override Task Notify(object notifyObject)
        {
            throw new NotImplementedException();
        }

        public override Task UpdateStatus(StatusUpdateDataDto updateData)
        {
            throw new NotImplementedException();
        }
    }
}
