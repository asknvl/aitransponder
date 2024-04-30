using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.pmprocessor.message_queue
{
    public class MessageHistory
    {
        #region vars
        Dictionary<long, UserHistory> history = new Dictionary<long, UserHistory>();
        object lockObject = new object();   
        #endregion

        #region public
        public void Add(MessageFrom from, long tg_user_id, string text)
        {

            lock (lockObject)
            {
                bool exists = history.ContainsKey(tg_user_id);
                if (exists)
                {
                    history[tg_user_id].Add(from, text);
                }
                else
                {

                    history.Add(tg_user_id, new UserHistory());
                    history[tg_user_id].Add(from, text);


                }
            }
        }

        public Dictionary<long, UserHistory> Get()
        {
            Dictionary<long, UserHistory> res = new();
            lock (lockObject) {
                res = history.Where(m => m.Value.IsUpdated).ToDictionary(p => p.Key, p => p.Value);
            }
            return res;
        }
        #endregion
    }

  

    public class UserHistory
    {
        #region const
        int capacity = 30;
        #endregion

        #region vars
        List<HistoryItem> userHistory = new();
        object lockObject = new object();
        #endregion

        #region properties
        public bool IsUpdated { get; set; }
        #endregion

        public UserHistory() { 
        }

        #region public
        public void Add(MessageFrom from, string text)
        {
            lock (lockObject)
            {
                if (userHistory.Count == capacity)                
                    userHistory.RemoveAt(0);      
                userHistory.Add(new HistoryItem(from, text));
                if (from == MessageFrom.Lead)
                    IsUpdated = true;
            }
        }

        public List<HistoryItem> Get()
        {
            List<HistoryItem> tmp;
            lock (lockObject)
            {
                tmp = userHistory.ToList();
                IsUpdated = false;
            }
            return tmp;
        }
        #endregion
    }

    public class HistoryItem
    {
        public string role { get; set; }
        public string content { get; set; }

        public HistoryItem(MessageFrom from, string text)
        {
            role = (from == MessageFrom.Lead) ? "user" : "assistant";
            content = text;
        }
    }

    public enum MessageFrom
    {
        Lead,
        PM
    }
}
