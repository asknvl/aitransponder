using botplatform.Model.bot;
using ReactiveUI;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.ViewModels
{
    public class addBotVM : SubContentVM
    {
        #region properties
        string geotag;
        public string Geotag
        {
            get => geotag;
            set => this.RaiseAndSetIfChanged(ref geotag, value);
        }

        string name;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        string token;
        public string Token
        {
            get => token;
            set => this.RaiseAndSetIfChanged(ref token, value);
        }

        string link;
        public string Link
        {
            get => link;
            set => this.RaiseAndSetIfChanged(ref link, value);
        }

        string? support_pm;
        public string? SUPPORT_PM
        {
            get => support_pm;
            set => this.RaiseAndSetIfChanged(ref support_pm, value);
        }

        string pm;
        public string PM
        {
            get => pm;
            set => this.RaiseAndSetIfChanged(ref pm, value);
        }


        string? channel_tag;
        public string? ChannelTag
        {
            get => channel_tag;
            set => this.RaiseAndSetIfChanged(ref channel_tag, value);
        }

        string channel;
        public string Channel
        {
            get => channel;
            set => this.RaiseAndSetIfChanged(ref channel, value);
        }

        string help;
        public string Help
        {
            get => help;
            set => this.RaiseAndSetIfChanged(ref help, value);
        }

        string training;
        public string Training
        {
            get => training;
            set => this.RaiseAndSetIfChanged(ref training, value);
        }

        string reviews;
        public string Reviews
        {
            get => reviews;
            set => this.RaiseAndSetIfChanged(ref reviews, value);
        }

        string strategy;
        public string Strategy
        {
            get => strategy;
            set => this.RaiseAndSetIfChanged(ref strategy, value);
        }

        string vip;
        public string Vip
        {
            get => vip;
            set => this.RaiseAndSetIfChanged(ref vip, value);
        }

        bool? postbacks = false;
        public bool? Postbacks
        {
            get => postbacks;
            set => this.RaiseAndSetIfChanged(ref postbacks, value);
        }

        List<BotType> botTypes = new() {
            //BotType.aviator_v0,//0
            //BotType.aviator_v1,//1
            //BotType.aviator_v2_1w_br_eng,//2

            //BotType.getinfo_v0,//3

            //BotType.aviator_v3_1win_wv_eng,//4
            //BotType.aviator_v2_1win_br_esp,//5
            //BotType.aviator_v4_cana34,//6
            //BotType.aviator_v4_cana35,//7
            BotType.transponder_v1


        };
        public List<BotType> BotTypes
        {
            get => botTypes;
            set => this.RaiseAndSetIfChanged(ref botTypes, value);
        }

        BotType type = BotType.transponder_v1;
        public BotType Type
        {
            get => type;
            set => this.RaiseAndSetIfChanged(ref type, value);
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> cancelCmd { get; }
        public ReactiveCommand<Unit, Unit> addCmd { get; }
        #endregion

        public addBotVM()
        {
            #region commands
            cancelCmd = ReactiveCommand.Create(() => {
                CancelledEvent?.Invoke();
                Close();
            });
            addCmd = ReactiveCommand.Create(() => {
                BotModel model = new BotModel()
                {
                    type = Type,
                    geotag = Geotag,
                    token = Token,
                    link = Link,
                    support_pm = SUPPORT_PM,
                    pm = PM,
                    channel_tag = ChannelTag,
                    channel = Channel,

                    help = Help,
                    training = Training,
                    reveiews = Reviews,
                    strategy = Strategy,
                    vip = Vip,

                    postbacks = Postbacks



                };
                BotCreatedEvent?.Invoke(model);
                Close();
            });
            #endregion
        }

        #region callbacks
        public event Action<BotModel> BotCreatedEvent;
        public event Action CancelledEvent;
        #endregion

    }
}
