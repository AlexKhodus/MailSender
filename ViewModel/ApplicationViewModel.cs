using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using MailSender.Repository;

namespace MailSender.ViewModel
{
    public class ApplicationViewModel : ViewModelBase
    {
        private RelayCommand applicationStarted;

        public ICommand ApplicationStart
        {
            get
            {
                if (applicationStarted == null)
                {
                    applicationStarted = new RelayCommand(_ => 
                    {
                    });
                }

                return applicationStarted;
            }
        }
    }
}
