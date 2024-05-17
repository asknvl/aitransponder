using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.pmprocessor.user_storage
{
    public interface IUserStorage
    {
    }

    public class User
    {
        public bool is_active { get; set; }
    }
}
