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

        public User createUserIfNeeded(string geotag,
                                       long tg_id,
                                       string bcId,
                                       string? fn,
                                       string? ln,
                                       string? un)
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
                    return found;
                } else
                {
                    User user = new User(geotag, tg_id, bcId, fn: fn, ln: ln, un: un);
                    context.Users.Add(user);
                    context.SaveChanges();
                    return user;
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

        public void updateUser(string geotag,
                               long tg_id,
                               bool? ai_on = null,
                               string? ai_off_code = null)
        {
            lock (lockObject)
            {
                var found = context.Users.FirstOrDefault(u => u.geotag.Equals(geotag) && u.tg_id == tg_id);
                if (found != null)
                {
                    if (ai_on != null)
                    {
                        found.ai_on = (bool)ai_on;
                        if (ai_on == false)
                            found.ai_off_time = DateTime.UtcNow;
                    }

                    if (!string.IsNullOrEmpty(ai_off_code))
                        found.ai_off_code = ai_off_code;

                    context.SaveChanges();
                }
            }
        }        
    }
}
