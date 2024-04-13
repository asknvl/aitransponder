using asknvl.logger;
using botplatform.Model.bot;
using botplatform.Models.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.pmprocessor
{
    public class PmFactory : IPmFactory
    {

        #region vars
        IOperatorStorage operatorStorage;
        IPMStorage pmStorage;
        ILogger logger;
        #endregion

        public PmFactory(IOperatorStorage operatorStorage, IPMStorage pmStorage, ILogger logger)
        {
            this.operatorStorage = operatorStorage;
            this.pmStorage = pmStorage;
            this.logger = logger;
        }

        #region public
        public PMBase Get(PmModel model)
        {
            return new pm_processor_v0(model, operatorStorage, pmStorage, logger);
        }
        #endregion
    }
}
