using botplatform.Models.settings;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.ViewModels
{
    public class settingsVM : ViewModelBase
    {
        #region properties
        long _operator_tg;
        public long operator_tg
        {
            get => _operator_tg;
            set => this.RaiseAndSetIfChanged(ref _operator_tg, value);  
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> saveCmd { get; }
        #endregion

        public settingsVM()
        {
            var settings = Settings.getInstance();

            operator_tg = settings.operator_tg;

            #region commands
            saveCmd = ReactiveCommand.Create(() => {
                settings.operator_tg = operator_tg;
                settings.Save();
            });
            #endregion
        }
    }
}
