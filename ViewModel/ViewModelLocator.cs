/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:MailSender"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using MailSender.Repository;
using MailSender.ViewModels;

namespace MailSender.ViewModel
{

    public class ViewModelLocator
    {

        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<IRepository, StackRepository>();


            SimpleIoc.Default.Register<ApplicationViewModel>();
            SimpleIoc.Default.Register<MainMenuViewModel>();
            SimpleIoc.Default.Register<ControlTabViewModel>();
            SimpleIoc.Default.Register<SqlQueryTabViewModel>();
            SimpleIoc.Default.Register<HtmlTabViewModel>();
            SimpleIoc.Default.Register<SettingsTabViewModel>();
            SimpleIoc.Default.Register<ResultTabViewModel>();
            SimpleIoc.Default.Register<SaveShablonViewModel>();
            SimpleIoc.Default.Register<CreateConnectionViewModel>();
        }

      
        public ApplicationViewModel Application => ServiceLocator.Current.GetInstance<ApplicationViewModel>();      
        public MainMenuViewModel MainWindow => ServiceLocator.Current.GetInstance<MainMenuViewModel>();
        public ControlTabViewModel ControlTab => ServiceLocator.Current.GetInstance<ControlTabViewModel>();
        public SqlQueryTabViewModel SqlQueryTab => ServiceLocator.Current.GetInstance<SqlQueryTabViewModel>();
        public HtmlTabViewModel HtmlTab => ServiceLocator.Current.GetInstance<HtmlTabViewModel>();
        public SettingsTabViewModel SettingsTab => ServiceLocator.Current.GetInstance<SettingsTabViewModel>();
        public ResultTabViewModel ResultTab => ServiceLocator.Current.GetInstance<ResultTabViewModel>();
        public SaveShablonViewModel SaveShablon => ServiceLocator.Current.GetInstance<SaveShablonViewModel>();
        public CreateConnectionViewModel CreateConnection => ServiceLocator.Current.GetInstance<CreateConnectionViewModel>();
    }
}

