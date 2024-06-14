using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static asknvl.server.TGBotFollowersStatApi;

namespace asknvl.server
{
    public interface ITGBotFollowersStatApi
    {        
        Task UpdateFollowers(List<Follower> followers);
        Task<(string, string)> GetFollowerState(string geotag, long id);
        Task<tgFollowerStatusResponse> GetFollowerStateResponse(string geotag, long id);
        Task SlipPush(int notification_id, bool isok);
        Task SetFollowerMadeDeposit(string uuid, long player_id, uint sum);
        Task SetFollowerRegistered(string player_id, string uuid);
        Task<List<getIdUserInfoDto>> GetUserInfoByTGid(long tg_id);
        Task<List<getIdUserInfoDto>> GetUserInfoByPlayerId(string player_id);
        Task<List<subscriptionDto>> GetFollowerSubscriprion(string geotag, long tg_id);
        Task LeadDistributeRequest(long tg_id, string geotag, AssignmentTypes type);
        Task MarkFollowerMadeFeedback(string geotag, long id, string? fn = null, string? ln = null, string? un = null);
        Task MarkFollowerWasReplied(string geotag, long id);
        Task MarkChatWasDeleted(string geotag, long id);
    }

    public enum AssignmentTypes
    {
        RD
    }

    public enum DailyPushState
    {
        sent,
        delivered,
        disable
    }
    public class NotFoundException : Exception
    {
        public NotFoundException(string msg) : base(msg) { }
    }

    public class TGFollowersStatException : Exception
    {
        public TGFollowersStatException(string msg) : base(msg) { }
    }
}
