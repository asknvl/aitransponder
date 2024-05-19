using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.pmprocessor.db_storage
{
    public interface IDBStorage
    {
        (User, bool) createUserIfNeeded(string geotag, long tg_ids, string? fn, string? ln, string? un, string bcId);
        User getUser(string geotag, long tg_id);
        void updateUser(string geotag, long tg_id, bool? ai_on = null, string? ai_off_code = null);
    }    
}
