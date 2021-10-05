using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;

namespace MailSender.Repository
{
    public class StackRepository : IRepository
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public void SqlQuerySaver(CustomerType type, string queryText)
        {
            throw new NotImplementedException();
        }
        public DataTable QueryDemo(string selectDB, string queryText)
        {
            DataTable dataQueryStack = new DataTable();       
            string stackConnectionString = selectDB;           
            using (SqlConnection stackConnection = new SqlConnection(stackConnectionString))
            {
                try
                {                
                    stackConnection.Open();
                    SqlCommand cmd = new SqlCommand(queryText, stackConnection);
                    SqlDataReader reader = cmd.ExecuteReader();
                    dataQueryStack.Load(reader);

                    for (int i = dataQueryStack.Rows.Count - 1; i >= 0; i--)
                    {
                        if (i > 100)
                        {
                            DataRow row = dataQueryStack.Rows[i];
                            dataQueryStack.Rows.Remove(row);
                        }
                    }                   
                    return dataQueryStack;
                    
                }
                catch (SqlException er)
                {
                    string totalErrorMessage = null;

                    foreach (SqlError error in er.Errors)
                    {
                        totalErrorMessage += $"Данные для визуализации, ошибка подключения к БД {error.Source}\nКод ошибки: {er.Number}";
                    }
                    MessageBox.Show(totalErrorMessage);
                    logger.Error(totalErrorMessage);
                    return dataQueryStack;
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return dataQueryStack;
                }         
            }
        }

        public DataTable QueryForSend(string selectDB, string queryText)
        {
            DataTable dataQueryStack = new DataTable();
            string stackConnectionString = selectDB;
            using (SqlConnection stackConnection = new SqlConnection(stackConnectionString))
            {
                try
                {
                    stackConnection.Open();
                    SqlCommand cmd = new SqlCommand(queryText, stackConnection);
                    SqlDataReader reader = cmd.ExecuteReader();
                    dataQueryStack.Load(reader);

                    return dataQueryStack;
                }
                catch (SqlException er)
                {
                    string totalErrorMessage = null;

                    foreach (SqlError error in er.Errors)
                    {
                        totalErrorMessage += $"При получении данных для отправки, возникала ошибка подключения к БД {error.Source}\nКод ошибки: {er.Number}";
                        logger.Error(totalErrorMessage + "{Time}");
                    }
                    return dataQueryStack;
                }
                catch (Exception ex)
                {
                    logger.Error("При получении данных для отправки возникла ошибка:" + ex.Message);
                    return dataQueryStack;
                }
            }
        }

        //private string GetConnectionString(CustomerType type)
        //{
        //    switch (type)
        //    {
        //        case CustomerType.Fl:
        //            return ConfigurationManager.ConnectionStrings["FlConnection"].ConnectionString;
        //        case CustomerType.Ul:
        //            return ConfigurationManager.ConnectionStrings["UlConnection"].ConnectionString;
        //        default:
        //            return null;
        //    }
        //}
        public int SaveNewShablon(Shablons shablon)
        {
            using(MailContext db = new MailContext())
            {
                        db.Shablons.Add(shablon);
                        db.SaveChanges();            
            }
            int row_id = shablon.ID;
            return row_id;    
        }

        public async Task SaveNewSend(Send send)
        {
            using (MailContext db = new MailContext())
            {
                db.Send.Add(send);
                await db.SaveChangesAsync();
            }

        }

        public async Task SaveNewResult(Results results)
        {
            using (MailContext db = new MailContext())
            {
                db.Results.Add(results);
                await db.SaveChangesAsync();
            }
        }
        
        public List<string> GetTitleShablon(CustomerType type)
        {
            if (type == CustomerType.Fl || type == CustomerType.Ul || type == CustomerType.Another)
            {
                using (MailContext db = new MailContext())
                {
                    List<string> shablon = db.Shablons.
                        Where(p => p.Consumer_Type.Equals(type.ToString(), StringComparison.OrdinalIgnoreCase)).
                        Select(p => p.Title).ToList();

                    return shablon;
                }
            }
            return null;
        }
    }
}
