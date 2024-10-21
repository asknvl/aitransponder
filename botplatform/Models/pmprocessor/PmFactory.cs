using asknvl.logger;
using botplatform.Models.pmprocessor.db_storage;
using botplatform.Models.storage;
using System;

namespace botplatform.Models.pmprocessor
{
    public class PmFactory : IPmFactory
    {

        #region vars        
        IPMStorage pmStorage;
        IDBStorage dbStorage;
        ILogger logger;
        #endregion

        public PmFactory(IPMStorage pmStorage, IDBStorage dbStorage, ILogger logger)
        {            
            this.pmStorage = pmStorage;
            this.dbStorage = dbStorage; 
            this.logger = logger;
        }

        #region public
        public PMBase Get(PmModel model)
        {
            switch (model.posting_type)
            {
                case PMType.qualification_pm:
                    return new pm_processor_qual_v0(model, pmStorage, dbStorage, logger);

                case PMType.support_pm:
                    return new pm_processor_supp_v0(model, pmStorage, dbStorage, logger);

                case PMType.latam_pm:
                    return new pm_processor_latam_v0(model, pmStorage, dbStorage, logger);

                case PMType.tier_1:
                    return new pm_processor_tier1(model, pmStorage, dbStorage, logger);

                case PMType.latam_pm_v1:
                    return new pm_processor_latam_v1(model, pmStorage, dbStorage, logger);

                default:
                    throw new NotImplementedException();
            }
            
        }
        #endregion
    }

}
