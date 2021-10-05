using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;


namespace MailSender.Repository
{
    public interface IRepository
    {
        void SqlQuerySaver(CustomerType type, string queryText);
        int SaveNewShablon(Shablons shablon);
        Task SaveNewSend(Send send);
        Task SaveNewResult(Results results);
        
        DataTable QueryDemo(string selectDB, string queryText);
        DataTable QueryForSend(string selectDB, string queryText);

    }
}
