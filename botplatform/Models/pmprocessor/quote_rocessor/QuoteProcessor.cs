using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.pmprocessor.quote_rocessor
{
    public class QuoteProcessor
    {
        #region vars
        Dictionary<long, List<quoteInfo>> quotes = new();
        #endregion

        public QuoteProcessor() {
        }

        #region public
        public void Add(long tg_id, string response_code, int message_id)
        {
            if (quotes.ContainsKey(tg_id))
            {
                var found = quotes[tg_id].FirstOrDefault(q => q.response_code.Equals(response_code));
                if (found == null)
                {
                    quotes[tg_id].Add(new quoteInfo()
                    {
                        response_code = response_code,
                        message_id = message_id
                    });
                }
            } 
            else
            {
                quotes.Add(tg_id, new List<quoteInfo>() { new quoteInfo() { response_code = response_code, message_id = message_id } });
            }
        }

        public int Get(long tg_id, string response_code)
        {
            int res = -1;

            if (quotes.ContainsKey(tg_id))
            {
                var found = quotes[tg_id].FirstOrDefault(q => q.response_code.Equals(response_code));
                if (found != null)
                {
                    res = found.message_id;                    
                }
            }

            return res;
        }
        #endregion

        #region internal
        class quoteInfo
        {
            public string response_code { get; set; }
            public int message_id { get; set; }
        }
        #endregion
    }
}
