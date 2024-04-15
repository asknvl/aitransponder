using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static asknvl.server.TGBotFollowersStatApi;

namespace botplatform.Models.server
{
    public class AIServer : IAIserver
    {
        #region const
        string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyIjoiYm90MDEiLCJpYXQiOjE3MTMwNzgyMzJ9.ibCadqPOLluTcpp5_QPTlKc_AZMvDNkcN_2zSPzJdOM";
        #endregion

        #region vars
        string url;
        ServiceCollection serviceCollection;
        IHttpClientFactory httpClientFactory;
        HttpClient httpClient;
        #endregion

        public AIServer(string url)
        {
            this.url = url;
            serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var services = serviceCollection.BuildServiceProvider();
            httpClientFactory = services.GetRequiredService<IHttpClientFactory>();

            httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddHttpClient();
        }

        #region public
        class messageDto
        {
            public string message { get; set; }
            public long tg_user_id { get; set; }
            public string source { get; set; }
        }


        public async Task SendToAI(string geotag, long tg_user_id, string text)
        {
            var addr = $"{url}/api/message";


            messageDto msg = new messageDto()
            {
                source = geotag,
                tg_user_id = tg_user_id,
                message = text
            };

            var json = JsonConvert.SerializeObject(msg, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync(addr, data);                
                //var result = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new Exception($"SendToAI {ex.Message}");
            }
        }
        #endregion
    }
}
