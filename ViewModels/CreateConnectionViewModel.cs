using MailSender.BusinessLogic;
using MailSender.Repository;
using MailSender.ViewModel;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MailSender.ViewModels
{
    public class CreateConnectionViewModel : ViewModelBase
    {
        private IRepository repository;

        private string nameConnection;
        private string connectionString;

        private ICommand addNewConnecttion;
        private ICommand backPage;
        private Logger logger = LogManager.GetCurrentClassLogger();

        public string NameConnection
        {
            get => nameConnection;
            set => Set(ref nameConnection, value);
        }
        public string ConnectionString
        {
            get => connectionString;
            set => Set(ref connectionString, value);
        }
        public ICommand AddNewConnection
        {
            get
            {
                if (addNewConnecttion == null)
                {
                    addNewConnecttion = new RelayCommand(_ =>
                    {
                        ViewModelLocator locator = (ViewModelLocator)Application.Current.Resources["Locator"];

                        //List<string> NamesConnections = PropertiesManager.ConnectionStrings.Select(p => p.Key).ToList();
                        //bool repeatNames = NameConnection.Any(p => p.Equals(NameConnection));
                        //if (repeatNames == true)
                        //{
                            try
                            {
                                using (SqlConnection newConnection = new SqlConnection(ConnectionString))
                                {


                                    PropertiesManager.AddConnectionString(NameConnection, ConnectionString);
                                    locator.SqlQueryTab.OnPropertyChanged(nameof(locator.SqlQueryTab.ConnectionBox));
                                    logger.Trace("Проверка нового подключения");
                                    newConnection.Open();
                                    newConnection.Close();

                                    MessageBoxResult result = MessageBox.Show("Новое подключениек БД добавлено", "", MessageBoxButton.OK);
                                    if (result == MessageBoxResult.OK)
                                    {
                                        try
                                        {
                                            logger.Trace("Проверка подключения завершена успешно, новое подключение добавлено, информация о доступных подключениях хранится в файле пользовательских настроек");
                                            locator.MainWindow.CreateConnectionWindow.Hide();
                                        }
                                        catch (Exception ex)
                                        {
                                            throw;
                                        }
                                    }
                                }
                            }
                            catch (SqlException er)
                            {
                                string totalErrorMessage = null;

                                foreach (SqlError error in er.Errors)
                                {
                                    totalErrorMessage += $"Ошибка подключения к БД {error.Source}\nКод ошибки: {er.Number}";
                                }

                                logger.Error("Попытка добавить невалидное подключение" + totalErrorMessage);
                                MessageBox.Show("Данное подключение недоступно, проверьте работу сервера БД", "", MessageBoxButton.OK);
                            }
                            catch (Exception ex)
                            {
                                string exMessage = ex.Message;
                                if (exMessage == "Элемент с тем же ключом уже был добавлен.")
                                {
                                    MessageBox.Show("Ошибка: это имя занято", "", MessageBoxButton.OK);
                                }
                                else
                                {
                                    logger.Error("Попытка добавить невалидное подключение" + exMessage);
                                    MessageBox.Show("Ошибка: введенные вами данные неверные", "", MessageBoxButton.OK);
                                }
                            }
                        //}
                        //else
                        //{
                        //    MessageBox.Show("Ошибка: Это имя подключения уже занято, придумайте новое", "", MessageBoxButton.OK);
                        //}
                    });
                }
                return addNewConnecttion;
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
                        MessageBoxResult result = MessageBox.Show("Несохраненные данные будут удалены, продолжить?", "", MessageBoxButton.OKCancel);
                        if (result == MessageBoxResult.OK)
                        {
                            NameConnection = null;
                            ConnectionString = null;

                            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                            CreateConnection currentWindow = Application.Current.Windows.OfType<CreateConnection>().FirstOrDefault();
                            currentWindow.Close();
                            mainWindow.Show();
                        }
                        else
                        {
                            backPage = null;
                        }
                    });
                }

                return backPage;
            }
        }
    }
}
