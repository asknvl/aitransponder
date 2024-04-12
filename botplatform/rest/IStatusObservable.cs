using botplatform.Models.bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.rest
{
    public interface IStatusObservable
    {
        void Add(IStatusObserver observer);
        void Remove(IStatusObserver observer);
    }
}
