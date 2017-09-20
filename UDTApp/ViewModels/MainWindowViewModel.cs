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
        public DelegateCommand OpenCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand NewCommand { get; set; }
        public DelegateCommand SaveDataCommand { get; set; }


        public MainWindowViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            NavigateCommand = new DelegateCommand<string>(Navigate);
            WindowLoadedCommand = new DelegateCommand(windowLoaded);
            EditCommand = new DelegateCommand(editProject);
            RunCommand = new DelegateCommand(runProject, canRun);
            OpenCommand = new DelegateCommand(openProject);
            SaveCommand = new DelegateCommand(saveProject, canSavePorjext); 
            NewCommand = new DelegateCommand(newProject);
            SaveDataCommand = new DelegateCommand(saveData, canSaveData);
        }

        private string currentView = "";

        private void Navigate(string uri)
        {
            _regionManager.RequestNavigate("ContentRegion", uri);
            currentView = uri;
            RunCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
            SaveDataCommand.RaiseCanExecuteChanged();
        }

        private void windowLoaded()
        {
            //Navigate("Data");
            //Navigate("DataEditView");
        }

        private void editProject()
        {
            List<UDTBase> schema = UDTXml.UDTXmlData.readFromXml();
            if(schema != null)
            { 
                UDTData master = schema[0] as UDTData;
                master.validationChangedEvent += projectValidationChanged;
                master.dataChangeEvent += projectDataChanged;
                projectDataModified = false;
                Navigate("PageZero");
            }
        }

        private void projectValidationChanged()
        {
            SaveCommand.RaiseCanExecuteChanged();
        }

        private void projectDataChanged()
        {
            projectDataModified = true;
            SaveCommand.RaiseCanExecuteChanged();
        }

        // run current project else select poject and run
        private void runProject()
        {
            if(UDTXml.UDTXmlData.SchemaData.Count == 0)
            { 
                List<UDTBase> schema = UDTXml.UDTXmlData.readFromXml();
                if (schema == null) return;
            }
            UDTDataSet.udtDataSet.dataChangeEvent += dataChanged;
            UDTDataSet.udtDataSet.validationChangedEvent += dataValidationChanged;
            Navigate("DataEditView");
        }

        private bool canRun()
        {
            if (UDTXml.UDTXmlData.SchemaData.Count == 0) return true;
            else return currentView != "DataEditView";
        }

        // select a project and run
        private void openProject()
        {
            List<UDTBase> schema = UDTXml.UDTXmlData.readFromXml();
            if(schema != null)
            { 
                UDTDataSet.udtDataSet.dataChangeEvent += dataChanged;
                UDTDataSet.udtDataSet.validationChangedEvent += dataValidationChanged;
                Navigate("DataEditView");
            }
        }

        private void saveProject()
        {
            // save to xml file AND create/update database
            if(UDTXml.UDTXmlData.saveToXml(UDTXml.UDTXmlData.SchemaData))
            {
                UDTDataSet.udtDataSet.createDatabase(UDTXml.UDTXmlData.SchemaData[0] as UDTData);
                //Navigate("DataEditView");
            }
        }

        private bool canSavePorjext()
        {
            if (UDTXml.UDTXmlData.SchemaData.Count == 0) return false;
            else if (currentView != "PageZero") return false;
            else if (UDTXml.UDTXmlData.SchemaData[0].AnyErrors) return false;
            else return projectDataModified;
        }

        private void saveData()
        {
            UDTDataSet.udtDataSet.saveDataset();
        }


        public void dataChanged()
        {
            SaveDataCommand.RaiseCanExecuteChanged();
        }

        public void dataValidationChanged()
        {
            SaveDataCommand.RaiseCanExecuteChanged();
        }

        private bool canSaveData()
        {
            if (currentView != "DataEditView") return false;
            else if (UDTDataSet.udtDataSet.DataSet == null) return false;
            else if (UDTDataSet.udtDataSet.HasEditErrors) return false;
            else return UDTDataSet.udtDataSet.IsModified;
        }

        private void newProject()
        {
            NewProject win = new NewProject();
            win.ShowDialog();
            if((bool)win.DialogResult)
            { 
                string projName = win.prjName.Text;
                List<UDTBase> newSchmea = UDTXml.UDTXmlData.newProject(projName);
                UDTData master = newSchmea[0] as UDTData;
                master.validationChangedEvent += projectValidationChanged;
                master.dataChangeEvent += projectDataChanged;
                projectDataModified = false;
                Navigate("PageZero");
            }
           
        }

        private bool projectDataModified { get; set; }


    }

}



