using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace UDTApp.ViewModels
{
    public class NewProjectViewModel : ValidatableBindableBase
    {
        public DelegateCommand OkCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand<Window> WindowLoadedCommand { get; set; }


        public NewProjectViewModel()
        {
            OkCommand = new DelegateCommand(okCmd, canOk);
            CancelCommand = new DelegateCommand(cancelCmd);
            WindowLoadedCommand = new DelegateCommand<Window>(winLoaded);
        }

        private string _projectName = null;
        [Required(ErrorMessage = "Project Name is required.")]
        [StringLength(15, MinimumLength = 5, ErrorMessage = "Project Name must be between 5 and 15 characters.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Project Name can include only letter characters")]
        public string ProjectName 
        {
            get { return _projectName; }
            set 
            {
                SetProperty(ref _projectName, value);
                OkCommand.RaiseCanExecuteChanged();
            }
        }

        private void okCmd()
        {
            newPrjView.DialogResult = true;
            closeAction();            
        }

        private bool canOk()
        {
            return !HasErrors;
        }

        private void cancelCmd()
        {
            newPrjView.DialogResult = false;
            closeAction();
        }

        private Window newPrjView { get; set; }
        private Action closeAction { get; set; }
        private void winLoaded(Window window)
        {
            closeAction = new Action(window.Close);
            newPrjView = window;
            ProjectName = "";
        }
    }
}
