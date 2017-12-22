using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

        }
        private Version version = null;
        private Window aboutBoxView { get; set; }
        private Action closeAction { get; set; }
        private void winLoaded(Window window)
        {
            closeAction = new Action(window.Close);
            aboutBoxView = window;
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
