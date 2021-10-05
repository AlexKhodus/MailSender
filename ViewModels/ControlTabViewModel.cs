using MailSender.Repository;
using MailSender.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MailSender.ViewModels
{
    public class ControlTabViewModel : ViewModelBase
    {
        
        private int currentIndex = 0;
        public int CurrentIndex
        {
            get => currentIndex; 
            set => Set(ref currentIndex, value); 
        }
    }
}
