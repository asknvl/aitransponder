using aksnvl.storage;
using botplatform.Model.bot;
using motivebot.Model.storage;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace botplatform.ViewModels
{
    public class editBotVM : SubContentVM
    {
        #region properties
        IBotStorage botStorage;
        BotModel botModel;
        #endregion

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

        bool? postbacks;
        public bool? Postbacks
        {
            get => postbacks;
            set => this.RaiseAndSetIfChanged(ref postbacks, value);
        }
        #endregion

        #region commands
        ReactiveCommand<Unit, Unit> saveCmd { get; }
        ReactiveCommand<Unit, Unit> cancelCmd { get; }
        #endregion

        public editBotVM(IBotStorage botstorage, BotBase bot)
        {


            Geotag = bot.Geotag;
            Token = bot.Token;

            botStorage = botstorage;
            var models = botStorage.GetAll();
            botModel = models.FirstOrDefault(m => m.geotag.Equals(bot.Geotag));

            #region commands
            saveCmd = ReactiveCommand.Create(() => {

                var newModel = new BotModel()
                {
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

                    postbacks = Postbacks,
                };

            });

            cancelCmd = ReactiveCommand.Create(() => {

            });
            #endregion
        }

        #region public
        #endregion      
    }
}
