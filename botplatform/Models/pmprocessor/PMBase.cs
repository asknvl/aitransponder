using asknvl.logger;
using botplatform.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.Models.pmprocessor
{
    public abstract class PMBase : ViewModelBase, IPM
    {
        #region vars
        ILogger logger; 
        #endregion

        #region properties
        string _geotag;
        public string geotag {
            get => _geotag;
            set => this.RaiseAndSetIfChanged(ref _geotag, value);   
        }

        string _phone_number;        
        public string phone_number { 
            get => _phone_number;
            set => this.RaiseAndSetIfChanged(ref _phone_number, value);
        }

        string _bot_token;        
        public string bot_token { 
            get => _bot_token;
            set => this.RaiseAndSetIfChanged(ref _bot_token, value);    
        }

        PostingType _posting_type;
        public PostingType posting_type
        {
            get => _posting_type;
            set => this.RaiseAndSetIfChanged(ref _posting_type, value);
        }

        string _bot_username;
        public string bot_username
        {
            get => _bot_username;
            set => this.RaiseAndSetIfChanged(ref _bot_username, value);
        }

        bool _is_active;
        public bool is_active
        {
            get => _is_active;
            set => this.RaiseAndSetIfChanged(ref _is_active, value);    
        }
        #endregion

        public PMBase(PmModel model, ILogger logger)
        {

            this.logger = logger;

            geotag = model.geotag;
            phone_number = model.phone_number;
            bot_token = model.bot_token;
            posting_type = model.posting_type;

        }
    }
}
