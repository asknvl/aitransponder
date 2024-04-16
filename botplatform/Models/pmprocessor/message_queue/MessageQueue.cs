using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;

namespace botplatform.Models.pmprocessor.message_queue
{
    public class MessageQueue
    {
        #region const
        int clear_period = 12 * 60 * 60 * 1000;
        #endregion

        #region vars
        Dictionary<long, string> messages = new Dictionary<long, string>();
        object lockObj = new object();
        System.Timers.Timer clearTimer;
        #endregion

        public MessageQueue() {
            clearTimer = new System.Timers.Timer();
            clearTimer.Interval = clear_period;
            clearTimer.AutoReset = true;
            clearTimer.Elapsed += ClearTimer_Elapsed;
        }

        private void ClearTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            foreach (var item in messages)
            {
                if (item.Value.Equals(""))
                {
                    lock (lockObj)
                    {
                        messages.Remove(item.Key);
                    }
                }
            }            
        }

        #region public
        public void Add(long id, string message)
        {
            lock (lockObj)
            {
                if (messages.ContainsKey(id))
                {
                    if (messages[id] == "")
                        messages[id] += $"{message}";
                    else
                        messages[id] += $"|{message}";
                }
                else
                {
                    messages.Add(id, $"{message}");
                }
            }
        }

        public string Get(long id)
        {
            string res = "";

            if (messages.ContainsKey(id))
            {
                res = messages[id];
                lock (lockObj)
                {
                    messages[id] = "";
                }
            }
            return res;
        }       
        
        public Dictionary<long, string> GetMessages()
        {
            Dictionary<long, string> res = new Dictionary<long, string>();
            lock (lockObj)
            {
                res = messages.Where(m => !string.IsNullOrEmpty(m.Value)).ToDictionary(p => p.Key, p => p.Value);
                foreach (var item in res)
                {
                    messages[item.Key] = "";
                }

            }
            return res;
        }
        #endregion
    }
}
