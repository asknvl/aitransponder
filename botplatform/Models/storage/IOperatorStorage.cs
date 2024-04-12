using botplatform.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.storage
{
    public interface IOperatorStorage
    {
        void Add(string geotag);
        void Add(string geotag, Operator op);
        void Remove(string geotag, Operator op);
        List<Operator> GetAll(string geotag);
        List<BotOperators> GetAll();
        Operator GetOperator(string geotag, long tg_id);

        void Update(List<BotOperators> botOperators);

        public event Action UpdatedEvent;
    }
}
