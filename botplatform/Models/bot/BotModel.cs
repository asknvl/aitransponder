using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Model.bot
{
    public class BotModel
    {
        public BotType type { get; set; }
        public string service { get; set; } = "aviator_bot";
        public string geotag { get; set; }
        public string token { get; set; }
        public string phone { get; set; }

        public string link { get; set; }
        public string support_pm { get; set; }
        public string pm { get; set; }
        public string channel_tag { get; set; }
        public string channel { get; set; }

        public string help { get; set; }
        public string training { get; set; }
        public string reveiews { get; set; }
        public string strategy { get; set; }
        public string vip { get; set; }

        public bool? postbacks { get; set; }
        //public List<long> operators_id { get; set; } = new();
        public List<Operators.Operator> operators { get; set; } = new();
    }

    public enum BotType
    {
        transponder_v1
    }
}
