using Avalonia.X11;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace asknvl.server
{
    public class TGBotFollowersStatApi : ITGBotFollowersStatApi
    {
        #region const
        string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6MjksImxldHRlcl9pZCI6IllCIiwiaWF0IjoxNjU5MTk2Nzc1fQ.8qzVaYVky9m4m3aa0f8mMFI6mk3-wyhAiSZVmiHKwmg";
        #endregion

        #region vars
        string url;
        ServiceCollection serviceCollection;
        IHttpClientFactory httpClientFactory;
        HttpClient httpClient;
        #endregion

        public TGBotFollowersStatApi(string url)
        {
            this.url = url;
            serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var services = serviceCollection.BuildServiceProvider();
            httpClientFactory = services.GetRequiredService<IHttpClientFactory>();
            httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        #region private
        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddHttpClient();
        }
        #endregion

        #region public
        public class followersDto
        {
            public List<Follower> users { get; set; }
            public followersDto(List<Follower> followers)
            {
                this.users = followers;
            }
        }

        public async Task UpdateFollowers(List<Follower> followers)
        {
            var addr = $"{url}/v1/telegram";
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            followersDto flwrs = new followersDto(followers);
            var json = JsonConvert.SerializeObject(flwrs);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync(addr, data);
                var result = await response.Content.ReadAsStringAsync();
                var jres = JObject.Parse(result);
                bool res = jres["success"].ToObject<bool>();
                if (!res)
                    throw new Exception($"success=false");

            }
            catch (Exception ex)
            {
                throw new Exception($"UpdateFollowers {ex.Message}");
            }
        }

        public class tgFollowerStatusResponse
        {
            public bool success { get; set; }
            public string uuid { get; set; }
            public string start_params { get; set; }
            public string status_code { get; set; }
            public double amount { get; set; }
            public double target_amount { get; set; }
            public double amount_local_currency { get; set; }
            public double target_amount_local_currency { get; set; }
            public string player_id { get; set; }

            public override string ToString()
            {
                return $"{success} {uuid} {start_params} {status_code} {amount} {target_amount} {amount_local_currency} {target_amount_local_currency} {player_id}";
            }
        }

        public async Task<(string, string)> GetFollowerState(string geotag, long id)
        {
            string status;
            string uuid;

            var addr = $"{url}/v1/telegram/telegramBotStatus?userID={id}&geo={geotag}";
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await httpClient.GetAsync(addr);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                var resp = JsonConvert.DeserializeObject<tgFollowerStatusResponse>(result);

                if (resp.success)
                {
                    uuid = resp.uuid;
                    status = resp.status_code;
                }
                else
                    throw new Exception($"sucess=false");
            }
            catch (Exception ex)
            {
                throw new Exception($"GetFollowerState {ex.Message}");
            }

            return (uuid, status);
        }

        public async Task<tgFollowerStatusResponse> GetFollowerStateResponse(string geotag, long id)
        {
            tgFollowerStatusResponse res = null;

            var addr = $"{url}/v1/telegram/telegramBotStatus?userID={id}&geo={geotag}";
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await httpClient.GetAsync(addr);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                var resp = JsonConvert.DeserializeObject<tgFollowerStatusResponse>(result);

                if (resp.success)
                {
                    res = resp;
                }
                else
                    throw new Exception($"sucess=false");
            }
            catch (Exception ex)
            {
                throw new Exception($"GetFollowerState {ex.Message}");
            }

            return res;
        }

        public class pushSlipDto
        {
            public int notification_id { get; set; }
            public string status { get; set; }
        }
        public async Task SlipPush(int notification_id, bool isok)
        {
            var addr = $"{url}/v1/telegram/botNotifications";
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            pushSlipDto slip = new pushSlipDto()
            {
                notification_id = notification_id,
                status = (isok) ? "SUCCESS" : "ERROR"
            };
            var json = JsonConvert.SerializeObject(slip);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PutAsync(addr, data);
                var result = await response.Content.ReadAsStringAsync();
                var jres = JObject.Parse(result);
                bool res = jres["success"].ToObject<bool>();
                if (!res)
                    throw new Exception($"success=false");

            }
            catch (Exception ex)
            {
                throw new Exception($"SlipPush {ex.Message}");
            }
        }

        public async Task SetFollowerRegistered(string player_id, string uuid)
        {

            DateTime currentTime = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();

            var addr = $"{url.Replace("4000", "4003")}/v1/telegram/postbacks?subid=xxx&status=lead&timestamp={unixTime}&type=manual&sub_id_15={player_id}&from=1win.run.RS&uuid={uuid}";
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await httpClient.GetAsync(addr);
                response.EnsureSuccessStatusCode();
                //var result = await response.Content.ReadAsStringAsync();
                //var resp = JsonConvert.DeserializeObject<bool>(result);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                }
                else
                    throw new Exception($"sucess=false");
            }
            catch (Exception ex)
            {
                throw new Exception($"SetFollowerRegistered {ex.Message}");
            }
        }

        public async Task SetFollowerMadeDeposit(string uuid, long player_id, uint sum)
        {

#if DEBUG
#else
#endif
            DateTime currentTime = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();

            var addr = $"{url.Replace("4000", "4003")}/v1/telegram/postbacks?subid=xxx&amount={sum}&status=sale&tid=xxx&timestamp={unixTime}&type=promo&sub_id_15={player_id}&from=manual&uuid={uuid}";
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await httpClient.GetAsync(addr);
                response.EnsureSuccessStatusCode();
                //var result = await response.Content.ReadAsStringAsync();
                //var resp = JsonConvert.DeserializeObject<bool>(result);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                }
                else
                    throw new Exception($"sucess=false");
            }
            catch (Exception ex)
            {
                throw new Exception($"SetFollowerMadeDeposit {ex.Message}");
            }
        }

        #endregion

        public class getIdUserInfoDto
        {
            public string? geo { get; set; }
            public string? tg_user_id { get; set; }
            public string? subscribe_date { get; set; }
            public string? player_id { get; set; }
            public string? uuid { get; set; }
            public string? start_params { get; set; }
            public string? username { get; set; }
            public string? firstname { get; set; }
            public string? lastname { get; set; }
            public string? current_status_code { get; set; }
            public int? current_status_iteration { get; set; }
            public int? last_rd_iteration { get; set; }
            public double? current_status_amount { get; set; }
            public bool lead { get; set; }
            public string? lead_date { get; set; }
            public bool fd { get; set; }
            public string? fd_date { get; set; }
            public double? sum_fd { get; set; }
            public bool rd { get; set; }
            public int? total_rd_ammount { get; set; }
            public string? last_rd_date { get; set; }
            public double? sum_rd { get; set; }
            public double? sum_amount { get; set; }
        }

        public class getIdResponseDto
        {
            public bool success { get; set; }
            public List<getIdUserInfoDto> data { get; set; }
        }

        public async Task<List<getIdUserInfoDto>> GetUserInfoByTGid(long tg_id)
        {
            List<getIdUserInfoDto> res = null;

            var addr = $"{url}/v1/telegram/telegramStatus?tgUserID={tg_id}";            

            try
            {
                var response = await httpClient.GetAsync(addr);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    throw new NotFoundException("Пользователь не найден");

                response.EnsureSuccessStatusCode();                

                var result = await response.Content.ReadAsStringAsync();
                var resp = JsonConvert.DeserializeObject<getIdResponseDto>(result);

                if (resp.success)
                {
                    res = resp.data;
                }
                else
                    throw new Exception($"sucess=false");
            }
            catch (NotFoundException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"GetFollowerState {ex.Message}");
            }

            return res;
        }

        public async Task<List<getIdUserInfoDto>> GetUserInfoByPlayerId(string player_id)
        {
            List<getIdUserInfoDto> res = null;

            var addr = $"{url}/v1/telegram/telegramStatus?playerID={player_id}";
            

            try
            {
                var response = await httpClient.GetAsync(addr);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    throw new NotFoundException("Пользователь не найден");

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();
                var resp = JsonConvert.DeserializeObject<getIdResponseDto>(result);

                if (resp.success)
                {
                    res = resp.data;
                }
                else
                    throw new Exception($"sucess=false");
            }
            catch (NotFoundException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"GetFollowerState {ex.Message}");
            }

            return res;
        }

        public class subscriptionResponseDto
        {
            public bool success { get; set; }
            public List<subscriptionDto> data { get; set; } = new();
        }

        public class subscriptionDto
        {
            public string geolocation { get; set; }
            public bool is_subscribed { get; set; }
            public string? subscribe_date { get; set; }
        }
        public async Task<List<subscriptionDto>> GetFollowerSubscriprion(string geotag, long tg_id)
        {
            List<subscriptionDto> res = new();

            var addr = $"{url}/v1/telegram/telegramUser?code={geotag}&tg_user_id={tg_id}";

            try
            {
                var response = await httpClient.GetAsync(addr);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                var resp = JsonConvert.DeserializeObject<subscriptionResponseDto>(result);

                if (resp.success)
                {
                    res = resp.data;
                }
                else
                    throw new Exception($"sucess=false");
            }
            catch (Exception ex)
            {
                throw new Exception($"GetFollowerSubscriprion {ex.Message}");
            }

            return res;
        }


        class leadDistributeDto
        {
            public long tg_user_id { get; set; }
            public string source { get; set; }
            public string assignment_type { get; set; }
        }

        public async Task LeadDistributeRequest(long tg_id, string geotag, AssignmentTypes atype)
        {
            var addr = $"https://app.flopasda.site/v1/telegram/userMessages/distribute";

            leadDistributeDto lead = new()
            {
                tg_user_id = tg_id,
                source = geotag,
                assignment_type = atype.ToString()
            };

            var json = JsonConvert.SerializeObject(lead, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PutAsync(addr, data);
                var result = await response.Content.ReadAsStringAsync();
                var jres = JObject.Parse(result);
                bool res = jres["success"].ToObject<bool>();
                if (!res)
                    throw new Exception($"success=false");
            }
            catch (Exception ex)
            {
                throw new Exception($"LeadDistributeRequest {geotag} {tg_id} {atype} {ex.Message}");
            }
        }

        public class tgUserStateDto
        {
            [JsonProperty]
            public long tg_user_id { get; set; }
            [JsonProperty]
            public string tg_geolocation { get; set; }
            [JsonProperty]
            public bool? is_user_send_msg { get; set; } = null;
            [JsonProperty]
            public bool? is_user_msg_processed { get; set; } = null;
            [JsonProperty]
            public bool? is_user_get_auto_answer { get; set; } = null;
            [JsonProperty]
            public string? username { get; set; } = null;
            [JsonProperty]
            public string? firstname { get; set; } = null;
            [JsonProperty]
            public string? lastname { get; set; } = null;
        }
        public class tgUsersStatesDto
        {
            [JsonProperty]
            public List<tgUserStateDto> users { get; set; } = new();
        }

        public virtual async Task MarkFollowerMadeFeedback(string geotag, long id, string? fn = null, string? ln = null, string? un = null)
        {

            tgUsersStatesDto feedback = new();
            feedback.users.Add(new tgUserStateDto()
            {
                tg_user_id = id,
                tg_geolocation = geotag,
                is_user_send_msg = true,

                firstname = fn,
                lastname = ln,
                username = un,
            });

            string json = JsonConvert.SerializeObject(feedback, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            var addr = $"{url}/v1/telegram/userByGeo";
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync(addr, data);
                var result = await response.Content.ReadAsStringAsync();
                var jres = JObject.Parse(result);
                bool res = jres["success"].ToObject<bool>();
                if (!res)
                    throw new Exception($"success=false");

            }
            catch (Exception ex)
            {
                throw new Exception($"MarkFollowerMadeFeedback {ex.Message}");
            }

        }

        public virtual async Task MarkFollowerWasReplied(string geotag, long id)
        {
            tgUsersStatesDto reply = new();
            reply.users.Add(new tgUserStateDto()
            {
                tg_user_id = id,
                tg_geolocation = geotag,
                is_user_msg_processed = true
            });

            string json = JsonConvert.SerializeObject(reply, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });


            var addr = $"{url}/v1/telegram/userByGeo";
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync(addr, data);
                var result = await response.Content.ReadAsStringAsync();
                var jres = JObject.Parse(result);
                bool res = jres["success"].ToObject<bool>();
                if (!res)
                    throw new Exception($"success=false");

            }
            catch (Exception ex)
            {
                throw new Exception($"MarkFollowerWasReplied {ex.Message}");
            }
        }
    }


}
