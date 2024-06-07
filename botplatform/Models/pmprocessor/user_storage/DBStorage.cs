using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.pmprocessor.db_storage
{
    public class DBStorage : IDBStorage
    {

        #region vars
        ApplicationContext context;
        object lockObject = new object();
        #endregion

        public DBStorage(ApplicationContext context)
        {
            this.context = context; 
        }

        public (User, bool) createUserIfNeeded(string geotag,
                                               long tg_id,
                                               string bcId,
                                               string? fn,
                                               string? ln,
                                               string? un,
                                               bool? ai_on = false)
        {
            lock (lockObject)
            {
                var found = context.Users.FirstOrDefault(u => u.geotag.Equals(geotag) && u.tg_id == tg_id);
                if (found != null)
                {
                    if (!string.IsNullOrEmpty(bcId) && found.bcId != bcId) { 
                        found.bcId = bcId;
                        context.SaveChanges();
                    }
                    return (found, false);
                } else
                {
                    User user = new User(geotag, tg_id, bcId, fn: fn, ln: ln, un: un);

                    if (ai_on.HasValue)
                        user.ai_on = ai_on.Value;

                    context.Users.Add(user);
                    context.SaveChanges();
                    return (user, true);
                }
            }
        }

        public User getUser(string geotag, long tg_id)
        {            
            lock (lockObject)
            {
                var found = context.Users.FirstOrDefault(u => u.geotag.Equals(geotag) && u.tg_id == tg_id);
                if (found != null)
                    return found;
                else
                    throw new Exception($"No user {geotag} {tg_id} found");
            }
        }

        public void updateUserData(string geotag,
                               long tg_id,
                               bool? ai_on = null,
                               string? ai_off_code = null,
                               int? first_msg_id = null,
                               bool? is_reply = null,
                               bool? chat_deleted = null,
                               bool? was_autoreply = null)
        {
            lock (lockObject)
            {
                bool save = false;

                var found = context.Users.FirstOrDefault(u => u.geotag.Equals(geotag) && u.tg_id == tg_id);
                if (found != null)
                {
                    if (ai_on.HasValue)
                    {
                        found.ai_on = ai_on.Value;
                        if (ai_on == false)
                            found.ai_off_time = DateTime.UtcNow;
                        save = true;
                    }

                    if (!string.IsNullOrEmpty(ai_off_code))
                    {
                        found.ai_off_code = ai_off_code;
                        save = true;
                    }

                    if (first_msg_id.HasValue)
                    {
                        if (!found.first_msg_id.HasValue)
                        {
                            found.first_msg_id = first_msg_id.Value;
                            found.first_msg_rcvd_date = DateTime.UtcNow;
                            save = true;
                        }
                    }

                    if (is_reply.HasValue)
                    {
                        if (!found.is_first_msg_rep)
                        {
                            found.is_first_msg_rep = true;
                            found.first_msg_rep_date = DateTime.UtcNow;
                            save = true;
                        }
                    }

                    if (chat_deleted == true)
                    {
                        found.is_chat_deleted = true;
                        found.chat_delete_date = DateTime.UtcNow;
                        found.first_msg_id = null;
                        found.is_first_msg_rep = false;
                        save = true;
                    }

                    if (was_autoreply == true)
                    {
                        found.was_autoreply = true;
                        found.autoreply_date = DateTime.UtcNow;
                        save = true;
                    }

                    if (save)
                        context.SaveChanges();
                }
            }
        }        
    }
}
