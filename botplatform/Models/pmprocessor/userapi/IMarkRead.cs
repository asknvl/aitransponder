using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.pmprocessor.userapi
{
    public interface IMarkRead
    {
        Task MarkAsRead(long id);
    }
}
