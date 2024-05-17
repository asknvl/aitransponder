using aksnvl.storage;
using asknvl.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.pmprocessor.user_storage
{
    public class JsonUserStorage : IUserStorage
    {
        #region vars   
        Dictionary<long, User> users = new Dictionary<long, User>();
        IStorage<Dictionary<long, User>> userStorage;
        object lockObj = new object();
        #endregion

        public JsonUserStorage(string geotag)
        {
            userStorage = new Storage<Dictionary<long, User>>($"{geotag}", "userstorage", users);
            users = userStorage.load();
        }

        #region private
        //TODO COPY
        #endregion

        #region public
        public User createUserIfNeeded(long tg_id, bool is_active)
        {
            User user = new User();
            user.is_active = is_active;

            if (!users.ContainsKey(tg_id))
            {
                lock (lockObj)
                {
                    users.Add(tg_id, user);
                }
            } else
            {
                lock (lockObj)
                {
                    user.is_active = users[tg_id].is_active;
                }
            }

            return user;
        }

        public void updateUser(long tg_id, bool? is_active = null)
        {
            lock (lockObj)
            {
                if (users.ContainsKey(tg_id))
                {
                    if (is_active != null)
                        users[tg_id].is_active = (bool)is_active;
                }
            }
        }

        public void save()
        {
            lock (lockObj)
            {
                userStorage.save(users);
            }
        }        

        public void load()
        {
            users = userStorage.load();
        }
        #endregion
    }
}
