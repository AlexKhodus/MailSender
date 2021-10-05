using MailSender.Repository;
using MailSender.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit.Document;
using System.Threading;
using System.Windows.Threading;
using NLog;
using Newtonsoft.Json;
using MailSender.BusinessLogic;

namespace MailSender.ViewModels
{
    public class SqlQueryTabViewModel : ViewModelBase
    {

        private IRepository repository;
        private UsingParametrs usingRepository;
        private Logger logger;

        private DataTable queryResultSource;
        private CustomerType selectedTypeCustomer = CustomerType.Empty;
        private Thread uiThread;
        private Saver saveWindow;

        private bool working;
        private int currentIndex;
        private string selectConnectionName;
        private string selectConnection;
        private TextDocument doc;
        private TextDocument docFinally;



        private RelayCommand nextPage;
        private RelayCommand executeQuery;
        private RelayCommand cleaning;
        private RelayCommand saveForm;
        private RelayCommand showMainWindow;

        public SqlQueryTabViewModel()
        {
            repository = new StackRepository();
            usingRepository = new UsingParametrs();
            uiThread = Thread.CurrentThread;
            logger = LogManager.GetCurrentClassLogger();
        }
        public List<string> ConnectionBox => PropertiesManager.ConnectionStrings.Keys.ToList();
        public string SelectConnection
        {
            get => PropertiesManager.ConnectionStrings.Where(p => p.Key.Equals(SelectConnectionName)).Select(p => p.Value).FirstOrDefault();
            set {               
                    if (selectConnection == value)
                    {
                        return;
                    }
                selectConnection = value;
            }
        }                                                            
        

        public TextDocument Doc
        {
            get
            {
                if (doc == null)
                {
                    doc = new TextDocument();
                }

                return doc;
            }
            set => Set(ref doc, value);
        }
        public TextDocument DocFinally
        {
            get
            {
                if (docFinally == null)
                {
                    docFinally = new TextDocument();
                }

                return docFinally;
            }
            set => Set(ref docFinally, value);
        }
        public DataTable QueryResultSource
        {
            get => queryResultSource;
            set => Set(ref queryResultSource, value);
        }
        public int CurrentIndex
        {
            get => currentIndex;
            set => Set(ref currentIndex, value);
        }
        public string SelectConnectionName
        {
            get => selectConnectionName;
            set => Set(ref selectConnectionName, value);
        }
        public Saver SaveWindow => saveWindow;
        public bool Working
        {
            get => working;
            set => Set(ref working, value);
        }
        public CustomerType SelectedTypeCustomer
        {
            get => selectedTypeCustomer;
            set
            {
                if (selectedTypeCustomer == value)
                {
                    return;
                }
                selectedTypeCustomer = value;
                OnPropertyChanged("FlRadioChecked");
                OnPropertyChanged("UlRadioChecked");
                OnPropertyChanged("AnotherRadioChecked");
                //ViewModelLocator locator = (ViewModelLocator)Application.Current.Resources["Locator"];
                //locator.ResultTab.selectedDatabase = selectedTypeCustomer;

            }
        }
        public bool FlRadioChecked
        {
            get => SelectedTypeCustomer == CustomerType.Fl;
            set
            {
                SelectedTypeCustomer = value ? CustomerType.Fl : SelectedTypeCustomer;
            }
        }
        public bool UlRadioChecked
        {
            get => SelectedTypeCustomer == CustomerType.Ul;
            set
            {
                SelectedTypeCustomer = value ? CustomerType.Ul : SelectedTypeCustomer;
            }
        }
        public bool AnotherRadioChecked
        {
            get => SelectedTypeCustomer == CustomerType.Another;
            set
            {
                SelectedTypeCustomer = value ? CustomerType.Another : SelectedTypeCustomer;
            }
        }
        public ICommand ExecuteQuery
        {
            get
            {
                if (executeQuery == null)
                {
                    executeQuery = new RelayCommand(async _ =>
                    {
                        await DoWork(() =>
                        {
                            ViewModelLocator locator = (ViewModelLocator)Application.Current.Resources["Locator"];
                            string queryText = null;

                            Dispatcher.FromThread(uiThread).Invoke(() => { queryText = Doc.Text; DocFinally.Text = Doc.Text; });
                            locator.ResultTab.QueryString = queryText;
                            if (SelectConnection == null)
                            {
                                MessageBox.Show("Ошибка: не выбрана строка подключения","", MessageBoxButton.OK);
                            }
                            else
                            {
                                try
                                {
                                    DataTable table = repository.QueryDemo(SelectConnection, queryText);
                                    logger.Trace("Пользователь" + usingRepository.WhoIsActive() + "выполнил запрос для визуализации");

                                    if (table != null)
                                    {
                                        QueryResultSource = table;
                                        try
                                        {
                                            //if (SelectedTypeCustomer.Equals("Fl"))
                                            //{
                                                List<dynamic> accountNumber = table.Rows.Cast<DataRow>().
                                                Select(r => r.Field<dynamic>("Номер")).ToList();
                                                locator.HtmlTab.AccountNumber = accountNumber;
                                            //}
                                            //else if(SelectedTypeCustomer.Equals("Ul"))
                                            //{
                                            //    List<string> accountNumber = table.Rows.Cast<DataRow>().
                                            //    Select(r => r.Field<string>("Номер")).ToList();
                                            //    locator.HtmlTab.AccountNumber = accountNumber;
                                            //}
                                        }
                                        catch (Exception ex)
                                        {
                                            logger.Error("Запрос не содержит колонку Номер" + ex.GetType() + "в" + "{Time} ms");
                                            MessageBox.Show("Ошибка: запрос должен содержать колонку Номер");
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ex.GetType();
                                }
                            }
                        });
                    });
                }
                return executeQuery;
            }
        }     
        public ICommand SaveForm
        {
            get
            {
                if (saveForm == null)
                {
                    saveForm = new RelayCommand(_ =>
                    {
                        //switch
                        if (selectConnectionName == null)
                        {
                            MessageBox.Show("Выберите подключение к базе данных", "", MessageBoxButton.OK);
                        }
                        else
                        {
                            saveWindow = new Saver();
                            saveWindow.ShowDialog();
                        }
                    });
                }
                return saveForm;
            }
        }       
        public ICommand Cleaning
        {
            get 
            { 
                if(cleaning == null)
                {
                    cleaning = new RelayCommand(_ =>
                    {
                    MessageBoxResult result = MessageBox.Show("Хотите удалить все даные?", "", MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                        {
                            Doc.Text = string.Empty;
                            QueryResultSource = default;
                        }
                        else
                        {
                            cleaning = null;
                        }
                    });
                }
                return cleaning;
            }            
        }
        public ICommand NextPage
        {
            get
            {
                if(nextPage== null)
                {
                    nextPage = new RelayCommand(_ =>
                    {
                        ViewModelLocator locator = (ViewModelLocator)Application.Current.Resources["Locator"];
                        locator.ControlTab.CurrentIndex = 1;
                    }
                    , _ => QueryResultSource?.Rows?.Count > 0 && DocFinally.Text==Doc.Text);
                }
                return nextPage;
            }
        }
        public ICommand ShowMainWindow
        {
            get
            {
                if (showMainWindow == null)
                {
                    showMainWindow = new RelayCommand(_ => 
                    {

                        Doc = null;
                        QueryResultSource = null;
                        ViewModelLocator locator = (ViewModelLocator)Application.Current.Resources["Locator"];
                        locator.HtmlTab.AccountNumber = null;
                        locator.HtmlTab.HtmlDoc = null;

                        WebBrowser webBrowser = Application.Current.Windows.OfType<CreateShablon>().Single().HtmlTabUc.WebBroweserForHtml;
                        MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                        CreateShablon shablonWindow = Application.Current.Windows.OfType<CreateShablon>().FirstOrDefault();

                        webBrowser = null;
                        shablonWindow.Close();
                        mainWindow.Show();
                    });
                }

                return showMainWindow;
            }
        }

        private async Task DoWork(Action job)
        {
            Working = true;

            await Task.Run(() => job.Invoke());

            Working = false;
        }
    }
}
