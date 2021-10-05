using MailSender.ViewModel;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MailSender.BusinessLogic
{
    public class PropertiesManager
    {
        private static Dictionary<string, string> connectionStrings;

        public static Dictionary<string, string> ConnectionStrings
        {
            get
            {
                if (connectionStrings == null)
                {
                    connectionStrings = new Dictionary<string, string>();
                    string rawConnectionStrings = Properties.Settings.Default.ConnectionsCollection;

                    if (!string.IsNullOrEmpty(rawConnectionStrings))
                    {
                        connectionStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(rawConnectionStrings);
                    }
                }

                return connectionStrings;
            }
        }

        public static void AddConnectionString(string connectionName, string connectionString)
        {
            if (string.IsNullOrEmpty(connectionName) || string.IsNullOrEmpty(connectionString))
            {
                return;
            }
            ConnectionStrings.Add(connectionName, connectionString);
            string rawConnectionStrings = JsonConvert.SerializeObject(ConnectionStrings, Formatting.Indented);
            Properties.Settings.Default.ConnectionsCollection = rawConnectionStrings;
            Properties.Settings.Default.Save();

           
        }
    }
}