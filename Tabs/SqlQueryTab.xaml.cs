using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MailSender.Tabs
{
    /// <summary>
    /// Логика взаимодействия для SqlQueryTab.xaml
    /// </summary>
    public partial class SqlQueryTab : UserControl
    {
        public SqlQueryTab()
        {
            InitializeComponent();
        }

        private void queryGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
