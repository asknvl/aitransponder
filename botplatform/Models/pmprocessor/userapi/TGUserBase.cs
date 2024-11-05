
using asknvl.logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TL;
using WTelegram;

namespace asknvl
{
    public abstract class TGUserBase : ITGUser
    {
        #region vars
        protected Client user;
        readonly ManualResetEventSlim verifyCodeReady = new();
        string verifyCode;
        protected ILogger logger;

        protected Dictionary<long, ChatBase> chats = new();
        protected Dictionary<long, User> users = new();

        //protected Messages_Chats chats;
        protected List<Messages_ChatFull> fullChats = new();
        protected Messages_Dialogs dialogs;
        string dir = Path.Combine("C:", "userpool");

        protected List<long> acceptedIds = new();
        protected UpdateManager updateManager;

        #endregion

        #region properties        
        public string api_id { get; set; }
        public string api_hash { get; set; }
        public string phone_number { get; set; }
        public long tg_id { get; set; }
        public string? username { get; set; }
        public string _2fa_password { get; }
        //public bool is_active { get; set; }
        public DropStatus status { get; set; }
        public bool test_mode { get; set; }
        public bool is_subscription_running { get; set; }
        #endregion

        public TGUserBase(string api_id, string api_hash, string phone_number, string _2fa_password, ILogger logger)
        {
            this.api_id = api_id;
            this.api_hash = api_hash;
            this.phone_number = phone_number;
            this.logger = logger;
            this._2fa_password = _2fa_password;
            this.status = DropStatus.stopped;            
        }

        #region protected
        protected string Config(string what)
        {


            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            switch (what)
            {
                case "api_id": return api_id;
                case "api_hash": return api_hash;
                case "session_pathname": return $"{dir}/{phone_number}.session";
                case "phone_number": return phone_number;
                case "verification_code":
                    setStatus(DropStatus.verification);
                    VerificationCodeRequestEvent?.Invoke(this);
                    verifyCodeReady.Reset();
                    verifyCodeReady.Wait();
                    return verifyCode;
                //case "first_name": return "Stevie";
                //case "last_name": return "Voughan";
                case "password": return _2fa_password;
                default: return null;
            }
        }

        abstract protected Task processUpdate(TL.Update update);
        #endregion

        #region private
        private async Task User_OnUpdate(Update update)
        {
            await processUpdate(update);
        }

        //private async Task OnUpdate(UpdatesBase updates)
        //{

        //    updates.CollectUsersChats(users, chats);

        //    if (updates is UpdateShortMessage usm && !users.ContainsKey(usm.user_id))
        //        (await user.Updates_GetDifference(usm.pts - usm.pts_count, usm.date, 0)).CollectUsersChats(users, chats);
        //    else if (updates is UpdateShortChatMessage uscm && (!users.ContainsKey(uscm.from_id) || !chats.ContainsKey(uscm.chat_id)))
        //        (await user.Updates_GetDifference(uscm.pts - uscm.pts_count, uscm.date, 0)).CollectUsersChats(users, chats);

        //    //if (arg is not UpdatesBase updates)
        //    //    return;

        //    foreach (var update in updates.UpdateList)
        //    {
        //        await processUpdate(update);
        //    }
        //}      
        #endregion

        #region public
        public virtual Task Start()
        {
            if (status == DropStatus.active)
                return Task.CompletedTask;

            User usr = null!;

            return Task.Run(async () =>
            {
                try
                {
                    user = new Client(Config);
                    logger.inf(phone_number, $"Starting...");

                    updateManager = user.WithUpdateManager(User_OnUpdate);

                    usr = await user.LoginUserIfNeeded();
                    username = usr.username;
                    tg_id = usr.ID;

                    var dialogs = await user.Messages_GetDialogs(limit: 100);
                    dialogs.CollectUsersChats(updateManager.Users, updateManager.Chats);

                    //chats = await user.Messages_GetAllChats();

                    //dialogs = await user.Messages_GetAllDialogs();
                    //dialogs.CollectUsersChats(users, chats);

                    //using (var db = new DataBaseContext())
                    //{
                    //    acceptedIds = db.Channels.Select(c => c.tg_id).ToList();
                    //}

                    //user.OnUpdate -= OnUpdate;
                    //user.OnUpdate += OnUpdate;

                    setStatus(DropStatus.active);

                }
                catch (RpcException ex)
                {
                    processRpcException(ex);
                }
                catch (Exception ex)
                {
                    logger.err(phone_number, $"Starting fail: {ex.Message}");
                    await Stop();
                }

            }).ContinueWith(async t =>
            {
                //StartedEvent?.Invoke(this, is_active);

                if (status == DropStatus.active)
                {
                    await user.Account_UpdateStatus(false);
                    logger.inf(phone_number, $"Started OK");
                }
            });
        }

        public void SetVerifyCode(string code)
        {
            verifyCode = code;
            verifyCodeReady.Set();
        }

        public virtual async Task Stop()
        {
            await Task.Run(() =>
            {
                verifyCodeReady.Set();
                user?.Dispose();                
                //StoppedEvent?.Invoke(this);                
                setStatus(DropStatus.stopped);
            });
        }
        #endregion

        #region protected
        protected void SendChannelMessageViewedEvent(long channel_id, uint counter)
        {
            ChannelMessageViewedEvent?.Invoke(channel_id, counter);
        }

        protected void setStatus(DropStatus _status)
        {

            if (status != _status)
            {
                status = _status;
                StatusChangedEvent?.Invoke(this, status);                
            }
        }

        protected void processRpcException(RpcException ex)
        {
            switch (ex.Message)
            {
                case "PHONE_NUMBER_BANNED":
                    setStatus(DropStatus.banned);
                    user.Dispose();
                    break;

                case "SESSION_REVOKED":
                case "AUTH_KEY_UNREGISTERED":
                    user.Dispose();
                    setStatus(DropStatus.revoked);
                    break;

            }
        }

        protected void onBusinessBotToggle(long tg_user_id, bool state)
        {
            BusinessBotToggleEvent?.Invoke(tg_user_id, state);
        }

        protected void onMessagesDeletedEvent(int[] messages)
        {
            MessagesDeletedEvent?.Invoke(messages); 
        }
        #endregion    

        #region events
        public event Action<ITGUser> VerificationCodeRequestEvent;
        public event Action<string, long, string> ChannelAddedEvent;        
        public event Action<long, uint> ChannelMessageViewedEvent;
        public event Action<string> _2FAPasswordChanged;
        public event Action<ITGUser, DropStatus> StatusChangedEvent;

        public event Action<long, bool> BusinessBotToggleEvent;
        public event Action<int[]> MessagesDeletedEvent;
        #endregion
    }
}
