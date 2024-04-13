using aksnvl.storage;
using asknvl.storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace botplatform.Models.settings
{
    public class Settings
    {

        #region vars
        IStorage<AppParameters> storage;
        #endregion

        #region singletone
        static Settings instance;
        public static Settings getInstance()
        {
            if (instance == null)            
                instance = new Settings();                
            return instance;
        }

        private Settings()
        {
            AppParameters p = new();
            storage = new Storage<AppParameters>("settings", "settings", p);
            var prmtrs = storage.load();

            operator_tg = prmtrs.operator_id;
        }
        #endregion

        #region properties
        [JsonProperty]
        public long operator_tg { get; set; } = 0;
        #endregion

        #region public
        public void Save()
        {
            AppParameters p = new AppParameters();

            p.operator_id = operator_tg;

            storage.save(p);
        }
        #endregion
    }

    public class AppParameters
    {
        public long operator_id { get; set; }
    }

}
