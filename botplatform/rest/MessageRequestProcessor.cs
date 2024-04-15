using botplatform.Models.bot;
using botplatform.Models.pmprocessor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace botplatform.rest
{
    public class MessageRequestProcessor : IRequestProcessor, IMessageObservable
    {
        #region vars
        List<IMessageObserver> messageObservers = new List<IMessageObserver>();
        #endregion

        #region public
        public void Add(IMessageObserver observer)
        {
            if (!messageObservers.Contains(observer))
                messageObservers.Add(observer);
        }

        public async Task<(HttpStatusCode, string)> ProcessRequestData(string data)
        {            
            HttpStatusCode code = HttpStatusCode.BadRequest;
            string responseText = "Incorrect parameters";

            try
            {
                await Task.Run(async () =>
                {

                    var messageData = JsonConvert.DeserializeObject<MessageDto>(data);
                    int cntr = 0;


                    var messageObserver = messageObservers.FirstOrDefault(o => o.GetGeotag().Equals(messageData.source));
                    if (messageObserver != null)
                    {
                        var _ = Task.Run(async () => {

                            await messageObserver.SendMessage(
                                                            messageData.source,
                                                            messageData.tg_user_id,
                                                            messageData.response_code,
                                                            messageData.message);
                        
                        });
                    }

                    //Task.Run(async () =>
                    //{
                    //    foreach (var item in pushdata.data)
                    //    {
                    //        var geotag = item.geotag;
                    //        var observer = pushObservers.FirstOrDefault(o => o.GetGeotag().Equals(geotag));
                    //        if (observer != null) { 
                    //            try
                    //            {
                    //                bool res = await observer.Push(item.tg_id, item.code, item.notification_id);
                    //                if (res)
                    //                    cntr++;
                    //            }
                    //            catch (Exception ex)
                    //            {
                    //            }
                    //        }
                    //    }
                    //    });

                    code = HttpStatusCode.OK;
                    responseText = $"{code.ToString()}";

                    //responseText = JsonConvert.SerializeObject(inactiveUsers);
                });

            }
            catch (Exception ex)
            {

            }

            return (code, responseText);

        }

        public void Remove(IMessageObserver observer)
        {
            messageObservers.Remove(observer);
        }
        #endregion
    }

    public class MessageDto
    {
        public long tg_user_id { get; set; }
        public string source { get; set; }
        public string response_code { get; set; }
        public string message { get; set; }
    }
}
