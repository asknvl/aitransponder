using botplatform.Models.bot;
using botplatform.Models.pmprocessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.rest
{
    public interface IMessageObservable
    {
        void Add(IMessageObserver observer);
        void Remove(IMessageObserver observer);        
    }
}
