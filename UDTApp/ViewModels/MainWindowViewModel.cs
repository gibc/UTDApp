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
using UDTApp.DataBaseProvider;
using System.Windows.Controls;
using UDTApp.Settings;
using Microsoft.Win32;
using System.Windows.Media;
using System.IO;

namespace UDTApp.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        public DelegateCommand<string> NavigateCommand { get; set; }
        public DelegateCommand<Window> WindowLoadedCommand { get; set; }
        //public DelegateCommand<CancelEventArgs> WindowClosingCommand { get; set; }
        public DelegateCommand EditCommand { get; set; }
        public DelegateCommand RunCommand { get; set; }
        public DelegateCommand OpenCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand NewCommand { get; set; }
        public DelegateCommand SaveDataCommand { get; set; }
        public DelegateCommand UndoChangesCommand { get; set; }
        public DelegateCommand AboutCommand { get; set; }
        public DelegateCommand<MenuItem> SubmenuOpenedCommand { get; set; }
        public DelegateCommand ViewDatasetCommand { get; set; }
        public DelegateCommand ViewDesignCommand { get; set; }


        public MainWindowViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            NavigateCommand = new DelegateCommand<string>(Navigate);
            WindowLoadedCommand = new DelegateCommand<Window>(windowLoaded);
            //WindowClosingCommand = new DelegateCommand<CancelEventArgs>(windowClosing);
            EditCommand = new DelegateCommand(editProject);
            RunCommand = new DelegateCommand(runProject, canRun);
            OpenCommand = new DelegateCommand(openProject);
            SaveCommand = new DelegateCommand(saveProject, canSavePorjext);
            UndoChangesCommand = new DelegateCommand(undoChanges);
            NewCommand = new DelegateCommand(newProject);
            SaveDataCommand = new DelegateCommand(saveData, canSaveData);
            AboutCommand = new DelegateCommand(showAbout);
            SubmenuOpenedCommand = new DelegateCommand<MenuItem>(menuOpen);
            ViewDatasetCommand = new DelegateCommand(viewDataset, canChangeView);
            ViewDesignCommand = new DelegateCommand(viewDesign, canChangeView);
        }

        private bool canChangeView()
        {
            if (UDTXml.UDTXmlData.SchemaData.Count == 0) return false;
            return true;
        }

        private bool _designVisible = false;
        public bool designVisible
        {
            get { return _designVisible; }
            set { SetProperty(ref _designVisible, value); }
        }

        private void viewDesign()
        {
            designVisible = true;
            AppSettings.appSettings.designView = true;
            ViewDatasetCommand.RaiseCanExecuteChanged();
            ViewDesignCommand.RaiseCanExecuteChanged();
            if (PageZeroViewModel.viewModel != null)
                PageZeroViewModel.viewModel.windowLoaded();
            Navigate("PageZero");
        }

        private bool _dataSetVisible = false;
        public bool dataSetVisible
        {
            get { return _dataSetVisible; }
            set { SetProperty(ref _dataSetVisible, value); }
        }

        private void viewDataset()
        {
            if(projectStatus == projectSatausEnum.error)
            {
                if (MessageBox.Show("Design errors. Permanently DISCARD Design Changes and switch to DataSet View?", "Design Errors", MessageBoxButton.OKCancel,
                    MessageBoxImage.Question) == MessageBoxResult.OK)
                {
                    // Ok to discard changes and continue
                    discardProjectChanges();
                }
                // else stop navigaion
                else
                {
                    dataSetVisible = false;
                    designVisible = true;
                    return;
                }
            }

            // if modifed, save or discard or stop view change
            if (projectStatus == projectSatausEnum.modifed)
            {
                if (MessageBox.Show("Design Changed. Save Design Changes and switch to DataSet View?", "Save Design Changes", MessageBoxButton.OKCancel,
                    MessageBoxImage.Question) == MessageBoxResult.OK)
                {
                    // Ok to save changes and continue
                    saveDesignChanges();
                }
                else
                {
                    if (MessageBox.Show("Design Changed. Permanently DISCARD Design Changes and switch to DataSet View", "Design Changes", MessageBoxButton.OKCancel,
                        MessageBoxImage.Question) == MessageBoxResult.OK)
                    {
                        // Ok to discard changes and continue
                        discardProjectChanges();
                    }
                    // else stop view change
                    else
                    {
                        dataSetVisible = false;
                        designVisible = true;
                        return;
                    }
                }
            }

            dataSetVisible = true;
            AppSettings.appSettings.designView = false;
            ViewDatasetCommand.RaiseCanExecuteChanged();
            ViewDesignCommand.RaiseCanExecuteChanged();
            if (DataEditViewModel.dataEditViewModel != null)
                DataEditViewModel.dataEditViewModel.windowLoaded();
            Navigate("DataEditView");
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

        private void windowClosing(object sender, CancelEventArgs e)
        {
            if(saveProject(SaveType.appExit))
            {
                e.Cancel = false;
            }
            else e.Cancel = true;
        }

        private void windowLoaded(Window window)
        {
            window.Closing += windowClosing;
        
            if (AppSettings.appSettings.autoOpenFile != null)
            {
                if (AppSettings.appSettings.designView) designVisible = true;
                else dataSetVisible = true;
                openProject(AppSettings.appSettings.autoOpenFile.filePath);
            }
        }

        private void editProject()
        {
            try
            {
                List<UDTBase> schema = UDTXml.UDTXmlData.readFromXml();
                if (schema != null)
                {
                    UDTData master = schema[0] as UDTData;
                    master.validationChangedEvent += projectValidationChanged;
                    master.dataChangeEvent += projectDataChanged;
                    projectDataModified = false;
                    Navigate("PageZero");
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("editProject failed: {0}", ex.Message);
                UDTApp.Log.Log.LogMessage(msg);
                MessageBox.Show(msg);
            }
        }

        private Visibility _projectStatusVisibility = Visibility.Collapsed;
        public Visibility projectStatusVisibility
        {
            get
            {
                if (findDesignValidationError())
                    return Visibility.Visible;
                else if (projectDataModified)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
            set { SetProperty(ref _projectStatusVisibility, value); }
        }

        private Visibility _dataSetStatusVisibility = Visibility.Collapsed;
        public Visibility dataSetStatusVisibility
        {
            get
            {
                if (findDataSetError())
                    return Visibility.Visible;
                else if (UDTDataSet.udtDataSet.IsModified)
                    return Visibility.Visible;
                else return Visibility.Collapsed;
            }
            set { SetProperty(ref _dataSetStatusVisibility, value); }
        }

        private Visibility _saveRemoveButtonVisibility = Visibility.Collapsed;
        public Visibility saveRemoveButtonVisibility
        {
            get
            {
                if (projectStatus == projectSatausEnum.normal && dataSetStatus == projectSatausEnum.normal)
                    return Visibility.Collapsed;
                else return Visibility.Visible;
            }
            set
            {
                value = Visibility.Visible;
                if (projectStatus == projectSatausEnum.normal && dataSetStatus == projectSatausEnum.normal)
                    value = Visibility.Collapsed;
                else value = Visibility.Visible;
                SetProperty(ref _saveRemoveButtonVisibility, value);
            }
        }

        public enum projectSatausEnum { normal, modifed, error, Error}
        private projectSatausEnum _projectStatus = projectSatausEnum.normal;
        public projectSatausEnum projectStatus
        {
            get
            {
                if (findDesignValidationError())
                    return projectSatausEnum.error;
                else if (projectDataModified)
                    return projectSatausEnum.modifed;
                else
                    return projectSatausEnum.normal;
            }
            set
            {
                if (findDesignValidationError())
                    value = projectSatausEnum.error;
                else if (projectDataModified)
                    value = projectSatausEnum.modifed;
                else
                    value = projectSatausEnum.normal;

                SetProperty(ref _projectStatus, value);
            }
        }

        private projectSatausEnum _dataSetStatus = projectSatausEnum.normal;
        public projectSatausEnum dataSetStatus
        {
            get
            {
                if (findDataSetError())
                    return projectSatausEnum.error;
                else if (UDTDataSet.udtDataSet.IsModified)
                    return projectSatausEnum.modifed;
                return projectSatausEnum.normal;
            }
            set
            {
                if (findDataSetError())
                    value = projectSatausEnum.error;
                else if (UDTDataSet.udtDataSet.IsModified)
                    value = projectSatausEnum.modifed;
                else value = projectSatausEnum.normal;

                SetProperty(ref _dataSetStatus, value);
            }
        }

        private SolidColorBrush _projectStatusColor = new SolidColorBrush(Colors.DarkOrange);
        public SolidColorBrush projectStatusColor
        {
            get
            {
                if (findDesignValidationError())
                    return new SolidColorBrush(Colors.Red);
                else if (projectDataModified)
                    return new SolidColorBrush(Colors.Orange);
                else
                    return new SolidColorBrush(Colors.Black);
            }
            set
            {
                SetProperty(ref _projectStatusColor, value);
            }
        }

        private SolidColorBrush _dataSetStatusColor = new SolidColorBrush(Colors.DarkOrange);
        public SolidColorBrush dataSetStatusColor
        {
            get
            {
                if (findDataSetError())
                    return new SolidColorBrush(Colors.Red);
                else if (UDTDataSet.udtDataSet.IsModified)
                    return new SolidColorBrush(Colors.DarkOrange);
                else return new SolidColorBrush(Colors.Black);
            }
            set
            {
                SetProperty(ref _dataSetStatusColor, value);
            }
        }


        private void projectValidationChanged()
        {
            SaveCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged("projectStatus");
            RaisePropertyChanged("projectStatusVisibility");
            RaisePropertyChanged("projectStatusColor");
            RaisePropertyChanged("saveRemoveButtonVisibility");
        }

        private void projectDataChanged()
        {
            projectDataModified = true;
            SaveCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged("projectStatus");
            RaisePropertyChanged("projectStatusVisibility");
            RaisePropertyChanged("projectStatusColor");
            RaisePropertyChanged("saveRemoveButtonVisibility");

        }

        // run current project else select poject and run
        private void runProject()
        {
            try
            {
                if (UDTXml.UDTXmlData.SchemaData.Count == 0)
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
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Xml (*.xml)|*.xml";
            if (openFileDialog.ShowDialog().Value)
            {
                openProject(openFileDialog.FileName);
            }
        }

        private string _projectName = "none"; 
        public string projectName
        {
            get { return _projectName; }
            set { SetProperty(ref _projectName, value); }
        }

        void openProject(string filePath)
        {
            try
            {

                List<UDTBase> schema = UDTXml.UDTXmlData.openProject(filePath);
                if (schema != null)
                {
                    projectName = schema[0].Name;
                    UDTXml.UDTXmlData.SchemaData = schema;

                    AppSettings.appSettings.addFile(filePath);
                    UDTDataSet.udtDataSet.dataChangeEvent -= dataChanged;
                    UDTDataSet.udtDataSet.dataChangeEvent += dataChanged;
                    UDTDataSet.udtDataSet.validationChangedEvent -= dataValidationChanged;
                    UDTDataSet.udtDataSet.validationChangedEvent += dataValidationChanged;

                    UDTData master = schema[0] as UDTData;
                    master.validationChangedEvent += projectValidationChanged;
                    master.dataChangeEvent += projectDataChanged;
                    projectDataModified = false;

                    //Navigate("DataEditView");
                    if (dataSetVisible)
                        viewDataset();
                    else if(designVisible)
                        viewDesign();
                    else
                        viewDataset();

                    RaisePropertyChanged("dataSetStatus");
                    RaisePropertyChanged("dataSetStatusVisibility");
                    RaisePropertyChanged("dataSetStatusColor");
                    RaisePropertyChanged("saveRemoveButtonVisibility");

                    RaisePropertyChanged("projectStatus");
                    RaisePropertyChanged("projectStatusVisibility");
                    RaisePropertyChanged("projectStatusColor");

                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("openProject failed: {0}", ex.Message);
                UDTApp.Log.Log.LogMessage(msg);
                MessageBox.Show(msg);
            }
        }

        private void undoChanges()
        {
            if(dataSetStatus != projectSatausEnum.normal)
            {
                if(MessageBox.Show("Permanently discard DATASET changes?", "Dicard DataSet", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                {
                    discardDataSetChanges();
                }
            }

            if(projectStatus != projectSatausEnum.normal)
            {
                if (MessageBox.Show("Permanently discard DESIGN changes?", "Dicard Design", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                {
                    if (AppSettings.appSettings.autoOpenFile == null)
                    {
                        discardNewProjct();
                    }
                    else discardProjectChanges();
                }
            }
        }

        private void saveProject()
        {
            saveProject(SaveType.saveCommand);
        }

        enum SaveType { appExit, viewChange, saveCommand};
        private bool saveProject(SaveType saveType)
        {
            try
            {
                string opeationName = "save";
                if (saveType == SaveType.appExit)
                    opeationName = "exit";

                // if dataset error discard changes or stop operation
                if (dataSetStatus == projectSatausEnum.error)
                {
                    string msg =
                        string.Format(@"The Dataset has errors." + Environment.NewLine +
                        "Select 'Cancel' to stop the {0} opeation and correct the errors or" + Environment.NewLine +
                        "Select 'Ok' to permanently discard all changes and continue.", opeationName);
                    if (MessageBox.Show(msg,
                        "Dataset Errors", MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.OK)
                    {
                        if (saveType != SaveType.appExit)
                        {
                            discardDataSetChanges();
                        }
                    }
                    else return false; // stop exit
                }

                // if project design error discard changes or stop operation
                else if (projectStatus == projectSatausEnum.error)
                {
                    // if un-saved new project..
                    if (AppSettings.appSettings.autoOpenFile == null)
                    {
                        string msg = string.Format(
                            @"The new Dataset has Design errors." + Environment.NewLine +
                            "Select 'Cancel' to stop the {0} opeation and correct the errors or" + Environment.NewLine +
                            "Select 'Ok' to permanently discard the new project and contine.", opeationName);
                        if (MessageBox.Show(msg,
                            "Design Errors", MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.OK)
                        {
                            discardNewProjct();
                        }
                        else return false; // stop exit
                    }
                    // else if project has been saved
                    else
                    {
                        string msg = string.Format(
                            @"The Dataset Design has errors." + Environment.NewLine +
                            "Select 'Cancel' to stop the {0} opeation and correct the errors or" + Environment.NewLine +
                            "Select 'Ok' to permanently discard all changes and continue.", opeationName);
                        if (MessageBox.Show(msg,
                            "Design Errors", MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.OK)
                        {
                            discardProjectChanges();
                        }
                        else return false; // stop exit
                    }
                }

                // if DataSet is modified save changes or discard changes and continue
                if(dataSetStatus == projectSatausEnum.modifed)
                {
                    string msg = "Save Dataset changes?";
                    if (saveType == SaveType.appExit)
                    { 
                        msg = 
                            @"The dataset is modifed."  + Environment.NewLine +
                            "Select OK to save the data set changes" + Environment.NewLine +
                            "or Select cancel to permanently discard the changes.";
                    }

                    if (MessageBox.Show(msg, 
                        "Dataset Changes", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                    {
                        UDTDataSet.udtDataSet.saveDataset();
                    }
                    else if (saveType != SaveType.appExit)
                    {
                        discardDataSetChanges();
                    }
                }

                // if project is modifed save changes or discard changes and continue
                if (projectStatus == projectSatausEnum.modifed)
                {
                    string msg = "Save dataset Design changes?";
                    if (saveType == SaveType.appExit)
                    {
                        msg = 
                        @"The dataset design is modifed." + Environment.NewLine +
                        "Select OK to save the design changes"  + Environment.NewLine +
                        "or Select cancel to permanently discard the changes.";
                    }

                    if (MessageBox.Show(msg, 
                        "Design Changes", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                    {
                        // if new project get save-to directory from the user;
                        //if (AppSettings.appSettings.autoOpenFile == null)
                        //{
                        //    System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
                        //    if (AppSettings.appSettings.fileSettings.Count > 0)
                        //    {
                        //        string startPath = Path.GetDirectoryName(AppSettings.appSettings.fileSettings[0].filePath);
                        //        dlg.SelectedPath = startPath;
                        //    }
                        //    System.Windows.Forms.DialogResult res = dlg.ShowDialog();
                        //    if (res == System.Windows.Forms.DialogResult.OK)
                        //    {
                        //        string filePath = dlg.SelectedPath + "\\" + UDTXml.UDTXmlData.SchemaData[0].Name + ".xml";
                        //        FileSetting fileSetting = new FileSetting() { filePath = filePath, dateTime = DateTime.Now.ToString() };
                        //        AppSettings.appSettings.autoOpenFile = fileSetting;
                        //    }
                        //    else return false; // stop exit, project mods not saved
                        //}

                        // save current design to xml file or stop operation if can't be saved
                        if (!saveDesignChanges()) return false;
                    }
                    //else return true; // continue exit
                    else if(saveType != SaveType.appExit)
                    {
                        discardProjectChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("saveProject failed: {0}", ex.Message);
                UDTApp.Log.Log.LogMessage(msg);
                MessageBox.Show(msg);
                return false; // stop exit
            }
            return true;
        }

        private bool canSavePorjext()
        {
            return true;
        }

        private void raiseProjectChangeEvents()
        {
            RaisePropertyChanged("projectStatus");
            RaisePropertyChanged("projectStatusVisibility");
            RaisePropertyChanged("projectStatusColor");
            RaisePropertyChanged("saveRemoveButtonVisibility");
        }

        private void raiseDataSetChangeEvents()
        {
            RaisePropertyChanged("dataSetStatus");
            RaisePropertyChanged("dataSetStatusVisibility");
            RaisePropertyChanged("dataSetStatusColor");
            RaisePropertyChanged("saveRemoveButtonVisibility");
        }

        private void discardNewProjct()
        {
            PageZeroViewModel.viewModel.SchemaList = null;
            UDTXml.UDTXmlData.SchemaData.Clear();
            raiseProjectChangeEvents();
        }

        private void discardProjectChanges()
        {
            if (AppSettings.appSettings.autoOpenFile == null)
                discardNewProjct();
            else
            {
                List<UDTBase> schema =
                    UDTXml.UDTXmlData.openProject(AppSettings.appSettings.autoOpenFile.filePath);
                if (schema != null)
                {
                    projectName = schema[0].Name;
                    UDTXml.UDTXmlData.SchemaData = schema;
                    UDTData master = schema[0] as UDTData;
                    master.validationChangedEvent += projectValidationChanged;
                    master.dataChangeEvent += projectDataChanged;
                    projectDataModified = false;
                    PageZeroViewModel.viewModel.windowLoaded();
                    raiseProjectChangeEvents();
                }
            }
        }

        private void discardDataSetChanges()
        {
            UDTDataSet.udtDataSet.DataSet.RejectChanges();
            UDTDataSet.udtDataSet.IsModified = false;
            DataEditViewModel.dataEditViewModel.loadDataGrid();
            raiseDataSetChangeEvents();        
        }

        private bool saveDesignChanges()
        {
            if (AppSettings.appSettings.autoOpenFile == null)
            {
                System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
                if (AppSettings.appSettings.fileSettings.Count > 0)
                {
                    string startPath = Path.GetDirectoryName(AppSettings.appSettings.fileSettings[0].filePath);
                    dlg.SelectedPath = startPath;
                }
                System.Windows.Forms.DialogResult res = dlg.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    string filePath = dlg.SelectedPath + "\\" + UDTXml.UDTXmlData.SchemaData[0].Name + ".xml";
                    FileSetting fileSetting = new FileSetting() { filePath = filePath, dateTime = DateTime.Now.ToString() };
                    AppSettings.appSettings.autoOpenFile = fileSetting;
                }
                else return false; // stop exit, project mods not saved
            }
            // save project
            if (UDTXml.UDTXmlData.saveToXml(UDTXml.UDTXmlData.SchemaData, 
                AppSettings.appSettings.autoOpenFile.filePath))
            {
                try
                {
                    // update or create database from saved xml definition
                    UDTDataSet.udtDataSet.createDatabase(UDTXml.UDTXmlData.SchemaData[0] as UDTData);
                    // create DataEditView if not already created
                    if (DataEditViewModel.dataEditViewModel == null)
                        _regionManager.AddToRegion("ContentRegion", new DataEditView());
                    // loaded dataset data to DataSetView
                    DataEditViewModel.dataEditViewModel.loadDataSet();
                    projectDataModified = false;
                    raiseProjectChangeEvents();
                }
                catch (Exception ex)
                {
                    string errmsg = string.Format("saveDesignChanges failed: {0}", ex.Message);
                    UDTApp.Log.Log.LogMessage(errmsg);
                    MessageBox.Show(errmsg);
                }
            }
            return true;
        }

        private bool findDesignValidationError()
        {
            if (UDTXml.UDTXmlData.SchemaData.Count <= 0)
                return false;
            else return findValidationError(UDTXml.UDTXmlData.SchemaData[0]);
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

        private void menuOpen(MenuItem fileMenu)
        {
            fileMenu.Items.Clear();
            foreach (FileSetting file in AppSettings.appSettings.fileSettings)
            {
                MenuItem newItem = new MenuItem();
                newItem.Header = file.filePath;
                newItem.Click += fileOpen_Click;
                fileMenu.Items.Add(newItem);
            }

        }

        private void fileOpen_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string header = item.Header.ToString();
            openProject(header);
        }

        public void dataChanged()
        {
            SaveDataCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged("dataSetStatus");
            RaisePropertyChanged("dataSetStatusVisibility");
            RaisePropertyChanged("dataSetStatusColor");
            RaisePropertyChanged("saveRemoveButtonVisibility");
        }

        public void dataValidationChanged()
        {
            SaveDataCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged("dataSetStatus");
            RaisePropertyChanged("dataSetStatusVisibility");
            RaisePropertyChanged("dataSetStatusColor");
            RaisePropertyChanged("saveRemoveButtonVisibility");
        }

        private bool findDataSetError()
        {
            if(DataEditViewModel.dataEditViewModel == null) return false;
            DataEditGrid grid = DataEditViewModel.dataEditViewModel.currentEditGrid;
            if (grid == null) return false;

            bool error = false;
            findDataSetError(grid, ref error);
            return error;
        }

        private void findDataSetError(DataEditGrid grid, ref bool error)
        {
            foreach (DataEditGrid childGrid in grid.childGrids)
                findDataSetError(childGrid, ref error);
            foreach (UDTDataBoxBase editBox in grid.editBoxes)
            {
                if (editBox.HasErrors)
                { 
                    error = true;
                    return;
                }
            }          
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
                saveProject();
                NewProject win = new NewProject();
                win.ShowDialog();
                if ((bool)win.DialogResult)
                {
                    string projName = win.prjName.Text;

                    AppSettings.appSettings.autoOpenFile = null;
                    List<UDTBase> newSchmea = UDTXml.UDTXmlData.newProject(projName);
                    UDTData master = newSchmea[0] as UDTData;
                    master.validationChangedEvent += projectValidationChanged;
                    master.dataChangeEvent += projectDataChanged;
                    projectDataModified = false;
                    //Navigate("PageZero");
                    viewDesign();
                    RaisePropertyChanged("projectStatus");
                    RaisePropertyChanged("projectStatusVisibility");
                    RaisePropertyChanged("projectStatusColor");
                    RaisePropertyChanged("saveRemoveButtonVisibility");
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("newProject failed: {0}", ex.Message);
                UDTApp.Log.Log.LogMessage(msg);
                MessageBox.Show(msg);
            }

        }

        private bool _projectDataModified = false;
        private bool projectDataModified
        {
            get { return _projectDataModified; }
            set
            {
                SetProperty(ref _projectDataModified, value);
                //if (_projectDataModified)
                //    projectStatus = projectSatausEnum.modifed;
                //else
                //    projectStatus = projectSatausEnum.normal;               
            }
        }


    }

}



