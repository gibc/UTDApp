using Prism.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UDTApp.BlobStorage;
using UDTApp.Views;

namespace UDTApp.ViewModels
{
    public class InstallViewModel : ValidatableBindableBase
    {
        public DelegateCommand OkCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand BeginInstallCommand { get; set; }
        //public DelegateCommand testConnectionCmd { get; set; }
        public DelegateCommand<Window> WindowLoadedCommand { get; set; }

        public InstallViewModel()
        {
            OkCommand = new DelegateCommand(okCmd, canOk);
            CancelCommand = new DelegateCommand(cancelCmd, canCancel);
            BeginInstallCommand = new DelegateCommand(beginInstalCmd);
            WindowLoadedCommand = new DelegateCommand<Window>(winLoaded);

        }

        InstallView installView = null;
        TextBlock loadCountTxtBlock = null;
        private void winLoaded(Window window)
        {
            installView = window as InstallView;
            loadCountTxtBlock = installView.FindName("loadCount") as TextBlock;
            packageName = string.Format("Installation Package: {0}", fileName);
        }

        private string _packageName = "";
        public string packageName
        {
            get { return _packageName; }
            set { SetProperty(ref _packageName, value); }
        }
        private string _downLoadCountStr = "";
        public string downLoadCountStr
        {
            get { return _downLoadCountStr; }
            set { SetProperty(ref _downLoadCountStr, value); }
        }

        public Visibility _lableVisibility = Visibility.Collapsed;
        public Visibility lableVisibility
        {
            get { return _lableVisibility; }
            set { SetProperty(ref _lableVisibility, value); }
        }

        public string fileName
        { get; set; }

        public string blobName
        { get; set; }

        private void okCmd()
        {
            installView.DialogResult = installOk;
            installView.Close();
        }

        private void cancelCmd()
        {
            if (!isCancelled) isCancelled = true;
        }

        private bool canCancel()
        { return installRunning; }

        private bool installOk = false;
        private async void beginInstalCmd()
        {
            installOk = await installPackage(fileName, blobName);
        }

        private bool canOk()
        {
            return !installRunning;
        }

        private bool _installRunning = false;
        private bool installRunning
        {
            get { return _installRunning; }
            set
            {
                _installRunning = value;
                CancelCommand.RaiseCanExecuteChanged();
                OkCommand.RaiseCanExecuteChanged();
                if (_installRunning) lableVisibility = Visibility.Visible;
                else lableVisibility = Visibility.Collapsed;
            }
        }
        private bool isCancelled = false;
        private async Task<bool> installPackage(string fileName, string blobName)
        {
            string tempFolder = Path.GetTempPath();
            tempFolder += "udtsetup";
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);
            string msiFile = tempFolder + "\\" + fileName;

            int exitVal = 0;
            //if (Directory.GetFiles(tempFolder, fileName).ToList().Count <= 0)
            if (true)
            {
                long length = 0;
                long downloadCount = 0;
                installRunning = true;
                //new Thread(() => BlobLoader.downloadFile(
                //   blobName, msiFile,
                //   (rv) => exitVal = rv,
                //   (cnt, sz) => { downloadCount = cnt; length = sz; }
                //   )).Start();
                BlobLoader.downloadFile(
                   blobName, msiFile,
                   (rv) => exitVal = rv,
                   (cnt, sz) => { downloadCount = cnt; length = sz; },
                   () => { return isCancelled; }
                   );

                while (exitVal == 0)
                {
                    if (loadCountTxtBlock.Foreground == Brushes.DarkBlue)
                        loadCountTxtBlock.Foreground = Brushes.DarkGreen;
                    else
                        loadCountTxtBlock.Foreground = Brushes.DarkBlue;

                    await Task.Delay(1000);
                    loadCountTxtBlock.Text = String.Format("{0:n0} of {1:n0} bytes", downloadCount, length);
                    loadCountTxtBlock.UpdateLayout();
                    //if (isCancelled) break;
                }
            }
            installRunning = false;
            if(isCancelled)
            {
                loadCountTxtBlock.Text = "Download Cancelled!";
            }
            if (exitVal < 0 ) return false;

            System.Diagnostics.Process installerProcess;
            //installerProcess = System.Diagnostics.Process.Start(msiFile, "/q");
            installerProcess = System.Diagnostics.Process.Start(msiFile);
            while (installerProcess.HasExited == false)
            {
                await Task.Delay(1000);
            }
            if (installerProcess.ExitCode != 0)
            {
                MessageBox.Show("Installation app failed or was cancled", "Install Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }
    }
}
