using botplatform.Models.pmprocessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.rest
{
    public interface IAutoReplyObservable
    {
        void Add(IAutoReplyObserver observer);
        void Remove(IAutoReplyObserver observer);
    }
}
