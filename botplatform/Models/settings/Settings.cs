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
            ai_server = prmtrs.ai_server;
            ai_token = prmtrs.ai_token;
            stat_server = prmtrs.stat_server;
            stat_token = prmtrs.stat_token;
        }
        #endregion

        #region properties
        [JsonProperty]
        public long operator_tg { get; set; } = 0;
        public string ai_server { get; set; } = "";
        public string ai_token { get; set; } = "";
        public string stat_server { get; set; } = "";
        public string stat_token { get; set; } = "";
        #endregion

        #region public
        public void Save()
        {
            AppParameters p = new AppParameters();

            p.operator_id = operator_tg;
            p.ai_server = ai_server;    
            p.ai_token = ai_token;
            p.stat_server = stat_server;
            p.stat_token = stat_token;  

            storage.save(p);
        }
        #endregion
    }

    public class AppParameters
    {
        public long operator_id { get; set; }
        public string ai_server { get; set; } = "";
        public string ai_token { get; set; } = "";
        public string stat_server { get; set; } = "";
        public string stat_token { get; set; } = "";
    }

}
