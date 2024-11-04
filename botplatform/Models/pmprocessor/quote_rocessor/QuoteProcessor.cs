using asknvl.storage;
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
        IStorage<Dictionary<long, List<quoteInfo>>> quotesStorage;
        object quoteLocker = new object();  
        #endregion

        public QuoteProcessor(string geotag) {            
            quotesStorage = new Storage<Dictionary<long, List<quoteInfo>>>($"{geotag}.json", "quotes", quotes);
            quotes = quotesStorage.load();
        }

        #region public
        public void Add(long tg_id, string response_code, int message_id)
        {
            lock (quoteLocker)
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

                quotesStorage.save(quotes);
            }
        }

        public void Remove(long tg_id)
        {
            lock (quoteLocker)
            {
                if (quotes.ContainsKey(tg_id)) {
                    quotes.Remove(tg_id);
                    quotesStorage.save(quotes);
                }
            }
        }

        public (int, bool) Get(long tg_id, string response_code)
        {
            int res = -1;
            bool is_used = false;

            if (quotes.ContainsKey(tg_id))
            {
                var found = quotes[tg_id].FirstOrDefault(q => q.response_code.Equals(response_code));
                if (found != null)
                {
                    lock (quoteLocker)
                    {
                        res = found.message_id;
                        is_used = found.is_used;
                        found.is_used = true;

                        quotesStorage.save(quotes);
                    }                   
                }                
            }
            return (res, is_used);
        }
        #endregion

        #region internal
        class quoteInfo
        {
            public string response_code { get; set; }
            public int message_id { get; set; }
            public bool is_used { get; set; }
        }
        #endregion
    }
}
