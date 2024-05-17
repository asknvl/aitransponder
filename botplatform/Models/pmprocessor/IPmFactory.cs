using asknvl.logger;
using botplatform.Model.bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.pmprocessor
{
    public interface IPmFactory
    {
        PMBase Get(PmModel model, PMType type);
    }
}
