﻿using asknvl.logger;
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
        IPMStorage pmStorage;
        ILogger logger;
        #endregion

        public PmFactory(IPMStorage pmStorage, ILogger logger)
        {            
            this.pmStorage = pmStorage;
            this.logger = logger;
        }

        #region public
        public PMBase Get(PmModel model, PMType type)
        {
            switch (type)
            {
                case PMType.strategy_ind_pm:
                    return new pm_processor_v0(model, pmStorage, logger);

                default:
                    throw new NotImplementedException();
            }
            
        }
        #endregion
    }

    public enum PMType
    {
        strategy_ind_pm,
        hack_ind_supp,
        hack_ind_pm        
    }
}
