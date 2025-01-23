using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.pmprocessor.db_storage
{
    public interface IDBStorage
    {
        (User, bool) createUserIfNeeded_AI(string geotag, long tg_id, string bcId, string? fn, string? ln, string? un);
        (User, bool) createUserIfNeeded_TRCK(string geotag, long tg_id, string bcId, string? fn, string? ln, string? un);
        User getUser(string geotag, long tg_id);
        User? getUser(string geotag, int message_id);
        List<User> getAIUsers(string geotag);
        void updateUserData(string geotag, long tg_id,
                        bool? ai_on = null,
                        string? ai_off_code = null,
                        int? first_msg_id = null,
                        bool? is_reply = null,
                        bool? chat_deleted = null,
                        bool? was_autoreply = null,
                        int? message_counter = null);
    }    
}
