using asknvl.storage;
using botplatform.Models.pmprocessor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.storage.local
{
    public class LocalPmStorage : IPMStorage
    {
        #region vars
        IStorage<List<PmModel>> storage;
        List<PmModel> PmModels = new List<PmModel>();
        #endregion

        public LocalPmStorage()
        {
            var subdir = Path.Combine("pms");
            storage = new Storage<List<PmModel>>("pms.json", subdir, PmModels);
        }

        #region public
        public void Add(PmModel pm)
        {
            var found = PmModels.Any(m => m.geotag.Equals(pm.geotag));
            if (!found)
                PmModels.Add(pm);
            else
                throw new Exception($"Личка с геотегом {pm.geotag} уже существует");

            storage.save(PmModels);
        }

        public void Remove(string geotag)
        {
            var found = PmModels.FirstOrDefault(m => m.geotag.Equals(geotag));
            if (found != null)
                PmModels.Remove(found);

            storage.save(PmModels);
        }

        public List<PmModel> GetAll()
        {
            Load();
            return PmModels;
        }

        public void Load()
        {
            try
            {
                PmModels = storage.load();

            }
            catch (Exception ex)
            {
                throw new Exception("Не удалось загрузить данные");
            }
        }

        public void Update(string geotag, PmModel pm)
        {
            try
            {
                var found = PmModels.FirstOrDefault(m => m.geotag.Equals(geotag));
                if (found != null)
                {
                    found.geotag = pm.geotag;
                    found.bot_token = pm.bot_token;
                    found.phone_number = pm.phone_number;                    
                    found.posting_type = pm.posting_type;
                    found.is_ai_enabled = pm.is_ai_enabled;
                    found.start_date = pm.start_date;

                    storage.save(PmModels);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Не удалось обновить данные");
            }
        }

        public void Save()
        {
            try
            {
                storage.save(PmModels);

            }
            catch (Exception ex)
            {
                throw new Exception("Не удалось сохранить данные");
            }
        }
        #endregion

    }
}
