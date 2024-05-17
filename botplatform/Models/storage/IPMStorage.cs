using botplatform.Models.pmprocessor;
using System.Collections.Generic;

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
