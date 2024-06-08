using botplatform.Models.pmprocessor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.rest
{
    public class AutoReplyRequestProcessor : IRequestProcessor, IAutoReplyObservable
    {
        #region vars
        List<IAutoReplyObserver> autoReplyObservers = new List<IAutoReplyObserver>();
        #endregion
        public void Add(IAutoReplyObserver observer)
        {
            if (observer != null && !autoReplyObservers.Contains(observer))
                autoReplyObservers.Add(observer);
        }

        public void Remove(IAutoReplyObserver observer)
        {
            throw new NotImplementedException();
        }

        public Task<(HttpStatusCode, string)> ProcessRequest()
        {
            throw new NotImplementedException();
        }

        public async Task<(HttpStatusCode, string)> ProcessRequestData(string data)
        {
            HttpStatusCode code = HttpStatusCode.BadRequest;
            string responseText = "Incorrect parameters";

            try
            {

                var _ = Task.Run(async () => { 

                    var replydata = JsonConvert.DeserializeObject<autoReplyInfoDto>(data);
                    var observer = autoReplyObservers.FirstOrDefault(o => o.GetChannelTag().Equals(replydata.source));
                    if (observer != null)
                    {
                        try
                        {
                            await observer.AutoReply(replydata.source, replydata.tg_user_id, replydata.response_code, replydata.message);
                        } catch (Exception ex)
                        {                            
                        }
                    }
                });

                await Task.CompletedTask;
                code = HttpStatusCode.OK;
                responseText = $"{code.ToString()}";

            }
            catch (Exception ex)
            {

            }

            return (code, responseText);
        }
    }

    public class autoReplyInfoDto
    {
        public string source { get; set; }
        public long tg_user_id { get; set; }
        public string response_code { get; set; }
        public string? message { get; set; }
    }
}
