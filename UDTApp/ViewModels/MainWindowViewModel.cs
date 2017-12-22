﻿using System;
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
using UDTApp.DataBaseProvider;

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
        public DelegateCommand AboutCommand { get; set; }


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
            AboutCommand = new DelegateCommand(showAbout);
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
            try
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
            catch(Exception ex)
            {
                string msg = string.Format("editProject failed: {0}", ex.Message);
                UDTApp.Log.Log.LogMessage(msg);
                MessageBox.Show(msg);
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
            try
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
            catch (Exception ex)
            {
                string msg = string.Format("runProject failed: {0}", ex.Message);
                UDTApp.Log.Log.LogMessage(msg);
                MessageBox.Show(msg);
            }

        }

        private bool canRun()
        {
            if (UDTXml.UDTXmlData.SchemaData.Count == 0) return true;
            else return currentView != "DataEditView";
        }

        // select a project and run
        private void openProject()
        {
            try
            { 
                List<UDTBase> schema = UDTXml.UDTXmlData.readFromXml();
                if(schema != null)
                { 
                    UDTDataSet.udtDataSet.dataChangeEvent += dataChanged;
                    UDTDataSet.udtDataSet.validationChangedEvent += dataValidationChanged;
                    Navigate("DataEditView");
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("openProject failed: {0}", ex.Message);
                UDTApp.Log.Log.LogMessage(msg);
                MessageBox.Show(msg);
            }

        }

        private void saveProject()
        {
            // save to xml file AND create/update database
            try
            { 
                if(UDTXml.UDTXmlData.saveToXml(UDTXml.UDTXmlData.SchemaData))
                {
                    try 
                    {
                        UDTDataSet.udtDataSet.createDatabase(UDTXml.UDTXmlData.SchemaData[0] as UDTData);
                    }
                    catch(Exception ex)
                    {
                        string msg = string.Format("createDatabase failed: {0}", ex.Message);
                        UDTApp.Log.Log.LogMessage(msg);
                        MessageBox.Show(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("saveProject failed: {0}", ex.Message);
                UDTApp.Log.Log.LogMessage(msg);
                MessageBox.Show(msg);
            }

        }

        private bool canSavePorjext()
        {
            if (UDTXml.UDTXmlData.SchemaData.Count == 0) return false;
            else if (currentView != "PageZero") return false;
            //else if (UDTXml.UDTXmlData.SchemaData[0].AnyErrors) return false;
            else if (findValidationError(UDTXml.UDTXmlData.SchemaData[0])) return false;
            else return projectDataModified;
        }

        private bool findValidationError(UDTBase udtItem)
        {
            UDTData udtData = udtItem as UDTData;
            if (udtData == null)
            {
                if (udtItem.HasErrors) return true;
                if (udtItem.editProps.HasErrors) return true;
                return false;
            }
            else
            {
                if (udtData.HasErrors) return true;
                foreach(UDTData table in udtData.tableData)
                {
                    if (findValidationError(table))
                        return true;
                }
                foreach (UDTBase column in udtData.columnData)
                {
                    if (findValidationError(column))
                        return true;
                }
            }
            return false;
        }

        private void saveData()
        {
            UDTDataSet.udtDataSet.saveDataset();
        }

        private void showAbout()
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();   
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
            try
            {
                NewProject win = new NewProject();
                win.ShowDialog();
                if ((bool)win.DialogResult)
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
            catch (Exception ex)
            {
                string msg = string.Format("newProject failed: {0}", ex.Message);
                UDTApp.Log.Log.LogMessage(msg);
                MessageBox.Show(msg);
            }

        }

        private bool projectDataModified { get; set; }


    }

}



