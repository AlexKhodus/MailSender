using MailSender.Repository;
using MailSender.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using NLog;
using MailSender.BusinessLogic;

namespace MailSender.ViewModels
{
    public class SaveShablonViewModel : ViewModelBase
    {
        private string shablonName;
        private int shablonID;

        private RelayCommand backpage;
        private RelayCommand saveQuery;       

        private Logger logger = LogManager.GetCurrentClassLogger();
        private UsingParametrs usingRepository => new UsingParametrs();

        private readonly IRepository stackRepository;
        private readonly ViewModelLocator locator;

        public ICommand BackPage
        {
            get
            {
                if (backpage == null)
                {
                    backpage = new RelayCommand(_ =>
                    {
                        Saver saveWindow = Application.Current.Windows.OfType<Saver>().FirstOrDefault();
                        saveWindow.Close();
                    });
                }
                return backpage;
            }
        }
        public SaveShablonViewModel(IRepository repository)
        {
            stackRepository = repository;
            locator = new ViewModelLocator();
        }
        public int ShablonID
        {
            get => shablonID;
            set => Set(ref shablonID, value);
        }
        public string ShablonName
        {
            get => shablonName;
            set => Set(ref shablonName, value);
        }

        public ICommand SaveQuery
        {
            get
            {
                if (saveQuery == null)
                {
                    saveQuery = new RelayCommand(_ =>
                    {
                        List<string> shablonNames;
                        using (MailContext db = new MailContext())
                        {
                            shablonNames = db.Shablons.
                                Select(p => p.Title).ToList();
                        }
                        string title = shablonName.ToString();
                        string textQuery = locator.SqlQueryTab.Doc.Text.ToString();
                        string textHteml = locator.HtmlTab.HtmlDoc.Text.ToString();
                        string typeCustomer = locator.SqlQueryTab.SelectedTypeCustomer.ToString();
                        string dbname = locator.SqlQueryTab.SelectConnectionName.ToString();
                        bool replayName = shablonNames.Any(p => p == title);
                        if (replayName == false || ShablonID == 0)
                        {
                            try
                            {
                                Shablons shablon = new Shablons()
                                {
                                    Title = title,
                                    Query = textQuery,
                                    HTML = textHteml,
                                    DB_Name = dbname,
                                    Consumer_Type = typeCustomer
                                };
                                stackRepository.SaveNewShablon(shablon);
                                locator.MainWindow.SelectedShablon = shablon;
                                logger.Trace("Пользователь" + usingRepository.WhoIsActive() + "добавил новый шаблон");
                                switch(typeCustomer)
                                {
                                    case "Fl":
                                        locator.MainWindow.TextTemplateFL.Add(title);
                                    break;
                                    case "Ul":
                                        locator.MainWindow.TextTemplateUL.Add(title);
                                    break;
                                    case "Another":
                                        locator.MainWindow.TextTemplateAnother.Add(title);
                                    break;
                                    default:
                                        MessageBox.Show("Ошибка: не определена категория шаблона", "", MessageBoxButton.OK);
                                    break;
                                } 
                                MessageBoxResult res = MessageBox.Show("Новый шаблон добавлен");
                                if (res == MessageBoxResult.OK)
                                {
                                    try
                                        {
                                            if (locator.ControlTab.CurrentIndex == 0)
                                            {
                                                locator.SqlQueryTab.SaveWindow.Hide();
                                            }
                                            else if (locator.ControlTab.CurrentIndex == 1)
                                            {
                                                locator.HtmlTab.SaveWindow.Hide();
                                            }
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show("Ошибка:" + ex.Message, "", MessageBoxButton.OK);
                                        logger.Error(ex.StackTrace);
                                    }
                                }                                  
                            }
                            catch(Exception ex)
                            {
                                MessageBox.Show("Ошибка:" + ex.Message, "", MessageBoxButton.OK);
                                logger.Error(ex.StackTrace); 
                            }
                        }
                        else if(ShablonID != 0)
                        {
                            Shablons shablon = new Shablons()
                            {
                                ID = ShablonID,
                                Title = title,
                                Query = textQuery,
                                HTML = textHteml,
                                DB_Name = dbname,
                                Consumer_Type = typeCustomer
                            };
                            usingRepository.UpdateShablon(shablon);
                            locator.MainWindow.SelectedShablon = shablon;
                            MessageBoxResult res = MessageBox.Show("Шаблон обновлен");
                            if (res == MessageBoxResult.OK)
                            {
                                try
                                {
                                    if (locator.ControlTab.CurrentIndex == 0)
                                    {
                                        locator.SqlQueryTab.SaveWindow.Hide();
                                    }
                                    else if (locator.ControlTab.CurrentIndex == 1)
                                    {
                                        locator.HtmlTab.SaveWindow.Hide();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Ошибка:" + ex.Message, "", MessageBoxButton.OK);
                                    logger.Error(ex.StackTrace);
                                }
                            }                       
                        }
                        else
                        {
                            MessageBoxResult result = MessageBox.Show("Шаблон с таким именем уже существует");
                            //if (result == MessageBoxResult.OK)
                            //{
                            //    ShablonName = null;
                            //}
                        }                       
                    });
                }
                return saveQuery;
            }
        }
    }
}
