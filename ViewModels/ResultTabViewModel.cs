using MailSender.BusinessLogic;
using MailSender.Repository;
using MailSender.ViewModel;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MailSender.ViewModels
{
    public class ResultTabViewModel : ViewModelBase
    {
        private readonly ViewModelLocator locator;

        private bool working;
        private int minProgress;
        private int maxProgress;
        private int idShablon;
        public int unsuccessfulResults;
        private string nameParam;
        private string valueParam;
        private string queryText;
        private string resultSending;
        private string selectedDatabase;
        private List<string> file;
        private List<string> emailAddresses;
        private DataTable queryResultSourceFull;
        private ObservableCollection<Results> sendedMails;
        private Thread uiThread;



        private ICommand mainMenu;
        private ICommand exit;
        private ICommand stop;
        private ICommand executeSend;
        private ICommand log;

        private IRepository rep;
        private readonly IRepository stackRepository;
        private readonly UsingParametrs parametersCreator;
        private CancellationTokenSource cancelationFabric = new CancellationTokenSource();
        private Logger logger = LogManager.GetCurrentClassLogger();
        public ResultTabViewModel(IRepository repository)
        {
            rep = new StackRepository();
            stackRepository = repository;
            locator = new ViewModelLocator();
            parametersCreator = new UsingParametrs();
            sendedMails = new ObservableCollection<Results>();
            uiThread = Thread.CurrentThread;
            
        }

        public string SelectDatabase
        {
            get => selectedDatabase;
            set => Set(ref selectedDatabase, value);
        }
        public DataTable QueryResultSourceFull
        {
            get => queryResultSourceFull;
            set => Set(ref queryResultSourceFull, value);
        }
        public List<string> File
        {
            get => file;
            set => Set(ref file, value);
        }
        public List<string> EmailAddresses
        {
            get => emailAddresses;
            set => Set(ref emailAddresses, value);
        }
        public bool Working
        {
            get => working;
            set => Set(ref working, value);
        }
        public int IdShablon
        {
            get => idShablon;
            set => Set(ref idShablon, value);
        }
        public string NameParam
        {
            get => nameParam;
            set => Set(ref nameParam, value);
        }
        public string ValueParam
        {
            get => valueParam;
            set => Set(ref valueParam, value);
        }
        public int CurrentProgressValue
        {
            get => minProgress;
            set => Set(ref minProgress, value);
        }
        public int MaxProgress
        {
            get => maxProgress;
            set => Set(ref maxProgress, value);

        }
        public string QueryString
        {
            get => queryText;
            set => Set(ref queryText, value);

        }
        public string ResultSending
        {
            get => resultSending;
            set => Set(ref resultSending, value);
        }

        public ObservableCollection<Results> SendedMails
        {
            get => sendedMails;
            set => Set(ref sendedMails, value);
        }

        private List<string> EverSelectedParameters { get; set; }
        private string MarkUp { get; set; }

        public ICommand ExecuteSend
        {
            get
            {
                if (executeSend == null)
                {
                    executeSend = new RelayCommand(async _ =>
                    {
                        Working = true;

                        EverSelectedParameters = locator.HtmlTab.EverSelected;
                        MarkUp = locator.HtmlTab.HtmlDoc.Text;

                        await Task.Run(async () =>
                        {
                            logger.Trace("Пользователем" + parametersCreator.WhoIsActive() + "запущено получение данных для SmtpClient");
                            SettingsTabViewModel settingsViewModel = locator.SettingsTab;
                            string selectedDatabase = locator.SqlQueryTab.SelectConnection;
                            logger.Trace("Получение DataTable");
                            DataTable table = rep.QueryForSend(selectedDatabase, queryText);
                            if (table != null)
                            {
                                QueryResultSourceFull = table;
                                logger.Trace("Получение всех строк данных");
                                IEnumerable<DataRow> exactRows = QueryResultSourceFull.Rows.Cast<DataRow>();

                                Send sended = new Send
                                {
                                    Date = DateTime.Now,
                                    ID_shablon = locator.MainWindow.SelectedShablon.ID
                                };

                                await stackRepository.SaveNewSend(sended);
                                logger.Trace("Пользователем" + parametersCreator.WhoIsActive() + "создана запись в БД о попытке рассылки");
                                try
                                {
                                    using (SmtpClient client = new SmtpClient(settingsViewModel.Server, settingsViewModel.Port))
                                    {
                                        client.UseDefaultCredentials = true;
                                        client.Credentials = new NetworkCredential(settingsViewModel.Login, settingsViewModel.Password);
                                        Dispatcher.FromThread(uiThread).Invoke(() => { MaxProgress = exactRows.Count(); });  

                                        foreach (DataRow row in exactRows)
                                        {
                                            if (cancelationFabric.IsCancellationRequested)
                                            {
                                                break;
                                            }
                                            else
                                            {

                                                try
                                                {
                                                    logger.Trace("Пользователем" + parametersCreator.WhoIsActive() + "начата рассылки");
                                                    DataMessage(row, sended, client);

                                                    Dispatcher.FromThread(uiThread).Invoke(() => { ++CurrentProgressValue; });
                                                }
                                                catch (SmtpException ex)
                                                {
                                                    if (ex.StatusCode == SmtpStatusCode.SyntaxError)
                                                    {
                                                        //Dispatcher.FromThread(uiThread).Invoke(() => {  });
                                                        SaveSendResult(row, sended, false);
                                                    }
                                                    else
                                                    {
                                                        logger.Warn("Письмо с необработанной ошибкой, SmtpException код:" + ex.StatusCode + " " + ex.StackTrace + "в" + "{Time} ms");
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    //обработать ошибку в случае неверного формата почты
                                                    logger.Warn("Ошибка" + ex.GetType());

                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // todo: log here

                                    logger.Error("Ошибка, рассылка не производилась" + ex.GetType() + " " + ex.Source + ex.StackTrace);


                                }
                            }
                        }, cancelationFabric.Token);
                        Dispatcher.FromThread(uiThread).Invoke(() => {
                            int successSend = parametersCreator.SuccessfullResults();
                            ResultSending = "Сообщений отправлено:" + successSend + "\n" + "Не отправлено:" + (MaxProgress - successSend);
                            Working = false;
                        });
                        
                    });
                }
                return executeSend;
            }
        }

        private void DataMessage(DataRow row, Send send, SmtpClient client)
        {
            SettingsTabViewModel settingsViewModel = locator.SettingsTab;
            MailMessage message = new MailMessage();
            message.From = new MailAddress(settingsViewModel.MailFrom, settingsViewModel.WhoSend);
            message.Subject = settingsViewModel.MailTheme;
            message.IsBodyHtml = true;
            logger.Trace("Получение текста письма");
            message.Body = parametersCreator.PlaceArgumentsInMarkup(row, MarkUp, EverSelectedParameters);
            if (File != null)
            {
                foreach (string file in File)
                {
                    message.Attachments.Add(new Attachment(file));
                }
            }
            logger.Trace("Получение emails получателей");
            string email = row.Field<string>("EMAIL");
            message.To.Add(email);
            client.Send(message);
            logger.Trace("Рассылка началась");
            //Dispatcher.FromThread(uiThread).Invoke(() =>
            //{
                SaveSendResult(row, send, true);
            //});
        }


        private  void SaveSendResult(DataRow row, Send send, bool isSuccessfulSend)
        {
            int idsend = send.ID;
            dynamic clientNumber = row.Field<dynamic>("Номер");
            Results currentMailSend = new Results()
            {
                Client_account = clientNumber.ToString(),
                Date = DateTime.Now,
                Email = row.Field<string>("EMAIL"),
                Parametrs = parametersCreator.PlaceArgumentsInMarkupForSend(row, MarkUp, EverSelectedParameters),
                ID_send = idsend,
                Flag_success = isSuccessfulSend
            };

            //SendedMails.Add(currentMailSend);
            stackRepository.SaveNewResult(currentMailSend);
            logger.Trace("Запись в БД об отправке сообщения");
        }


        public ICommand Exit
        {
            get
            {
                if (exit == null)
                {
                    exit = new RelayCommand(_ =>
                    {
                        System.Windows.Application.Current.Shutdown();
                    }
                    , _ => ResultSending != null
                    );
                }
                return exit;
            }

        }
        public ICommand MainMenu
        {
            get
            {
                if (mainMenu == null)
                {
                    mainMenu = new RelayCommand(_ =>
                    {
                        WebBrowser webBrowser = Application.Current.Windows.OfType<CreateShablon>().Single().HtmlTabUc.WebBroweserForHtml;
                        MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                        CreateShablon shablonWindow = Application.Current.Windows.OfType<CreateShablon>().FirstOrDefault();
                        ResultSending = null;
                        webBrowser = null;
                        shablonWindow.Close();
                        mainWindow.Show();
                    }
                    , _ => ResultSending != null);
                }
                return mainMenu;
            }

        }

        public ICommand Log
        {
            get
            {
                if (log == null)
                {
                    log = new RelayCommand(_ =>
                    {
                        string pathToLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
                        Process.Start(pathToLog);
                    }
                    , _ => ResultSending != null);
                }
                return log;
            }
        }
        public ICommand Stop
        {
            get
            {
                if (stop == null)
                {
                    stop = new RelayCommand(_ =>
                    {
                        cancelationFabric.Cancel();
                    });
                }
                return stop;
            }
        }
        public void ResultsToExcel(Send sended)
        {
            using(MailContext db = new MailContext())
            {
                System.Data.SqlClient.SqlParameter param = new System.Data.SqlClient.SqlParameter("@currSend", sended);
                
            }
        }

    }
}
