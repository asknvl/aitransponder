using botplatform.Models;
using botplatform.Models.pmprocessor;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;

namespace botplatform.ViewModels.pmrocessor
{
    public class addPmVM : SubContentVM
    {

        #region vars
        #endregion

        #region properties
        string _geotag;
        public string geotag
        {
            get => _geotag;
            set => this.RaiseAndSetIfChanged(ref _geotag, value);
        }

        string _phone_number;
        public string phone_number
        {
            get => _phone_number;
            set => this.RaiseAndSetIfChanged(ref _phone_number, value);
        }

        string _bot_token;
        public string bot_token
        {
            get => _bot_token;
            set => this.RaiseAndSetIfChanged(ref _bot_token, value);
        }

        List<PMType> _posting_types = common.common_Available_Posting_Types;
           
        public List<PMType> posting_types
        {
            get => _posting_types;
            set => this.RaiseAndSetIfChanged(ref _posting_types, value);
        }

        PMType _posting_type;
        public PMType posting_type
        {
            get => _posting_type;
            set => this.RaiseAndSetIfChanged(ref _posting_type, value);
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> addCmd { get; }
        public ReactiveCommand<Unit, Unit> cancelCmd { get; }
        #endregion

        public addPmVM()
        {
            #region commands
            addCmd = ReactiveCommand.Create(() => {
                PmModel model = new PmModel()
                {
                    geotag = geotag,
                    phone_number = phone_number,
                    bot_token = bot_token,
                    posting_type = posting_type,
                };
                CreatedEvent?.Invoke(model);
                Close();
            });
            cancelCmd = ReactiveCommand.Create(() => {
                CancelledEvent?.Invoke();
                Close();
            });
            #endregion
        }        

        #region callbacks
        public event Action<PmModel> CreatedEvent;
        public event Action CancelledEvent;
        #endregion
    }
}
