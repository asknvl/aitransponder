using asknvl.logger;
using botplatform.Models.storage;
using botplatform.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.InlineQueryResults;

namespace botplatform.Models.pmprocessor
{
    public abstract class PMBase : ViewModelBase, IPM
    {
        #region vars
        protected ILogger logger;
        protected IOperatorStorage opertatorStorage;
        protected IPMStorage pmStorage;
        PmModel tmpPmModel;
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

        public List<PostingType> posting_types { get; } = common.common_Available_Posting_Types;

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

        bool _is_editable;
        public bool is_editable
        {
            get => _is_editable;
            set => this.RaiseAndSetIfChanged(ref _is_editable, value);
        }

        bool _is_active = false;
        public bool is_active
        {
            get => _is_active;
            set
            {
                is_editable = false;
                this.RaiseAndSetIfChanged(ref _is_active, value);
            }
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> startCmd { get; }
        public ReactiveCommand<Unit, Unit> stopCmd { get; }
        public ReactiveCommand<Unit, Unit> editCmd { get; }
        public ReactiveCommand<Unit, Unit> cancelCmd { get; }
        public ReactiveCommand<Unit, Unit> saveCmd { get; }
        #endregion

        public PMBase(PmModel model, IOperatorStorage opertatorStorage, IPMStorage pmStorage, ILogger logger)
        {

            this.logger = logger;
            this.opertatorStorage = opertatorStorage;
            this.pmStorage = pmStorage;

            geotag = model.geotag;
            phone_number = model.phone_number;
            bot_token = model.bot_token;
            posting_type = model.posting_type;

            #region commands
            startCmd = ReactiveCommand.CreateFromTask(async () => {



                is_active = true;
            });
            stopCmd = ReactiveCommand.Create(() => { 
                is_active = false;
            });
            editCmd = ReactiveCommand.Create(() => {

                tmpPmModel = new PmModel()
                {
                    geotag = geotag,
                    phone_number = phone_number,
                    bot_token = bot_token,
                    posting_type = posting_type,

                };

                is_editable = true;            
            });    
            cancelCmd = ReactiveCommand.Create(() => {
                geotag = tmpPmModel.geotag;
                bot_token = tmpPmModel.bot_token;
                phone_number = tmpPmModel.phone_number;
                posting_type = tmpPmModel.posting_type;
                is_editable = false;
            });  
            saveCmd = ReactiveCommand.Create(() => {
                var model = new PmModel()
                {
                    geotag = geotag,
                    phone_number = phone_number,
                    bot_token = bot_token,
                    posting_type = posting_type
                };
                pmStorage.Update(tmpPmModel.geotag, model);
                is_editable = false;
            });
            #endregion

        }
    }
}
