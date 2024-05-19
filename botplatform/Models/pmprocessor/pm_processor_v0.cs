using asknvl.logger;
using botplatform.Models.pmprocessor.db_storage;
using botplatform.Models.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.pmprocessor
{
    public class pm_processor_v0 : PMBase
    {
        public pm_processor_v0(PmModel model, IPMStorage pmStorage, IDBStorage dbStorage, ILogger logger) : base(model, pmStorage, dbStorage, logger)
        {
        }
    }
}
