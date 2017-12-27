using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using UDTApp.Log;
using UDTApp.Settings;

namespace UDTApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    //public partial class App : Application
    //{
    //}

    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            UDTApp.Log.Log.LogMessage("UDTApp startup.");
            AppSettings settings = AppSettings.appSettings;
            base.OnStartup(e);
            Bootstrapper bs = new Bootstrapper();
            bs.Run();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            AppSettings settings = AppSettings.appSettings;
            AppSettings.Save(settings, AppSettings.settingFilePath);
        }

        //private void CheckBox_Checked(object sender, RoutedEventArgs e)
        //{

        //}
    }
}
