using MailSender.Repository;
using MailSender.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit.Document;
using System.Collections.ObjectModel;
using NLog;
using MailSender.BusinessLogic;

namespace MailSender.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {

        private Logger logger = LogManager.GetCurrentClassLogger();
        private UsingParametrs usingRepository => new UsingParametrs();
        private CreateConnection createConnectionWindow;

        private ObservableCollection<string> textTemplateFL;
        private ObservableCollection<string> textTemplateUL;
        private ObservableCollection<string> textTemplateAnother;
        private string currentShablon;
        private string currentUser;

        private ICommand newShablon;
        private ICommand exitProgram;
        private ICommand currentShablonSelected;
        private ICommand newConnection;

        public string CurrentUser
        {
            get => currentUser;
            set => Set(ref currentUser, value);
        }
        public string CurrentShablon
        {
            get => currentShablon;
            set => Set(ref currentShablon, value);
        }

        public Shablons SelectedShablon { get; set; }
        public CreateConnection CreateConnectionWindow => createConnectionWindow;
        public ObservableCollection<string> TextTemplateFL
        {
            get
            {
                if (textTemplateFL == null)
                {
                    List<string> sourceTemplates = new StackRepository().GetTitleShablon(CustomerType.Fl);
                    textTemplateFL = new ObservableCollection<string>(sourceTemplates);
                }
                return textTemplateFL;
            }
        }

        public ObservableCollection<string> TextTemplateUL
        {
            get
            {
                if (textTemplateUL == null)
                {
                    List<string> sourceTemplates = new StackRepository().GetTitleShablon(CustomerType.Ul);
                    textTemplateUL = new ObservableCollection<string>(sourceTemplates);
                }
                return textTemplateUL;
            }
        }
        public ObservableCollection<string> TextTemplateAnother
        {
            get
            {
                if (textTemplateAnother == null)
                {
                    List<string> sourceTemplates = new StackRepository().GetTitleShablon(CustomerType.Another);
                    textTemplateAnother = new ObservableCollection<string>(sourceTemplates);
                }
                return textTemplateAnother;
            }
        }
        public ICommand CurrentShablonSelected
        {
            get
            {
                if (currentShablonSelected == null)
                {
                    currentShablonSelected = new RelayCommand(_ =>
                    {
                        logger.Trace("Пользователь" + usingRepository.WhoIsActive() + "выбрал шаблон"); 
                        string nameShablon = currentShablon; 
                        ViewModelLocator locator = (ViewModelLocator)Application.Current.Resources["Locator"];
                        using (MailContext db = new MailContext())
                        {
                            SelectedShablon = db.Shablons.Single(p => p.Title.Equals(nameShablon));
                            locator.SqlQueryTab.Doc = new TextDocument(SelectedShablon.Query.AsEnumerable());
                            locator.SaveShablon.ShablonID = SelectedShablon.ID;
                            locator.SqlQueryTab.SelectConnectionName = SelectedShablon.DB_Name;
                            switch(SelectedShablon.Consumer_Type)
                            {
                                case "Fl":
                                    locator.SqlQueryTab.FlRadioChecked = true;
                                break;
                                case "Ul":
                                    locator.SqlQueryTab.UlRadioChecked = true;
                                    break;
                                case "Another":
                                    locator.SqlQueryTab.AnotherRadioChecked = true;
                                    break;
                                default:
                                    MessageBox.Show("Ошибка: не определена категория шаблона", "", MessageBoxButton.OK);
                                    break;
                            }
                            if (SelectedShablon.HTML != null)
                            {
                                locator.HtmlTab.HtmlDoc = new TextDocument(SelectedShablon.HTML.AsEnumerable());
                            }
                            else 
                            { 
                                locator.HtmlTab.HtmlDoc = new TextDocument(" "); 
                            }
                            locator.SaveShablon.ShablonName = SelectedShablon.Title;
                        }
                        CreateShablon page = new CreateShablon();
                        MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                        mainWindow.Hide();
                        page.ShowDialog();
                    });
                }
                return currentShablonSelected;
            }
        }

        public ICommand NewShablon
        {
            get
            {
                if (newShablon == null)
                {
                    newShablon = new RelayCommand(_ =>
                    {

                        ViewModelLocator locator = (ViewModelLocator)Application.Current.Resources["Locator"];
                        locator.HtmlTab.HtmlDoc.Text = string.Empty;
                        locator.SqlQueryTab.Doc.Text = string.Empty;
                        locator.SaveShablon.ShablonName = null;
                        locator.SqlQueryTab.QueryResultSource = null;
                        CreateShablon page = new CreateShablon();
                        MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                        mainWindow.Hide();
                        page.ShowDialog();

                    });
                }
                return newShablon;
            }
        }
        public ICommand NewConnection
        {
            get
            {
                if(newConnection == null)
                {
                    newConnection = new RelayCommand(_ =>
                    {
                        ViewModelLocator locator = (ViewModelLocator)Application.Current.Resources["Locator"];
                        locator.CreateConnection.ConnectionString = string.Empty;
                        locator.CreateConnection.NameConnection = string.Empty;
                        createConnectionWindow = new CreateConnection();
                        CreateConnectionWindow.ShowDialog();
                    });
                }
                return newConnection;
            }
        }

        public ICommand ExitProgram
        {
            get
            {
                if (exitProgram == null)
                {
                    exitProgram = new RelayCommand(_ =>
                    {
                        System.Windows.Application.Current.Shutdown();
                    });
                }
                return exitProgram;
            }
        }
    }
}
