using botplatform.Model.bot;
using botplatform.Models.pmprocessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.storage
{
    public interface IPMStorage
    {
        void Load();
        void Save();
        void Add(PmModel pm);
        void Remove(string geotag);
        void Update(string geotag, PmModel newPmModel);
        List<PmModel> GetAll();
    }
}
