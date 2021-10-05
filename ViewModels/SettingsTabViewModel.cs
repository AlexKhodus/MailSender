using MailSender.Repository;
using MailSender.ViewModel;
using System.Collections.Generic;
using System.Windows.Input;
using System.Net.Mail;
using System.Net;
using System;
using System.Windows.Forms;
using System.Windows;
using NLog;

namespace MailSender.ViewModels
{
    public class SettingsTabViewModel : ViewModelBase
    {
        private IRepository repository;
        private string server;
        private string mailFrom;
        private string whoSend;
        private string mailTheme;
        private string mailTest;
        private string login;
        private string password;
        private List<string> files;
        private string fileDialogContent;
        private string mailText;
        private int port;
        private int working;


        private ICommand sendTest;
        private ICommand fileDialog;
        private ICommand sendMail;
        private RelayCommand nextPage;
        private RelayCommand backPage;

        private Logger logger = LogManager.GetCurrentClassLogger();
        public string Server
        {
            get => server;
            set => Set(ref server, value);
        }
        public int Port
        {
            get => port;
            set => Set(ref port, value);
        }
        public string MailFrom
        {
            get => mailFrom;
            set => Set(ref mailFrom, value);
        }
        public string MailTheme
        {
            get => mailTheme ;
            set => Set(ref mailTheme, value);
        }
        public string MailTest
        {
            get => mailTest;
            set => Set(ref mailTest, value);
        }
        public string Login
        {
            get => login ;
            set => Set(ref login, value);
        }
        public string Password
        {
            get => password;
            set => Set(ref password, value);
        }
        public string WhoSend
        {
            get => whoSend;
            set => Set(ref whoSend, value);
        }
        public string MailText
        {
            get => mailText;
            set => Set(ref mailText, value);
        }

        public List<string> Files
        {
            get => files;
            set
            {
                Set(ref files, value);
                OnPropertyChanged(nameof(FileDialogContent));
            }
        }

        public string FileDialogContent
        {
            get { 
                if (Files == null)
                    {
                    fileDialogContent = "Прикрепить вложение";
                    }
                else
                {
                    fileDialogContent = "Файлы добавлены";
                }    
                return fileDialogContent;
            }  
                //?? (fileDialogContent = "Прикрепить вложение");
               
        }

        public int Working
        {
            get => working;
            set => Set(ref working, value);
        }
        
        public ICommand SendTest
        {
            get
            {
                if (sendTest == null)
                {
                    sendTest = new RelayCommand(_ =>
                    {
                        //ViewModelLocator locator = (ViewModelLocator)System.Windows.Application.Current.Resources["Locator"];
                        SmtpClient client = new SmtpClient(server, port)
                        {
                            UseDefaultCredentials = true,
                            Credentials = new NetworkCredential(login, password)
                        };
                        MailMessage message = new MailMessage();
                        message.From = new MailAddress(mailFrom, whoSend);
                        message.Subject = mailTheme;
                        message.IsBodyHtml = true;
                        message.Body = MailText;
                            if (Files != null)
                            {
                                foreach (string file in Files)
                                {
                                    message.Attachments.Add(new Attachment(file));
                                }                             
                            }                                          
                        try
                        {
                            message.To.Add(mailTest);
                            client.Send(message);
                            MessageBoxResult result = System.Windows.MessageBox.Show("Сообщение успешно отправлено!", "Отправлено!", MessageBoxButton.OK);
                            if (result == MessageBoxResult.OK)
                            {
                                Working = 1;
                            }
                        }
                        catch (Exception ex)
                        {
                            //if(ex.GetType()==FormatException)
                            SmtpStatusCode StatusCode = new SmtpStatusCode();
                            if (StatusCode == 0 && MailTest == null) { System.Windows.MessageBox.Show("Укажите почту для тестовой отправки"); }
                            else if (StatusCode == 0) { System.Windows.MessageBox.Show("Введите пароль почты отправителя"); }

                            else if (StatusCode != 0)
                            {
                                System.Windows.MessageBox.Show("Ошибка: " + StatusCode);
                                logger.Error("Ошибка" + StatusCode);                               
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("Ошибка:" + ex.GetType());
                                logger.Error("Ошибка" + ex.GetType());
                            }                           
                        }
                    });
                }
                return sendTest;
            }
        }
        public ICommand FileDialog
        {
            get
            {
                if (fileDialog == null)
                {
                    fileDialog = new RelayCommand(  _ =>
                    {
                        OpenFileDialog f = new OpenFileDialog();
                        f.Multiselect = true;
                        f.ShowDialog();
                        ViewModelLocator locator = (ViewModelLocator)System.Windows.Application.Current.Resources["Locator"];
                        locator.ResultTab.File = Files; 
                    });
                }
                 return fileDialog;
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
                        ViewModelLocator locator = (ViewModelLocator)System.Windows.Application.Current.Resources["Locator"];
                        locator.ControlTab.CurrentIndex = 3;
                    }, _ => Working == 1);
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
                        ViewModelLocator locator = (ViewModelLocator)System.Windows.Application.Current.Resources["Locator"];
                        locator.ControlTab.CurrentIndex = 1;
                    });
                }
                return backPage;
            }

        }
    }
}