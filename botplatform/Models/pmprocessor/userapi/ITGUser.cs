﻿using System;
using System.Threading.Tasks;

namespace asknvl
{
    public interface ITGUser
    {        
        public string api_id { get; set; }     
        public string api_hash { get; set; }                     
        string phone_number { get; set; }
        public string _2fa_password { get; }
        long tg_id { get; set; }
        //bool is_active { get; set; }
        DropStatus status { get; set; }
        bool is_subscription_running { get; set; }  
        bool test_mode { get; set; }

        Task Start();        
        Task Stop();
        void SetVerifyCode(string code);
        void ClearSession();

        event Action<ITGUser> VerificationCodeRequestEvent;        
        event Action<ITGUser, DropStatus> StatusChangedEvent;
        
        event Action<long, uint> ChannelMessageViewedEvent;
        public event Action<string> _2FAPasswordChanged;
    }

    public enum DropStatus
    {
        stopped,
        active,        
        verification,
        revoked,
        banned,
        removed,
        subscription,
        idled
    }   
}
