using asknvl.logger;
using botplatform.Models.storage;
using System;

namespace botplatform.Models.pmprocessor
{
    public class PmFactory : IPmFactory
    {

        #region vars        
        IPMStorage pmStorage;
        ILogger logger;
        #endregion

        public PmFactory(IPMStorage pmStorage, ILogger logger)
        {            
            this.pmStorage = pmStorage;
            this.logger = logger;
        }

        #region public
        public PMBase Get(PmModel model)
        {
            switch (model.posting_type)
            {
                case PMType.qualification_pm:
                    return new pm_processor_v0(model, pmStorage, logger);

                default:
                    throw new NotImplementedException();
            }
            
        }
        #endregion
    }

}
