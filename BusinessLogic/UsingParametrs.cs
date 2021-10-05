using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.Entity;

namespace MailSender.BusinessLogic
{
    public class UsingParametrs
    {
        private const string token = "^^";


        public string CreatePlaceholder(string columnName)
        {
            return token + columnName + token;
        }
        public string PlaceArgumentsInMarkup(DataRow row, string markup, List<string> everSelectedParameters)
        {
            foreach (DataColumn column in row.Table.Columns)
            {
                string parameterPlaceHolderToken = CreatePlaceholder(column.ColumnName);

                if (everSelectedParameters.Contains(parameterPlaceHolderToken))
                {
                    if (markup.Contains(parameterPlaceHolderToken))
                    {
                        dynamic fieldValue = row.Field<dynamic>(column.ColumnName);
                        DataMustBeBeautiful(fieldValue);
                        markup = markup.Replace(parameterPlaceHolderToken, fieldValue.ToString());
                    }
                }
            }
            return markup;
        }
        public List<Parametrs> PlaceArgumentsInMarkupForSend(DataRow row, string markup, List<string> everSelectedParameters)
        {
            List<Parametrs> markupParameters = new List<Parametrs>();

            foreach (DataColumn column in row.Table.Columns)
            {
                string parameterPlaceHolderToken = CreatePlaceholder(column.ColumnName);

                if (everSelectedParameters.Contains(parameterPlaceHolderToken))
                {
                    if (markup.Contains(parameterPlaceHolderToken))
                    {
                        dynamic fieldValue = row.Field<dynamic>(column.ColumnName);

                        Parametrs markupParameter = new Parametrs()
                        {
                            NameParam = column.ColumnName,
                            ValueParam = fieldValue.ToString(),
                        };
                        //SaveNewParameters(markupParameter);
                        markupParameters.Add(markupParameter);
                    }
                }
            }
            return markupParameters;
        }

        public void DataMustBeBeautiful(dynamic fieldValue)
        {
            if (fieldValue is DateTime dt)
            {
                fieldValue = dt.ToString("G");
            }
        }
        public void UpdateShablon(Shablons shablon)
        {
            using (MailContext db = new MailContext())
            {
                if (shablon.ID == 0)
                {
                    db.Shablons.Add(shablon);
                }
                else
                {
                    Shablons updatingShablon = db.Shablons.FirstOrDefault(sh => sh.ID == shablon.ID);

                    if (updatingShablon != null)
                    {
                        updatingShablon.Query = shablon.Query;
                        updatingShablon.HTML = shablon.HTML;
                    }
                }

                db.SaveChanges();
            }
        }
        public string WhoIsActive()
        {
            string currentUser = " " + Environment.UserName;
            return currentUser;
        }
        public int SuccessfullResults()
        {

            int noSend = 0;
            using (MailContext db = new MailContext())
            {
                Send currentSend = db.Send.OrderByDescending(s => s.ID).FirstOrDefault();
                noSend = db.Results.Where(p => p.ID_send == currentSend.ID && p.Flag_success == true).Count();
            }
            return noSend;

        }
        public DataTable CurrentResult()
        {
            DataTable MailsResult = null;
            using (MailContext db = new MailContext())
            {
                Send currentSend = db.Send.OrderByDescending(s => s.ID).FirstOrDefault();
                //db.Results.Load();
                MailsResult = (DataTable)db.Results.Where(p => p.ID_send == currentSend.ID).Select(p => new
                {
                    EMAIL = p.Email,
                    RESULT = p.Flag_success
                });
            }
            return MailsResult;
        }        
    }
}

