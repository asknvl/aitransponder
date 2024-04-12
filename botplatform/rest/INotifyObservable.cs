using botplatform.Models.bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.rest
{
    public interface INotifyObservable
    {
        void Add(INotifyObserver observer);
        void Remove(INotifyObserver observer);
    }
}
