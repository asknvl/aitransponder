using botplatform.Models.pmprocessor;
using botplatform.Models.pmprocessor.db_storage;
using botplatform.Models.settings;
using botplatform.Models.storage;
using botplatform.Models.storage.local;
using botplatform.rest;
using botplatform.ViewModels.pmrocessor;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;

namespace botplatform.ViewModels
{
    public class mainVM : LifeCycleViewModelBase
    {

        #region vars
        IPMStorage pmStorage;
        IPmFactory pmFactory;        
        #endregion

        #region properties                
        object subContent;
        public object SubContent
        {
            get => subContent;
            set
            {
                this.RaiseAndSetIfChanged(ref subContent, value);
                if (subContent is SubContentVM) {
                    ((SubContentVM)subContent).OnCloseRequest += () =>
                    {
                        SubContent = null;
                    };
                }
            }
        }


        public ObservableCollection<PMBase> PMs { get; set; } = new();

        PMBase selectedPm;
        public PMBase SelectedPm
        {
            get => selectedPm;
            set
            {
                SubContent = value;
                this.RaiseAndSetIfChanged(ref selectedPm, value);
            }
        }

        loggerVM logger;
        public loggerVM Logger {
            get => logger;
            set => this.RaiseAndSetIfChanged(ref logger, value);
        }

        settingsVM appSettings;
        public settingsVM AppSettings
        {
            get => appSettings;
            set => this.RaiseAndSetIfChanged(ref appSettings, value);  
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> addCmd { get; }
        public ReactiveCommand<Unit, Unit> removeCmd { get;  }
        public ReactiveCommand<Unit, Unit> editCmd { get; }        
        #endregion
        public mainVM()
        {

            AppSettings = new settingsVM();

            Logger = new loggerVM();
            pmStorage = new LocalPmStorage();                        

            ApplicationContext context = new ApplicationContext();
            context.Database.EnsureCreated();
            IDBStorage dbStorage = new DBStorage(context);

            pmFactory = new PmFactory(pmStorage, dbStorage, Logger);

            var settings = Settings.getInstance();            
            

            RestService restService = new RestService(Logger);
            MessageRequestProcessor messageRequestProcessor = new MessageRequestProcessor();    
            restService.RequestProcessors.Add(messageRequestProcessor); 

            restService.Listen();

            var models = pmStorage.GetAll();
            foreach (var model in models)
            {
                var pm = pmFactory.Get(model);
                PMs.Add(pm);        
                messageRequestProcessor.Add(pm);
            }

            #region commands
            addCmd = ReactiveCommand.Create(() => {

                //SelectedBot = null;

                //var addvm = new addBotVM();
                //addvm.BotCreatedEvent += (model) => {
                //    try
                //    {
                //        botStorage.Add(model);
                //    } catch (Exception ex)
                //    {
                //        throw;                        
                //    }

                //    var bot = botFactory.Get(model, logger);
                //    Bots.Add(bot);

                //    operatorStorage.Add(model.geotag);                
                //};

                //addvm.CancelledEvent += () => {                    
                //};

                //SubContent = addvm;

                //SelectedPm = null;

                var addvm = new addPmVM();
                addvm.CreatedEvent += (model) => { 

                    try
                    {
                        pmStorage.Add(model);
                    } catch (Exception ex)
                    {
                        throw;
                    }

                    var pm = pmFactory.Get(model);
                    PMs.Add(pm);
                    messageRequestProcessor.Add(pm);
                };

                SubContent = addvm;
                //SelectedPm = null;
            });

            removeCmd = ReactiveCommand.Create(() =>
            {

                //if (SelectedBot == null)
                //    return;

                //if (SelectedBot.IsActive)
                //    return;

                //var geotag = SelectedBot.Geotag;
                //try
                //{
                //    botStorage.Remove(geotag);
                //}
                //catch (Exception ex)
                //{
                //    throw;                    
                //}

                //Bots.Remove(SelectedBot);

                if (SelectedPm == null)
                    return;

                if (SelectedPm.is_active)
                    return;

                var geotag = SelectedPm.geotag;
                try
                {
                    pmStorage.Remove(geotag);

                } catch (Exception ex)
                {
                    throw;
                }

                PMs.Remove(SelectedPm);

            });            
            #endregion
        }        
    }
}
