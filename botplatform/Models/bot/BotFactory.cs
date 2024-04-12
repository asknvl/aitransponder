using asknvl.logger;
using botplatform.Model.bot;
using botplatform.Models.storage;
using botplatform.Models.storage.local;
using botplatform.Operators;
using motivebot.Model.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.bot
{
    public class BotFactory : IBotFactory
    {

        #region vars        
        IOperatorStorage operatorStorage;
        IBotStorage botStorage;
        #endregion

        public BotFactory(IOperatorStorage operatorStorage, IBotStorage botStorage)
        {            
            this.operatorStorage = operatorStorage;
            this.botStorage = botStorage;
        }

        public BotBase Get(BotModel model, ILogger logger)
        {
            switch (model.type)
            {           
                case BotType.transponder_v1:
                    return new transponder_bot(model, operatorStorage, botStorage, logger);
                
                 

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
