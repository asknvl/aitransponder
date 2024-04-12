using asknvl.logger;
using botplatform.Model.bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.pmprocessor
{
    public class PmFactory : IPmFactory
    {
        public PMBase Get(PmModel model, ILogger logger)
        {
            return new pm_processor_v0(model, logger);
        }
    }
}
