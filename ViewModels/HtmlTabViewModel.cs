using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using MailSender.Repository;
using MailSender.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using NLog;
using MailSender.BusinessLogic;

namespace MailSender.ViewModels
{
    public class HtmlTabViewModel : ViewModelBase
    {
        private IRepository repository;
        private TextDocument htmlDoc;
        private TextDocument htmlDocFinally;
        private string textParameter;
        private int working;
        private dynamic selectedAccountNumber;
        private List<MenuItem> nameParametr;
        private List<dynamic> accountNumber;
        private List<string> everSelectedParameters = new List<string>();
        private readonly UsingParametrs parameterCreator;

        private Thread uiThread;
        private Saver saveWindow;

        private RelayCommand viewerMail;
        private RelayCommand executeHtml;
        private RelayCommand saveForm;
        private RelayCommand cleaning;
        private RelayCommand nextPage;
        private RelayCommand insertParam;
        private RelayCommand backPage;

        private Logger logger = LogManager.GetCurrentClassLogger();
        private TextEditor Editor => Application.Current.Windows.OfType<CreateShablon>().Single().HtmlTabUc.textEditor;
        private WebBrowser WebBrowser => Application.Current.Windows.OfType<CreateShablon>().Single().HtmlTabUc.WebBroweserForHtml;
        public Saver SaveWindow => saveWindow;

        public HtmlTabViewModel()
        {
            uiThread = Thread.CurrentThread;
            parameterCreator = new UsingParametrs();
        }

        public dynamic SelectedAccountNumber
        {
            get => selectedAccountNumber;
            set => Set(ref selectedAccountNumber, value);
        }

        public List<string> EverSelected => everSelectedParameters;

        public TextDocument HtmlDoc
        {
            get
            {
                if (htmlDoc == null)
                {
                    htmlDoc = new TextDocument();
                }
                return htmlDoc;
            }
            set => Set(ref htmlDoc, value);
        }
        public TextDocument HtmlDocFinally
        {
            get
            {
                if (htmlDocFinally == null)
                {
                    htmlDocFinally = new TextDocument();
                }
                return htmlDocFinally;
            }
            set => Set(ref htmlDocFinally, value);
        }
        public string TextParameter
        {
            get => textParameter;
            set => Set(ref textParameter, value);
        }
        public int Working
        {
            get => working;
            set => Set(ref working, value);
        }
        public List<MenuItem> NameParametr
        {
            get
            {
                ViewModelLocator locator = (ViewModelLocator)Application.Current.Resources["Locator"];
                if (locator.SqlQueryTab.QueryResultSource != null)
                {


                    nameParametr = locator.SqlQueryTab.QueryResultSource.Columns.Cast<DataColumn>()
                                    .Select(x => new MenuItem()
                                    {
                                        Header = x.ColumnName,
                                        Command = InsertParam,
                                        CommandParameter = x.ColumnName
                                    })
                                    .ToList();
                }

                return nameParametr;
            }
        }
        public List<dynamic> AccountNumber
        {
            get => accountNumber;
            set => Set(ref accountNumber, value);
        }
        public ICommand InsertParam
        {
            get
            {
                if (insertParam == null)
                {
                    insertParam = new RelayCommand(p =>
                    {
                        if (p is string columnName)
                        {

                            columnName = parameterCreator.CreatePlaceholder(columnName);
                            everSelectedParameters.Add(columnName);
                            HtmlDoc.Text = HtmlDoc.Text.Insert(Editor.CaretOffset, columnName);
                        }
                    });
                }
                return insertParam;
            }
        }
        public ICommand Cleaning
        {
            get
            {
                if (cleaning == null)
                {
                    cleaning = new RelayCommand(_ =>
                    {
                        MessageBoxResult result = MessageBox.Show("Хотите удалить все даные?", "", MessageBoxButton.YesNo);
                        if(result == MessageBoxResult.Yes)
                        {
                            HtmlDoc.Text = string.Empty;
                            SelectedAccountNumber = null;
                            WebBrowser web =  Application.Current.Windows.OfType<CreateShablon>().Single().HtmlTabUc.WebBroweserForHtml;
                            web.Refresh();
                            Working = 0;
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
                if (nextPage == null)
                {
                    nextPage = new RelayCommand(_ =>
                    {
                        ViewModelLocator locator = (ViewModelLocator)Application.Current.Resources["Locator"];
                        locator.ControlTab.CurrentIndex = 2;


                    }                   
                    ,_ => Working == 1 && HtmlDocFinally.Text == HtmlDoc.Text);
                }
                return nextPage;
            }
        }

        public ICommand BackPage
        {
            get
            {
                if (backPage == null)
                {
                    backPage = new RelayCommand(_ =>
                    {
                        ViewModelLocator locator = (ViewModelLocator)Application.Current.Resources["Locator"];
                        locator.ControlTab.CurrentIndex = 0;
                    });
                }
                return backPage;
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
                        saveWindow = new Saver();
                        saveWindow.ShowDialog();
                    });
                }
                return saveForm;
            }

        }

        public ICommand ExecuteHtml
        {
            get
            {
                if (executeHtml == null)
                {
                    executeHtml = new RelayCommand( _ =>
                    {                       
                        ViewModelLocator locator = (ViewModelLocator)Application.Current.Resources["Locator"];
                        if (locator.SqlQueryTab.QueryResultSource != null)
                        {
                            if (SelectedAccountNumber != null)
                            {       
                                DataRow exactRow = locator.SqlQueryTab.QueryResultSource.Rows.
                                Cast<DataRow>().
                                FirstOrDefault(r => r.Field<dynamic>("Номер") == selectedAccountNumber);
                                if (everSelectedParameters.Count != 0)
                                {
                                    string textMail = parameterCreator.PlaceArgumentsInMarkup(exactRow, HtmlDoc.Text, everSelectedParameters);
                                    locator.SettingsTab.MailText = textMail;
                                    WebBrowser.NavigateToString(textMail);
                                    HtmlDocFinally.Text = HtmlDoc.Text;
                                    Working = 1;
                                }
                                else
                                {
                                    MessageBoxResult result = MessageBox.Show("Хотите отобразить без параметров?", "", MessageBoxButton.YesNo);
                                    switch (result)
                                    {
                                        case MessageBoxResult.Yes:
                                            try
                                            {
                                                string textMail = null;
                                                Dispatcher.FromThread(uiThread).Invoke(() => { textMail = htmlDoc.Text; });
                                                textMail = htmlDoc.Text;
                                                locator.SettingsTab.MailText = textMail;
                                                WebBrowser.NavigateToString(textMail);
                                                HtmlDocFinally.Text = HtmlDoc.Text;
                                                Working = 1;
                                            }
                                            catch(Exception ex)
                                            {
                                                logger.Trace("Попытка передать пустой шаблон верстки" + ex.GetType().ToString());
                                                MessageBox.Show("Отстутствует код верстки письма", "", MessageBoxButton.OK);
                                            }
                                        break;
                                        case MessageBoxResult.No:
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("Выберите номер ЛС", "", MessageBoxButton.OK);
                            }
                        }                      
                    });
                }
                return executeHtml;
            }
        }
    }
}
