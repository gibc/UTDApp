using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Windows;
using System.Windows.Documents;

namespace UDTApp.ViewModels
{
    public class AboutBoxViewModel
    {
        public DelegateCommand OkCommand { get; set; }
        public DelegateCommand<Window> WindowLoadedCommand { get; set; }

        public AboutBoxViewModel()
        {
            OkCommand = new DelegateCommand(okCmd);
            WindowLoadedCommand = new DelegateCommand<Window>(winLoaded);
            Assembly assembly = Assembly.GetExecutingAssembly();
            version = assembly.GetName().Version;

            versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
        }
        FileVersionInfo versionInfo = null;
        private Version version = null;
        private Window aboutBoxView { get; set; }
        private Action closeAction { get; set; }
        private void winLoaded(Window window)
        {
            closeAction = new Action(window.Close);
            aboutBoxView = window;
            window.ResizeMode = ResizeMode.NoResize;
            Hyperlink webLink = (Hyperlink)window.FindName("webLink");
            webLink.RequestNavigate += WebLink_RequestNavigate;
        }

        private void WebLink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }

        public string minorRevision
        {
            get
            {
                return version.MinorRevision.ToString();
            }
        }

        public string majorRevision
        {
            get
            {
                return version.Build.ToString();
            }
        }

        public string companyName
        {
            get
            {
                return versionInfo.CompanyName;
            }
        }

        public string copyright
        {
            get
            {
                return versionInfo.LegalCopyright;
            }
        }

        public string description
        {
            get
            {
                return versionInfo.FileDescription;
            }
        }

        public string title
        {
            get
            {
                return versionInfo.ProductName;
            }
        }

        public string titlecopyright
        {
            get
            {
                return title + " " + copyright;
            }
        }

        public string versionNumber
        {
            get
            {
                return version.Major.ToString() + "." + version.Minor.ToString();
            }
        }

        void okCmd()
        {
            closeAction();
        }
    }
}
