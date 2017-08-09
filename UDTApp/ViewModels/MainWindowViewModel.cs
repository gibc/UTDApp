using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
using System.Windows.Input;
using UDTApp.Models;
using System.Windows;
using Prism.Events;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.ComponentModel;
using UDTApp.Views;

namespace UDTApp.ViewModels
{    
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        public DelegateCommand<string> NavigateCommand { get; set; }
        public DelegateCommand WindowLoadedCommand { get; set; }
        public DelegateCommand EditCommand { get; set; }
        public DelegateCommand RunCommand { get; set; }


        public MainWindowViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            NavigateCommand = new DelegateCommand<string>(Navigate);
            WindowLoadedCommand = new DelegateCommand(windowLoaded);
            EditCommand = new DelegateCommand(editProject);
            RunCommand = new DelegateCommand(runProject);

        }

        private void Navigate(string uri)
        {
            _regionManager.RequestNavigate("ContentRegion", uri);
        }

        private void windowLoaded()
        {
            Navigate("Data");
            //Navigate("DataEditView");
        }

        private void editProject()
        {
            List<UDTBase> schema = UDTXml.UDTXmlData.readFromXml();
            //if (schema != null) SchemaList = schema;
            Navigate("SetUp");
        }

        private void runProject()
        {
            List<UDTBase> schema = UDTXml.UDTXmlData.readFromXml();
            //if (schema != null) SchemaList = schema;
            Navigate("DataEditView");
        }
    }

}



