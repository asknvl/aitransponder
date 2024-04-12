using botplatform.Models.bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.rest
{
    public interface IPushObservable
    {
        void Add(IPushObserver observer);
        void Remove(IPushObserver observer);        
    }
}
