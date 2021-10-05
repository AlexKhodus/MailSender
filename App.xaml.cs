using MailSender.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MailSender
{

    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ViewModelLocator locator = (ViewModelLocator)Application.Current.Resources["Locator"];
            locator.Application.ApplicationStart.Execute(null);
        }
    }
}
