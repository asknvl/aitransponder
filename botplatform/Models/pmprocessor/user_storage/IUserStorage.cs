using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.pmprocessor.user_storage
{
    public interface IUserStorage
    {
        User createUserIfNeeded(long tg_id, bool is_active);
        void updateUser(long tg_id, bool? is_active = null);
        void save();
        void load();
    }

    public class User
    {
        public bool is_active { get; set; }
    }
}
